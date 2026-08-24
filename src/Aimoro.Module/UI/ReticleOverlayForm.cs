using Aimoro.App.Native;
using System.Drawing.Drawing2D;

namespace Aimoro.App.UI;

public sealed class ReticleOverlayForm : Form
{
    private const int OverlayPadding = 8;
    private AppSettings _settings = new();
    private Screen _targetScreen = Screen.PrimaryScreen ?? Screen.AllScreens.First();

    public ReticleOverlayForm()
    {
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        TopMost = true;
        DoubleBuffered = true;
        BackColor = Color.Magenta;
        TransparencyKey = Color.Magenta;

        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.UserPaint,
            true);

        UpdateOverlayBounds();
    }

    protected override bool ShowWithoutActivation => true;

    protected override CreateParams CreateParams
    {
        get
        {
            const int wsExLayered = 0x00080000;
            const int wsExTransparent = 0x00000020;
            const int wsExToolWindow = 0x00000080;
            const int wsExNoActivate = 0x08000000;

            var parameters = base.CreateParams;
            parameters.ExStyle |= wsExLayered | wsExTransparent | wsExToolWindow | wsExNoActivate;
            return parameters;
        }
    }

    public void ApplySettings(AppSettings settings)
    {
        _settings = settings.Clone();
        _settings.Normalize();
        Opacity = _settings.ReticleOpacity / 255d;
        UpdateOverlayBounds();
        Invalidate();
    }

    public void SetTargetScreen(Screen screen)
    {
        _targetScreen = screen;
        UpdateOverlayBounds();
        Invalidate();
    }

    private void UpdateOverlayBounds()
    {
        var scale = (double)_settings.ReticleScale;
        var outlineThickness = (_settings.ReticleThickness + 2d) * scale;
        var maxCrosshairExtent = ((_settings.ReticleGap + _settings.ReticleLength) * scale) + scale + (outlineThickness / 2d);
        var centerDotExtent = _settings.ShowCenterDot
            ? ((_settings.CenterDotSize * scale) / 2d) + scale
            : 0;

        var radius = (int)Math.Ceiling(Math.Max(maxCrosshairExtent, centerDotExtent)) + OverlayPadding;
        var size = (radius * 2) + 1;
        var screenBounds = _targetScreen.Bounds;
        var centerX = screenBounds.Left + (screenBounds.Width / 2);
        var centerY = screenBounds.Top + (screenBounds.Height / 2);
        var newBounds = new Rectangle(centerX - radius, centerY - radius, size, size);

        if (Bounds != newBounds)
        {
            Bounds = newBounds;
        }
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(TransparencyKey);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        // A color-keyed transparent window cannot preserve partially transparent
        // anti-aliased pixels. Crisp rendering keeps the transparency key from
        // bleeding into the visible outline.
        e.Graphics.SmoothingMode = SmoothingMode.None;
        e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;

        var center = new PointF(ClientSize.Width / 2f, ClientSize.Height / 2f);
        var scale = (float)_settings.ReticleScale;
        var thickness = _settings.ReticleThickness * scale;
        using var outlinePen = CreateReticlePen(_settings.GetReticleOutlineColor(), thickness + (2f * scale));
        using var fillPen = CreateReticlePen(_settings.GetReticleColor(), thickness);

        var length = _settings.ReticleLength * scale;
        var gap = _settings.ReticleGap * scale;

        DrawCrosshair(e.Graphics, outlinePen, center, length, gap, scale);
        DrawCrosshair(e.Graphics, fillPen, center, length, gap);

        if (_settings.ShowCenterDot)
        {
            var size = _settings.CenterDotSize * scale;
            var outlineSize = size + (2f * scale);
            using var outlineBrush = new SolidBrush(_settings.GetReticleOutlineColor());
            using var fillBrush = new SolidBrush(_settings.GetReticleColor());
            e.Graphics.FillRectangle(outlineBrush, CenteredRectangle(center, outlineSize));
            e.Graphics.FillRectangle(fillBrush, CenteredRectangle(center, size));
        }
    }

    private static Pen CreateReticlePen(Color color, float width)
    {
        return new Pen(color, width)
        {
            StartCap = LineCap.Flat,
            EndCap = LineCap.Flat
        };
    }

    private static void DrawCrosshair(
        Graphics graphics,
        Pen pen,
        PointF center,
        float length,
        float gap,
        float endPadding = 0f)
    {
        graphics.DrawLine(
            pen,
            center.X - gap - length - endPadding,
            center.Y,
            center.X - gap + endPadding,
            center.Y);

        graphics.DrawLine(
            pen,
            center.X + gap - endPadding,
            center.Y,
            center.X + gap + length + endPadding,
            center.Y);

        graphics.DrawLine(
            pen,
            center.X,
            center.Y - gap - length - endPadding,
            center.X,
            center.Y - gap + endPadding);

        graphics.DrawLine(
            pen,
            center.X,
            center.Y + gap - endPadding,
            center.X,
            center.Y + gap + length + endPadding);
    }

    private static RectangleF CenteredRectangle(PointF center, float size)
    {
        return new RectangleF(center.X - (size / 2f), center.Y - (size / 2f), size, size);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == NativeMethods.WM_MOUSEACTIVATE)
        {
            m.Result = (IntPtr)NativeMethods.MA_NOACTIVATE;
            return;
        }

        base.WndProc(ref m);
    }
}

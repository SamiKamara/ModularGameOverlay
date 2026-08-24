namespace ModularGameOverlay.App.UI;

internal sealed class DarkToolStripRenderer : ToolStripProfessionalRenderer
{
    public DarkToolStripRenderer()
        : base(new DarkToolStripColorTable())
    {
        RoundedEdges = false;
    }

    internal Color CheckAreaBackground => DarkTheme.Raised;

    protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs eventArgs)
    {
        var available = eventArgs.ImageRectangle;
        if (available.Width <= 0 || available.Height <= 0)
        {
            available = new Rectangle(4, 2, 18, Math.Max(1, eventArgs.Item.Height - 4));
        }

        var glyphSize = Math.Max(10, Math.Min(14, Math.Min(available.Width, available.Height) - 2));
        var glyph = new Rectangle(
            available.Left + ((available.Width - glyphSize) / 2),
            available.Top + ((available.Height - glyphSize) / 2),
            glyphSize,
            glyphSize);

        using (var background = new SolidBrush(CheckAreaBackground))
        using (var border = new Pen(DarkTheme.Border))
        {
            eventArgs.Graphics.FillRectangle(background, glyph);
            eventArgs.Graphics.DrawRectangle(border, glyph);
        }

        if (eventArgs.Item is not ToolStripMenuItem { Checked: true })
        {
            return;
        }

        var oldSmoothingMode = eventArgs.Graphics.SmoothingMode;
        eventArgs.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using var check = new Pen(DarkTheme.Accent, 2f)
        {
            StartCap = System.Drawing.Drawing2D.LineCap.Round,
            EndCap = System.Drawing.Drawing2D.LineCap.Round,
            LineJoin = System.Drawing.Drawing2D.LineJoin.Round
        };
        eventArgs.Graphics.DrawLines(check,
        [
            new Point(glyph.Left + (glyph.Width / 5), glyph.Top + (glyph.Height / 2)),
            new Point(glyph.Left + (glyph.Width * 2 / 5), glyph.Bottom - (glyph.Height / 4)),
            new Point(glyph.Right - (glyph.Width / 6), glyph.Top + (glyph.Height / 4))
        ]);
        eventArgs.Graphics.SmoothingMode = oldSmoothingMode;
    }

    private sealed class DarkToolStripColorTable : ProfessionalColorTable
    {
        public override Color ToolStripDropDownBackground => DarkTheme.Card;
        public override Color ImageMarginGradientBegin => DarkTheme.Card;
        public override Color ImageMarginGradientMiddle => DarkTheme.Card;
        public override Color ImageMarginGradientEnd => DarkTheme.Card;
        public override Color CheckBackground => DarkTheme.Raised;
        public override Color CheckPressedBackground => DarkTheme.Raised;
        public override Color CheckSelectedBackground => DarkTheme.Raised;
        public override Color MenuBorder => DarkTheme.Border;
        public override Color MenuItemBorder => DarkTheme.Border;
        public override Color MenuItemSelected => Color.FromArgb(46, 54, 68);
        public override Color MenuItemSelectedGradientBegin => Color.FromArgb(46, 54, 68);
        public override Color MenuItemSelectedGradientEnd => Color.FromArgb(46, 54, 68);
        public override Color MenuItemPressedGradientBegin => DarkTheme.Raised;
        public override Color MenuItemPressedGradientMiddle => DarkTheme.Raised;
        public override Color MenuItemPressedGradientEnd => DarkTheme.Raised;
        public override Color SeparatorDark => DarkTheme.Border;
        public override Color SeparatorLight => DarkTheme.Border;
    }
}

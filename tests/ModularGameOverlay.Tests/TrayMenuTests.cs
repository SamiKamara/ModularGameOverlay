using ModularGameOverlay.App;
using ModularGameOverlay.App.UI;

namespace ModularGameOverlay.Tests;

public sealed class TrayMenuTests
{
    [Fact]
    public void TrayMenuUsesDarkRenderingForCheckedItemsAndNestedMenus()
    {
        RunSta(() =>
        {
            using var menu = new ContextMenuStrip();
            var checkedItem = new ToolStripMenuItem("Enabled") { Checked = true };
            var parent = new ToolStripMenuItem("Detailed settings");
            parent.DropDownItems.Add("SuperLighter...");
            menu.Items.Add(checkedItem);
            menu.Items.Add(parent);

            ModularGameOverlayApplicationContext.ConfigureTrayMenu(menu);

            var renderer = Assert.IsType<DarkToolStripRenderer>(menu.Renderer);
            Assert.Equal(DarkTheme.Card, menu.BackColor);
            Assert.Equal(DarkTheme.Text, checkedItem.ForeColor);
            Assert.Equal(DarkTheme.Raised, renderer.CheckAreaBackground);
            Assert.Equal(DarkTheme.Raised, renderer.ColorTable.CheckBackground);
            Assert.Same(renderer, parent.DropDown.Renderer);
            Assert.Equal(DarkTheme.Card, parent.DropDown.BackColor);
            Assert.All(parent.DropDownItems.Cast<ToolStripItem>(), item =>
                Assert.Equal(DarkTheme.Text, item.ForeColor));
        });
    }

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        using var finished = new ManualResetEventSlim();
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
            finally
            {
                finished.Set();
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(finished.Wait(TimeSpan.FromSeconds(20)), "STA test timed out.");
        thread.Join();
        if (failure is not null) throw failure;
    }
}

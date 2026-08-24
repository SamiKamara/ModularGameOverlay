using ModularGameOverlay.App.Hotkeys;
using ModularGameOverlay.App.Settings;
using ModularGameOverlay.App.UI;

namespace ModularGameOverlay.Tests;

public sealed class MainFormTests
{
    [Fact]
    public void MainFormExposesAllRequiredControlsAndDefaultStates()
    {
        var state = RunSta(() =>
        {
            var settings = ModularGameOverlaySettings.CreateDefaults();
            using var form = new MainForm(
                settings,
                _ => { },
                _ => { },
                _ => { },
                () => { },
                () => { },
                () => { },
                () => { },
                _ => null);
            _ = form.Handle;
            return form.GetStateForTests();
        });

        Assert.False(state.LightEnabled);
        Assert.True(state.AimoroEnabled);
        Assert.True(state.SoundEnabled);
        Assert.Equal("Ctrl+Alt+B", state.LightHotkey.ToDisplayString());
        Assert.Equal(3, state.ButtonLabels.Count(label => label == "Detailed settings..."));
        Assert.Contains("All global hotkeys...", state.ButtonLabels);
        Assert.True(state.VisibleContentControlCount >= 20);
    }

    [Fact]
    public void ImportedSuperLighterSelfTestStillPasses()
    {
        Assert.True(RunSta(SuperLighter.App.Services.SelfTests.Run));
    }

    [Fact]
    public void CentralHotkeyWindowContainsAllEightBindings()
    {
        var count = RunSta(() =>
        {
            using var form = new HotkeysForm(HotkeyConfiguration.CreateDefaults());
            _ = form.Handle;
            return form.BindingFieldCount;
        });

        Assert.Equal(8, count);
    }

    private static T RunSta<T>(Func<T> action)
    {
        T? result = default;
        Exception? failure = null;
        using var finished = new ManualResetEventSlim();
        var thread = new Thread(() =>
        {
            try
            {
                result = action();
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
        return result!;
    }
}

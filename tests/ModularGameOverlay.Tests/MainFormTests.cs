using ModularGameOverlay.App.Hotkeys;
using ModularGameOverlay.App.Settings;
using ModularGameOverlay.App.UI;
using AimoroSettingsForm = Aimoro.App.UI.SettingsForm;
using SoundSettingsForm = SoundDirectionVisualizer.App.UI.SettingsForm;
using SuperSettingsForm = SuperLighter.App.UI.SettingsForm;

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
            form.Show();
            Application.DoEvents();
            return form.GetStateForTests();
        });

        Assert.False(state.LightEnabled);
        Assert.True(state.AimoroEnabled);
        Assert.True(state.SoundEnabled);
        Assert.Equal("Ctrl+Alt+B", state.LightHotkey.ToDisplayString());
        Assert.Equal(3, state.ButtonLabels.Count(label => label == "Detailed settings..."));
        Assert.Contains("All global hotkeys...", state.ButtonLabels);
        Assert.True(state.VisibleContentControlCount >= 20);
        Assert.Contains("Modules:", state.LabelTexts);
        Assert.Contains("Toggle Light Enhancement hotkey", state.LabelTexts);
        Assert.DoesNotContain("One control surface for your game overlays", state.LabelTexts);
        Assert.DoesNotContain("ModularGameOverlay", state.LabelTexts);
        Assert.Equal(DarkTheme.Raised, state.GlobalHotkeysButtonBackColor);

        var detailedRightEdges = state.DetailedSettingsButtonBounds
            .Select(bounds => bounds.Right)
            .Distinct()
            .ToArray();
        Assert.Single(detailedRightEdges);
        Assert.Equal(detailedRightEdges[0], state.LightHotkeyBounds.Right);
        Assert.Equal(detailedRightEdges[0], state.GlobalHotkeysButtonBounds.Right);
    }

    [Fact]
    public void ImportedSuperLighterSelfTestStillPasses()
    {
        Assert.True(RunSta(SuperLighter.App.Services.SelfTests.Run));
    }

    [Fact]
    public void CentralHotkeyWindowContainsAllEightBindings()
    {
        var state = RunSta(() =>
        {
            using var form = new HotkeysForm(HotkeyConfiguration.CreateDefaults());
            _ = form.Handle;
            return (form.BindingFieldCount, form.BindingTextBounds, form.Text, form.Icon is not null);
        });

        Assert.Equal(8, state.BindingFieldCount);
        Assert.Equal("ModularGameOverlay - Global Hotkeys", state.Text);
        Assert.True(state.Item4);
        Assert.All(state.BindingTextBounds, bounds =>
        {
            var clientCenter = bounds.ClientBounds.Top + (bounds.ClientBounds.Height / 2d);
            var textCenter = bounds.TextBounds.Top + (bounds.TextBounds.Height / 2d);
            Assert.InRange(Math.Abs(clientCenter - textCenter), 0d, 1d);
        });
    }

    [Fact]
    public void AllSettingsWindowsUseUnifiedTitlesAndAnApplicationIcon()
    {
        var windows = RunSta(() =>
        {
            var settings = ModularGameOverlaySettings.CreateDefaults();
            using var main = new MainForm(
                settings,
                _ => { },
                _ => { },
                _ => { },
                () => { },
                () => { },
                () => { },
                () => { },
                _ => null);
            using var hotkeys = new HotkeysForm(settings.Hotkeys);
            using var super = new SuperSettingsForm(new SuperLighter.App.AppSettings(), _ => { });
            using var aimoro = new AimoroSettingsForm(new Aimoro.App.AppSettings());
            using var sound = new SoundSettingsForm(new SoundDirectionVisualizer.App.AppSettings());

            return new[]
            {
                (main.Text, main.Icon is not null),
                (hotkeys.Text, hotkeys.Icon is not null),
                (super.Text, super.Icon is not null),
                (aimoro.Text, aimoro.Icon is not null),
                (sound.Text, sound.Icon is not null)
            };
        });

        Assert.Equal(
        [
            "ModularGameOverlay",
            "ModularGameOverlay - Global Hotkeys",
            "ModularGameOverlay - SuperLighter",
            "ModularGameOverlay - Aimoro",
            "ModularGameOverlay - Sound Direction Visualizer"
        ],
            windows.Select(window => window.Text));
        Assert.All(windows, window => Assert.True(window.Item2));
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

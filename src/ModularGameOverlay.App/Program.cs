namespace ModularGameOverlay.App;

internal static class Program
{
    private const string SingleInstanceMutexName = @"Local\ModularGameOverlay.SingleInstance";
    private const string OpenMainEventName = @"Local\ModularGameOverlay.OpenMain";

    [STAThread]
    private static int Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        if (args.Contains("--self-test", StringComparer.OrdinalIgnoreCase))
        {
            return SuperLighter.App.Services.SelfTests.Run() ? 0 : 1;
        }

        using var mutex = new Mutex(true, SingleInstanceMutexName, out var createdNew);
        if (!createdNew)
        {
            SignalOpenMain();
            return 0;
        }

        using var openMainEvent = new EventWaitHandle(false, EventResetMode.AutoReset, OpenMainEventName);
        using var context = new ModularGameOverlayApplicationContext(openMainEvent);
        EventHandler processExit = (_, _) => context.RestoreDisplayEffects();
        AppDomain.CurrentDomain.ProcessExit += processExit;
        Application.ThreadException += (_, eventArgs) =>
        {
            context.RestoreDisplayEffects();
            MessageBox.Show(
                $"ModularGameOverlay encountered an error and restored display effects.\n\n{eventArgs.Exception.Message}",
                "ModularGameOverlay",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            context.ExitApplication();
        };

        try
        {
            Application.Run(context);
            return 0;
        }
        finally
        {
            context.RestoreDisplayEffects();
            AppDomain.CurrentDomain.ProcessExit -= processExit;
        }
    }

    private static void SignalOpenMain()
    {
        try
        {
            using var signal = EventWaitHandle.OpenExisting(OpenMainEventName);
            signal.Set();
        }
        catch (WaitHandleCannotBeOpenedException)
        {
        }
    }
}

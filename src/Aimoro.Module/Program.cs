namespace Aimoro.App;

internal static class Program
{
    private const string SingleInstanceMutexName = @"Local\Aimoro.SingleInstance";
    private const string OpenSettingsEventName = @"Local\Aimoro.OpenSettings";

    [STAThread]
    private static void Main()
    {
        using var singleInstanceMutex = new Mutex(true, SingleInstanceMutexName, out var createdNew);
        if (!createdNew)
        {
            SignalOpenSettings();
            return;
        }

        ApplicationConfiguration.Initialize();
        using var openSettingsEvent = new EventWaitHandle(false, EventResetMode.AutoReset, OpenSettingsEventName);
        Application.Run(new AimoroApplicationContext(openSettingsEvent, openSettingsOnStartup: true));
    }

    private static void SignalOpenSettings()
    {
        try
        {
            using var openSettingsEvent = EventWaitHandle.OpenExisting(OpenSettingsEventName);
            openSettingsEvent.Set();
        }
        catch (WaitHandleCannotBeOpenedException)
        {
        }
    }
}

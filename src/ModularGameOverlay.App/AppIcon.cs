namespace ModularGameOverlay.App;

internal static class AppIcon
{
    public static Icon Load()
    {
        try
        {
            var extracted = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            if (extracted is not null)
            {
                return (Icon)extracted.Clone();
            }
        }
        catch
        {
        }

        return (Icon)SystemIcons.Application.Clone();
    }
}

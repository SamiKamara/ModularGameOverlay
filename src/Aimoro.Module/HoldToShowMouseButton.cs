namespace Aimoro.App;

public enum HoldToShowMouseButton
{
    LeftButton,
    RightButton,
    MiddleButton,
    XButton1,
    XButton2
}

public static class HoldToShowMouseButtonExtensions
{
    public static string ToDisplayString(this HoldToShowMouseButton mouseButton)
    {
        return mouseButton switch
        {
            HoldToShowMouseButton.LeftButton => "Left mouse button",
            HoldToShowMouseButton.RightButton => "Right mouse button",
            HoldToShowMouseButton.MiddleButton => "Middle mouse button",
            HoldToShowMouseButton.XButton1 => "Mouse button 4",
            HoldToShowMouseButton.XButton2 => "Mouse button 5",
            _ => mouseButton.ToString()
        };
    }
}

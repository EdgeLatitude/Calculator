using System;

namespace Calculator.Shared.Theming
{
    public class Theming
    {
        public static event EventHandler<ThemeChangeNeededEventArgs> ThemeChangeNeeded;

        public static void ThemeChangeNeeded_Event(object sender, ThemeChangeNeededEventArgs theme)
        {
            ThemeChangeNeeded?.Invoke(sender, theme);
        }
    }
}

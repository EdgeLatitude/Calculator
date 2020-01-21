using System;

namespace Calculator.Shared.Theming
{
    public class ThemeChangeNeededEventArgs : EventArgs
    {
        public Theme Theme { get; set; }
    }
}

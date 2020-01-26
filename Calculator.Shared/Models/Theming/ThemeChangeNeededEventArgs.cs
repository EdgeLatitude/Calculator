using System;

namespace Calculator.Shared.Models.Theming
{
    public class ThemeChangeNeededEventArgs : EventArgs
    {
        public Theme Theme { get; set; }
    }
}

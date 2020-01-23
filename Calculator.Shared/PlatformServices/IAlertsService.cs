﻿using System.Threading.Tasks;

namespace Calculator.Shared.PlatformServices
{
    interface IAlertsService
    {
        Task DisplayAlertAsync(string title, string message);
        Task<bool> DisplayConfirmationAsync(string title, string message);
        Task<string> DisplayOptionsAsync(string title, string destruction, params string[] options);
    }
}

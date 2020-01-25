using System;

namespace Calculator.Shared.PlatformServices
{
    public interface IUiThreadService
    {
        void ExecuteOnUiThread(Action action);
    }
}

using Xamarin.Forms;

namespace Calculator.Mobile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent(); 
            MainPage = new AppShell();
        }
    }
}

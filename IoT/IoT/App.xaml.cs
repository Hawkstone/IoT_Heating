using System;
using System.Diagnostics;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace IoT
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override void OnStart()
        {
            // handle when your app starts
        }

        protected override void OnSleep()
        {
            // handle when your app sleeps
        }

        protected override void OnResume()
        {
            // handle when your app resumes
        }
    }
}

using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using CounterHayFever.Views;
using CounterHayFever.Models;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace CounterHayFever
{
    /// <summary>
    /// The starting point for the application
    /// </summary>
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            // Set main page of the app to Start Page and set the base URL of the web service.
            MainPage = new StartPage();
            Current.Properties["ServiceURL"] = "<YOUR_AZURE_ENDPOINT>";
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override async void OnResume()
        {
            // Handle when your app resumes
            await Data.GetCurrentLocationAsync();
        }
    }
}

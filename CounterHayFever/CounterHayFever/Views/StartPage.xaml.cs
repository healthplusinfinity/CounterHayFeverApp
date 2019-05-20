using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CounterHayFever.Models;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace CounterHayFever.Views
{
    /// <summary>
    /// The first page that shows up.
    /// </summary>
    public partial class StartPage : ContentPage
    {
        public StartPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Check for network connectivity
            var current = Connectivity.NetworkAccess;
            bool hasInternetConnectivity = current == NetworkAccess.Internet;

            // Check if location permission is given. If not, request for it.
            var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);
            if (status != PermissionStatus.Granted)
            {
                if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Location))
                {
                    await DisplayAlert("Location permission needed", "Please provide location permission to proceed.", "OK");
                }

                var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Location });
                status = results[Permission.Location];
            }

            bool hasLocationPermission = status == PermissionStatus.Granted;

            if (hasInternetConnectivity && hasLocationPermission)
            {
                // All permissions obtained, start a task to fetch suburb data
                // for use later in the application.
                Data.StartSuburbDataTask();

                // All permissions obtained, get current location and related weather data
                await Data.GetCurrentLocationAsync();
                double latitude = Data.CurrentLocation.Latitude;
                double longitude = Data.CurrentLocation.Longitude;
                WeatherModel weather = await Data.GetCurrentWeatherConditionsAsync(latitude, longitude);

                indicator.IsRunning = false;

                Application.Current.MainPage = new NavigationPage(new LandingPage(weather));
            }
            else
            {
                // If either permissions are not present, show an error.
                Application.Current.MainPage = new ErrorPage();
            }
        }
    }
}

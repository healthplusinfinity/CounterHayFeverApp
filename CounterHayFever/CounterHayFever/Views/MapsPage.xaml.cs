using System;
using System.Collections.Generic;
using CounterHayFever.Utils;
using Xamarin.Forms;

namespace CounterHayFever.Views
{
    /// <summary>
    /// Page to configure home and work locations for periodic notifications.
    /// </summary>
    public partial class MapsPage : ContentPage
    {
        /// <summary>
        /// The search bar for home location.
        /// </summary>
        private SearchBarUtil homeSearchBar;

        /// <summary>
        /// The search bar for work location.
        /// </summary>
        private SearchBarUtil workSearchBar;

        public MapsPage()
        {
            InitializeComponent();
            homeSearchBar = new SearchBarUtil(searchBarHome, homeSuggestionsList);
            workSearchBar = new SearchBarUtil(searchBarWork, workSuggestionsList);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (Application.Current.Properties.ContainsKey("HomeLocality"))
            {
                searchBarHome.Text = Application.Current.Properties["HomeLocality"].ToString();
            }
            if (Application.Current.Properties.ContainsKey("WorkLocality"))
            {
                searchBarWork.Text = Application.Current.Properties["WorkLocality"].ToString();
            }
            homeSearchBar.RegisterHandlers();
            workSearchBar.RegisterHandlers();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            homeSearchBar.DeregisterHandlers();
            workSearchBar.DeregisterHandlers();
        }

        /// <summary>
        /// Handler for the Save button. When clicked, shows and schedules notifications
        /// via the platform specific implementation through the MessagingCenter.
        /// </summary>
        /// <param name="sender">Save button.</param>
        /// <param name="e">E.</param>
        async void Handle_Clicked(object sender, System.EventArgs e)
        {
            Application.Current.Properties["HomeLocality"] = searchBarHome.Text ?? string.Empty;
            Application.Current.Properties["WorkLocality"] = searchBarWork.Text ?? string.Empty;
            await Application.Current.SavePropertiesAsync();
            await DisplayAlert("Success", "Successfully saved details", "OK");
            MessagingCenter.Send(new NotificationMessage(), "setupNotification");
        }
    }
}

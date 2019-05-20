using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CounterHayFever.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CounterHayFever.Views
{
    /// <summary>
    /// The first page that's seen after the app has checked for permissions
    /// and obtained user's current location
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LandingPage : TabbedPage
    {
        public LandingPage(WeatherModel weather)
        {
            InitializeComponent();
            var weatherPage = new WeatherPage(weather)
            {
                Title = "Risk Levels"
            };

            var recommendationPage = new ItemsPage
            {
                Title = "Symptoms recommendations"
            };

            // Add pages to the tabbed view
            Children.Add(weatherPage);
            Children.Add(recommendationPage);
        }

        /// <summary>
        /// Handler for the settings icon on the toolbar. Clicking 
        /// opens the home and work location setting page.
        /// </summary>
        /// <param name="sender">The settings icon.</param>
        /// <param name="e">E.</param>
        private async void Handle_Clicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new NavigationPage(new MapsPage()));
        }
    }
}

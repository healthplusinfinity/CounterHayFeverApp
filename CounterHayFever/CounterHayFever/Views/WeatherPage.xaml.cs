using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CounterHayFever.Models;
using CounterHayFever.Utils;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace CounterHayFever.Views
{
    public partial class WeatherPage : ContentPage
    {
        #region Constants
        #region Bounds for weather parameters
        private const double WINDSPEED_LOWER_BOUND = 20;

        private const double WINDSPEED_UPPER_BOUND = 35;

        private const double TEMP_LOWER_BOUND = 20;

        private const double TEMP_UPPER_BOUND = 27;

        private const double HUMIDITY_LOWER_BOUND = 71;

        private const double HUMIDITY_UPPER_BOUND = 31;
        #endregion

        /// <summary>
        /// Opacity value for unselected rating icons.
        /// </summary>
        private const double RATING_UNSELECTED_OPACITY = 0.25;

        /// <summary>
        /// Opacity value for selected rating icons.
        /// </summary>
        private const double RATING_SELECTED_OPACITY = 1;
        #endregion

        // The reference to the search bar utility to handle dropdowns.
        private SearchBarUtil searchBarUtil;

        /// <summary>
        /// The weather for the user's location.
        /// </summary>
        private WeatherModel weather;
         
        public WeatherPage(WeatherModel weather)
        {
            InitializeComponent();
            this.weather = weather;
            searchBarUtil = new SearchBarUtil(searchBar, suggestionsList, UpdateWeatherPage);

            #region Setup the rating images
            image1.Source = ImageSource.FromResource("CounterHayFever.Images.leaf.png");
            image2.Source = ImageSource.FromResource("CounterHayFever.Images.leaf.png");
            image3.Source = ImageSource.FromResource("CounterHayFever.Images.leaf.png");
            image4.Source = ImageSource.FromResource("CounterHayFever.Images.leaf.png");
            image5.Source = ImageSource.FromResource("CounterHayFever.Images.leaf.png");

            TapGestureRecognizer imageTapHandler = new TapGestureRecognizer();
            imageTapHandler.Tapped += (sender, e) =>
            {
                OnTap((Image)sender);
            };
            image1.Opacity = RATING_UNSELECTED_OPACITY;
            image2.Opacity = RATING_UNSELECTED_OPACITY;
            image3.Opacity = RATING_UNSELECTED_OPACITY;
            image4.Opacity = RATING_UNSELECTED_OPACITY;
            image5.Opacity = RATING_UNSELECTED_OPACITY;
            SetupRatingImages(imageTapHandler);

            #endregion
        }

        /// <summary>
        /// Setups the rating images and event handlers such that
        /// a user may only rate once a day.
        /// </summary>
        /// <param name="tapHandler">Event handler for taps.</param>
        private void SetupRatingImages(TapGestureRecognizer tapHandler)
        {

            if (IsRatingEnabled())
            {
                if (Application.Current.Properties.ContainsKey("DateRated"))
                {
                    Application.Current.Properties["UserRating"] = 0;

                    if (Application.Current.Properties.ContainsKey("LocationRated"))
                    {
                        string locality = Application.Current.Properties["LocationRated"].ToString();
                        lastRated.Text = $"You last rated {locality} at {Application.Current.Properties["DateRated"].ToString()}.";
                    }
                    else
                    {
                        lastRated.Text = $"You last rated at {Application.Current.Properties["DateRated"].ToString()}.";
                    }
                    lastRated.IsVisible = true;
                }
                image1.GestureRecognizers.Add(tapHandler);
                image2.GestureRecognizers.Add(tapHandler);
                image3.GestureRecognizers.Add(tapHandler);
                image4.GestureRecognizers.Add(tapHandler);
                image5.GestureRecognizers.Add(tapHandler);
            }
            else
            {
                if (Application.Current.Properties.ContainsKey("LocationRated"))
                {
                    string locality = Application.Current.Properties["LocationRated"].ToString();
                    lastRated.Text = $"You last rated {locality} at {Application.Current.Properties["DateRated"].ToString()}. " +
                    $"You may next rate after {GetNextRefreshTimeAsString()}";
                }
                else
                {
                    lastRated.Text = $"You last rated at {Application.Current.Properties["DateRated"].ToString()}.";
                }

                lastRated.IsVisible = true;
                int lastRating = Convert.ToInt32(Application.Current.Properties["UserRating"]);
                switch (lastRating)
                {
                    case 1:
                        image1.Opacity = RATING_SELECTED_OPACITY;
                        break;

                    case 2:
                        image1.Opacity = RATING_SELECTED_OPACITY;
                        image2.Opacity = RATING_SELECTED_OPACITY;
                        break;

                    case 3:
                        image1.Opacity = RATING_SELECTED_OPACITY;
                        image2.Opacity = RATING_SELECTED_OPACITY;
                        image3.Opacity = RATING_SELECTED_OPACITY;
                        break;

                    case 4:
                        image1.Opacity = RATING_SELECTED_OPACITY;
                        image2.Opacity = RATING_SELECTED_OPACITY;
                        image3.Opacity = RATING_SELECTED_OPACITY;
                        image4.Opacity = RATING_SELECTED_OPACITY;
                        break;
                    case 5:
                        image1.Opacity = RATING_SELECTED_OPACITY;
                        image2.Opacity = RATING_SELECTED_OPACITY;
                        image3.Opacity = RATING_SELECTED_OPACITY;
                        image4.Opacity = RATING_SELECTED_OPACITY;
                        image5.Opacity = RATING_SELECTED_OPACITY;
                        break;
                }
            }
        }

        protected override async void OnAppearing()
        {
            // Initiate the app to show user's location weather.
            #region Populate search bar
            double lat = Data.CurrentLocation.Latitude;
            double lon = Data.CurrentLocation.Longitude;
            var placemarks = await Geocoding.GetPlacemarksAsync(lat, lon);
            var placemark = placemarks?.FirstOrDefault();
            searchBar.Text = $"{placemark.Locality}";
            searchBarUtil.RegisterHandlers();
            #endregion

            ProcessWeatherData(weather);

            #region Ratings from other Users
            UserRatingsModel userRatings = await Data.GetRiskLevelsForSuburbAsync(Data.CurrentLocality);
            ProcessUserRatingsForSuburb(userRatings);
            #endregion
        }

        /// <summary>
        /// Process and display user ratings for suburb.
        /// </summary>
        /// <param name="userRatings">User ratings fetched from the service.</param>
        private void ProcessUserRatingsForSuburb(UserRatingsModel userRatings)
        {
            if (userRatings != null)
            {
                if (userRatings.AverageRating > 0)
                {
                    personalisedRisk.IsVisible = true;
                    numberOfUsersLabel.IsVisible = true;
                    riskHint.Text = $"Based on users' ratings, the risk level in {userRatings.Suburb} is:";
                    numberOfUsersLabel.Text = $"Based on {userRatings.RatingCount} user inputs.";
                    if (userRatings.AverageRating < 2)
                    {
                        personalisedRisk.Text = WeatherSeverity.Low.ToString();
                        personalisedRisk.TextColor = Color.Green;
                    }
                    else if (userRatings.AverageRating < 4)
                    {
                        personalisedRisk.Text = WeatherSeverity.Medium.ToString();
                        personalisedRisk.TextColor = Color.Orange;
                    }
                    else if (userRatings.AverageRating <= 5)
                    {
                        personalisedRisk.Text = WeatherSeverity.High.ToString();
                        personalisedRisk.TextColor = Color.Red;
                    }
                }
                else
                {
                    riskHint.Text = $"There is currently no rating available for {userRatings.Suburb}.";
                    personalisedRisk.IsVisible = false;
                    numberOfUsersLabel.IsVisible = false;
                }
            }
            else
            {
                riskHint.Text = $"Unable to fetch user ratings";
            }
            personalizedRatingValidity.Text = $"Last updated at: {DateTime.Now.ToString()}";
        }

        protected override void OnDisappearing()
        {
            // Deregister event handlers for the search bar.
            searchBarUtil.DeregisterHandlers();
        }

        /// <summary>
        /// Set the weather parameter values on the UI.
        /// </summary>
        /// <param name="weatherData">Weather data.</param>
        private void ProcessWeatherData(WeatherModel weatherData)
        {
            #region Process Temperature
            tempLabel.Text = $"{weatherData.Temperature}°C";

            if (weatherData.Temperature <= TEMP_LOWER_BOUND)
            {
                TempFrame.BackgroundColor = Color.Green;
                TempFrame.BorderColor = Color.Green;
            }
            else if (weatherData.Temperature > TEMP_LOWER_BOUND && weatherData.Temperature <= TEMP_UPPER_BOUND)
            {
                TempFrame.BackgroundColor = Color.Orange;
                TempFrame.BorderColor = Color.Orange;
            }
            else
            {
                TempFrame.BorderColor = Color.Red;
                TempFrame.BackgroundColor = Color.Red;
            }
            #endregion

            #region Process Wind speed
            windspeedLabel.Text = $"{weatherData.WindSpeed} km/h";

            if (weatherData.WindSpeed <= WINDSPEED_LOWER_BOUND)
            {
                WindspeedFrame.BackgroundColor = Color.Green;
                WindspeedFrame.BorderColor = Color.Green;
            }
            else if (weatherData.WindSpeed > WINDSPEED_LOWER_BOUND && weatherData.WindSpeed <= WINDSPEED_UPPER_BOUND)
            {
                WindspeedFrame.BackgroundColor = Color.Orange;
                WindspeedFrame.BorderColor = Color.Orange;
            }
            else
            {
                WindspeedFrame.BorderColor = Color.Red;
                WindspeedFrame.BackgroundColor = Color.Red;
            }
            #endregion

            #region Process pressure
            pressureLabel.Text = $"{weatherData.Pressure} hPa";
            PressureFrame.BorderColor = Color.Green;
            PressureFrame.BackgroundColor = Color.Green;
            #endregion

            #region Process humidity
            humidityLabel.Text = $"{weatherData.Humidity}%";
            if (weatherData.Humidity >= HUMIDITY_LOWER_BOUND)
            {
                HumidityFrame.BackgroundColor = Color.Green;
                HumidityFrame.BorderColor = Color.Green;
            }
            else if (weatherData.Humidity < HUMIDITY_LOWER_BOUND && weatherData.Humidity >= HUMIDITY_UPPER_BOUND)
            {
                HumidityFrame.BackgroundColor = Color.Orange;
                HumidityFrame.BorderColor = Color.Orange;
            }
            else
            {
                HumidityFrame.BorderColor = Color.Red;
                HumidityFrame.BackgroundColor = Color.Red;
            }
            #endregion

            DateTime now = DateTime.Now;
            string day = now.DayOfWeek.ToString();
            date.Text = $"{day}, {now.ToShortDateString()} {now.ToShortTimeString()}";

            #region Process severity
            risk.Text = weatherData.Severity.ToString();
            if (weatherData.Severity == WeatherSeverity.Low)
            {
                risk.TextColor = Color.Green;
                image.Source = ImageSource.FromResource("CounterHayFever.Images.sentiment_satisfied.png");
            }
            else if (weatherData.Severity == WeatherSeverity.Medium)
            {
                risk.TextColor = Color.Orange;
                image.Source = ImageSource.FromResource("CounterHayFever.Images.sentiment_med_dissatisfied.png");
            }
            else
            {
                risk.TextColor = Color.Red;
                image.Source = ImageSource.FromResource("CounterHayFever.Images.sentiment_dissatisfied.png");
            }
            #endregion
        }

        /// <summary>
        /// Update the weather details, when a new location is selected.
        /// </summary>
        private async Task UpdateWeatherPage()
        {
            var locations = await Geocoding.GetLocationsAsync($"{searchBar.Text}, VIC, Australia");
            var location = locations?.FirstOrDefault();

            WeatherModel weatherModel = await Data.GetCurrentWeatherConditionsAsync(location.Latitude, location.Longitude);
            ProcessWeatherData(weatherModel);

            UserRatingsModel userRatings = await Data.GetRiskLevelsForSuburbAsync(searchBar.Text);
            ProcessUserRatingsForSuburb(userRatings);
        }

        /// <summary>
        /// Handler for the tap event.
        /// </summary>
        /// <param name="sender">The rating image that was tapped.</param>
        private async void OnTap(Image sender)
        {
            bool confirmation =await DisplayAlert("Please confirm...",
               $"Are you sure you want to submit your rating for {Data.CurrentLocality}?",
                "Yes",
                "No");
            if (!confirmation)
            {
                return;
            }
            DateTime now = DateTime.Now;
            int rating = 0;
            if (sender == image1)
            {
                rating = 1;
                image1.Opacity = RATING_SELECTED_OPACITY;
                image2.Opacity = RATING_UNSELECTED_OPACITY;
                image3.Opacity = RATING_UNSELECTED_OPACITY;
                image4.Opacity = RATING_UNSELECTED_OPACITY;
                image5.Opacity = RATING_UNSELECTED_OPACITY;
            }
            else if (sender == image2)
            {
                rating = 2;
                image1.Opacity = RATING_SELECTED_OPACITY;
                image2.Opacity = RATING_SELECTED_OPACITY;
                image3.Opacity = RATING_UNSELECTED_OPACITY;
                image4.Opacity = RATING_UNSELECTED_OPACITY;
                image5.Opacity = RATING_UNSELECTED_OPACITY;
            }
            else if (sender == image3)
            {
                rating = 3;
                image1.Opacity = RATING_SELECTED_OPACITY;
                image2.Opacity = RATING_SELECTED_OPACITY;
                image3.Opacity = RATING_SELECTED_OPACITY;
                image4.Opacity = RATING_UNSELECTED_OPACITY;
                image5.Opacity = RATING_UNSELECTED_OPACITY;
            }
            else if (sender == image4)
            {
                rating = 4;
                image1.Opacity = RATING_SELECTED_OPACITY;
                image2.Opacity = RATING_SELECTED_OPACITY;
                image3.Opacity = RATING_SELECTED_OPACITY;
                image4.Opacity = RATING_SELECTED_OPACITY;
                image5.Opacity = RATING_UNSELECTED_OPACITY;
            }
            else if (sender == image5)
            {
                rating = 5;
                image1.Opacity = RATING_SELECTED_OPACITY;
                image2.Opacity = RATING_SELECTED_OPACITY;
                image3.Opacity = RATING_SELECTED_OPACITY;
                image4.Opacity = RATING_SELECTED_OPACITY;
                image5.Opacity = RATING_SELECTED_OPACITY;
            }

            Task<bool> pushTask = Data.PushUserRatingAsync(rating); 
            if (await pushTask)
            {
                Application.Current.Properties["DateRated"] = now.ToString();
                Application.Current.Properties["UserRating"] = rating;
                Application.Current.Properties["LocationRated"] = Data.CurrentLocality;
                lastRated.Text = $"You last rated {Data.CurrentLocality} at {now.ToString()}. " +
                    $"You may next rate after {GetNextRefreshTimeAsString()}";
                lastRated.IsVisible = true;
                image1.GestureRecognizers.Clear();
                image2.GestureRecognizers.Clear();
                image3.GestureRecognizers.Clear();
                image4.GestureRecognizers.Clear();
                image5.GestureRecognizers.Clear();
                await DisplayAlert("Thank you!", "Thanks for your feedback.", "OK");
                await Application.Current.SavePropertiesAsync();
            }
            else
            {
                await DisplayAlert("Uh-Oh", "Could not submit rating. Please try again later.", "OK");
            }
        }

        /// <summary>
        /// Check if the user is allowed to rate
        /// </summary>
        /// <returns><c>true</c>, if user is allowed to rate, <c>false</c> otherwise.</returns>
        private bool IsRatingEnabled()
        {
            if (!Application.Current.Properties.ContainsKey("DateRated"))
            {
                return true;
            }
            DateTime now = DateTime.Now;
            DateTime lastRatingTime = Convert.ToDateTime(Application.Current.Properties["DateRated"]);

            // If the difference is greater than 1 day, rating must be enabled.
            if ((now - lastRatingTime).Days >= 1)
            {
                return true;
            }

            if ((now - lastRatingTime).Hours > 6)
            {
                return true;
            }

            if (lastRatingTime.Hour < 3)
            {
                return now.Hour >= 3;
            }
            if (lastRatingTime.Hour >= 3 && lastRatingTime.Hour < 9)
            {
                return now.Hour >= 9;
            }
            if (lastRatingTime.Hour >= 9 && lastRatingTime.Hour < 15)
            {
                return now.Hour >= 15;
            }
            if (lastRatingTime.Hour >= 15 && lastRatingTime.Hour < 21)
            {
                return now.Hour >= 21;
            }
            if (lastRatingTime.Hour >= 21)
            {
                if (now.Day - lastRatingTime.Day >= 1)
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        /// <summary>
        /// Gets the next refresh time as string.
        /// </summary>
        /// <returns>The next refresh time as string.</returns>
        private string GetNextRefreshTimeAsString()
        {
            DateTime now = DateTime.Now;

            if (now.Hour < 3)
            {
                var result = new DateTime(now.Year, now.Month, now.Day,
                    3, 0, 0);
                return result.ToString();
            }

            if (now.Hour >= 3 && now.Hour < 9)
            {
                var result = new DateTime(now.Year, now.Month, now.Day,
                    9, 0, 0);
                return result.ToString();
            }
            if (now.Hour >= 9 && now.Hour < 15)
            {
                var result = new DateTime(now.Year, now.Month, now.Day,
                    15, 0, 0);
                return result.ToString();
            }
            if (now.Hour >= 15 && now.Hour < 21)
            {
                var result = new DateTime(now.Year, now.Month, now.Day,
                    21, 0, 0);
                return result.ToString();
            }
            if (now.Hour >= 21)
            {
                DateTime newDateTime = now.AddDays(1);
                var result = new DateTime(newDateTime.Year, newDateTime.Month, newDateTime.Day,
                    3, 0, 0);
                return result.ToString();
            }
            return string.Empty;
        }

        /// <summary>
        /// Event handler for clicking the "info" icon.
        /// </summary>
        /// <param name="sendre">The info icon.</param>
        /// <param name="e">E.</param>
        private void Button_Clicked(object sendre, EventArgs e)
        {
            DisplayAlert("Information", "The spread of pollen in the surrounding is determined by a combination of " +
                "wind speed, distribution of trees and their pollinating seasons, humidity, atmospheric pressure and temperature of that area", "OK");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CounterHayFever.Utils;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Essentials;

namespace CounterHayFever.Models
{
    public static class Data
    {
        /// <summary>
        /// The smallest value of score above which everything is <c>Low</c>
        /// </summary>
        private const double LOW_LOWER_BOUND = 30.55;

        /// <summary>
        /// The highest value of score below which everything is <c>High</c>.
        /// </summary>
        private const double HIGH_UPPER_BOUND = 79.8;

        /// <summary>
        /// The task to fetch suburb data.
        /// </summary>
        private static Task<string> suburbDataTask;

        /// <summary>
        /// The cancellation token to cancel asynchronous requests.
        /// </summary>
        private static readonly CancellationToken cancellationToken = new CancellationToken();

        /// <summary>
        /// The user's current location.
        /// </summary>
        /// <value>User's current location.</value>
        public static Location CurrentLocation { get; private set; }

        /// <summary>
        /// The user's current locality.
        /// </summary>
        /// <value>The user's current locality.</value>
        public static string CurrentLocality { get; private set; }

        /// <summary>
        /// The construction data.
        /// </summary>
        /// <value>The construction data.</value>
        public static List<ConstructionModel> ConstructionData { get; private set; }

        /// <summary>
        /// List of months in pollinating season.
        /// </summary>
        /// <value>The months in pollinating season.</value>
        public static Month[] MonthsInPollinatingSeason { get; } = new Month[]
        {
            Month.October, Month.November, Month.December, Month.January, Month.February
        };

        /// <summary>
        /// Populates the user's current location into <c>CurrentLocation</c>.
        /// </summary>
        /// <returns>User's current location.</returns>
        public static async Task GetCurrentLocationAsync()
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Medium);
            var location = await Geolocation.GetLocationAsync(request);

            if (location != null)
            {
                CurrentLocation = location;
                var placemarks = await Geocoding.GetPlacemarksAsync(location);
                var placemark = placemarks?.FirstOrDefault();
                CurrentLocality = $"{placemark.Locality}";
            }
        }

        /// <summary>
        /// Gets the current weather conditions at a given location.
        /// </summary>
        /// <returns>The current weather conditions async.</returns>
        /// <param name="latitude">Latitude of the place.</param>
        /// <param name="longitude">Longitud of the place.</param>
        public static async Task<WeatherModel> GetCurrentWeatherConditionsAsync(double latitude, double longitude)
        {
            string baseUrl = $"{Xamarin.Forms.Application.Current.Properties["ServiceURL"]}/api/weather";
            var placemarks = await Geocoding.GetPlacemarksAsync(latitude, longitude);
            var placemark = placemarks?.FirstOrDefault();
            var suburb = $"{placemark.Locality}";
            Dictionary<string, string> queryParameters = new Dictionary<string, string>
            {
                { "latitude", latitude.ToString() },
                { "longitude", longitude.ToString() },
                { "suburb", suburb }
            };
            string url = QueryHelpers.AddQueryString(baseUrl, queryParameters);

            using (var httpClient = new HttpClient())
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    WeatherModel weather = JsonConvert.DeserializeObject<WeatherModel>(result);

                    int currentMonth = DateTime.Now.Month;

                    // If month is not pollinating, the severity is low.
                    if (!MonthsInPollinatingSeason.Any(x => (int)x == currentMonth))
                    {
                        weather.Severity = WeatherSeverity.Low;
                    }
                    else
                    {
                        if (weather.Score <= LOW_LOWER_BOUND)
                        {
                            weather.Severity = WeatherSeverity.Low;
                        }
                        else if (weather.Score > LOW_LOWER_BOUND && weather.Score <= HIGH_UPPER_BOUND)
                        {
                            weather.Severity = WeatherSeverity.Medium;
                        }
                        else
                        {
                            weather.Severity = WeatherSeverity.High;
                        }
                    }

                    return weather;
                }
                return null;
            }
        }

        /// <summary>
        /// Starts the suburb data task to fetch suburb data from the API.
        /// </summary>
        public static void StartSuburbDataTask()
        {
            suburbDataTask = Task.Run(async () => {
                try
                {
                    var client = new HttpClient();
                    HttpResponseMessage response = await client.GetAsync($"{Xamarin.Forms.Application.Current.Properties["ServiceURL"]}/api/suburbs", cancellationToken);
                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsStringAsync();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.ToString());
                }
                return string.Empty;
            }, cancellationToken);
        }

        /// <summary>
        /// Get a list of suburb names.
        /// </summary>
        /// <returns>The list of suburbs.</returns>
        public async static Task<List<string>> GetSuburbsListAsync()
        {
            string response = await suburbDataTask;
            List<string> suburbs = JsonConvert.DeserializeObject<List<string>>(response);
            suburbs.Sort();
            return suburbs;
        }

        /// <summary>
        /// Gets the tree count in a suburb.
        /// </summary>
        /// <returns>A <c>SuburbLocation</c> with data for the suburb.</returns>
        /// <param name="suburb">Suburb name.</param>
        public static async Task<SuburbLocation> GetTreeCountInSuburbAsync(string suburb)
        {
            string url = $"{Xamarin.Forms.Application.Current.Properties["ServiceURL"]}/api/suburbs/{suburb}";
            using (var httpClient = new HttpClient())
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    SuburbLocation count = JsonConvert.DeserializeObject<SuburbLocation>(result);
                    return count;
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the construction data.
        /// </summary>
        /// <returns>A list of <c>ConstructionModel</c> that holds construction data.</returns>
        public static async Task<List<ConstructionModel>> GetConstructionDataAsync()
        {
            if (ConstructionData != null)
            {
                return ConstructionData;
            }

            string url = $"{Xamarin.Forms.Application.Current.Properties["ServiceURL"]}/api/construction";
            using (var httpClient = new HttpClient())
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    ConstructionData = JsonConvert.DeserializeObject<List<ConstructionModel>>(result);
                    return ConstructionData;
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the risk levels for a suburb based on other users' inputs from the web service.
        /// </summary>
        /// <returns>The risk levels for suburb.</returns>
        /// <param name="suburb">The suburb for which risk leve is needed.</param>
        public static async Task<UserRatingsModel> GetRiskLevelsForSuburbAsync(string suburb)
        {
            string url = $"{Xamarin.Forms.Application.Current.Properties["ServiceURL"]}/api/userratings/{suburb}";
            using (var httpClient = new HttpClient())
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    UserRatingsModel userRatings = JsonConvert.DeserializeObject<UserRatingsModel>(result);
                    return userRatings;
                }
                return null;
            }
        }

        /// <summary>
        /// Pushs the user rating to Azure.
        /// </summary>
        /// <returns><c>true</c> if the submission succeeded, <c>false</c> otherwise.</returns>
        /// <param name="rating">Rating.</param>
        public static async Task<bool> PushUserRatingAsync(int rating)
        {
            string url = $"{Xamarin.Forms.Application.Current.Properties["ServiceURL"]}/api/userratings";
            using (var httpClient = new HttpClient())
            {
                string requestBody = $"{{'Suburb':'{CurrentLocality}', 'Rating':{rating}}}";
                HttpResponseMessage response = await httpClient.PostAsync(url, new StringContent(requestBody,Encoding.UTF8, "application/json"));
                return response.IsSuccessStatusCode;
            }
        }
    }
}

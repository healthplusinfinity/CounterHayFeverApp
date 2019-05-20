using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CounterHayFever.Models;
using Xamarin.Forms;

namespace CounterHayFever.Utils
{
    public class SearchBarUtil
    {
        /// <summary>
        /// The search bar.
        /// </summary>
        private readonly SearchBar searchBar;

        /// <summary>
        /// The list view to use for the suggestions.
        /// </summary>
        private readonly ListView suggestionsList;

        /// <summary>
        /// An optional callback delegate to perform some operation after populating search box.
        /// </summary>
        private readonly Func<Task> action;

        /// <summary>
        /// A list with all suburbs 
        /// </summary>
        private static List<string> suburbs;

        /// <summary>
        /// The initial search bar text, to restore in erroneous conditions.
        /// </summary>
        private string initialSearchBarText;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:CounterHayFever.Utils.SearchBarUtil"/> class.
        /// </summary>
        /// <param name="searchBar">Search bar.</param>
        /// <param name="suggestionsList">The list view to use for the suggestions.</param>
        /// <param name="action">An optional callback delegate to perform some operation after populating search box.</param>
        public SearchBarUtil(SearchBar searchBar, ListView suggestionsList, Func<Task> action = null)
        {
            PopulateSuburbs();
            this.searchBar = searchBar;
            this.suggestionsList = suggestionsList;
            this.action = action;
        }

        /// <summary>
        /// Registers the event handlers for the search bar
        /// and the suggestion list.
        /// </summary>
        public void RegisterHandlers()
        {
            //Set up event handlers for both, the search bar and the list view.
            initialSearchBarText = searchBar.Text ?? string.Empty;
            searchBar.SearchButtonPressed += Handle_SearchButtonPressed;
            searchBar.TextChanged += Handle_TextChanged;
            suggestionsList.ItemTapped += Handle_ItemTapped;
        }

        /// <summary>
        /// Get the list of suburbs from <c>Data</c>.
        /// </summary>
        private void PopulateSuburbs()
        {
            // Intention is to prevent this method from being async.
            Task.Run(async () =>
            {
                suburbs = await Data.GetSuburbsListAsync();
            });
        }

        /// <summary>
        /// Handler for search bar text changes.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void Handle_TextChanged(object sender, Xamarin.Forms.TextChangedEventArgs e)
        {
            var keyword = searchBar.Text;
            if (searchBar.Text.Length > 0)
            {
                var suggestions = suburbs.Where(x => x.ToLower().Contains(keyword.ToLower()));
                if (!suggestions.Any())
                {
                    suggestionsList.ItemsSource = new string[] { "No matching suggestions." };
                }
                else
                {
                    suggestionsList.ItemsSource = suggestions;
                }
                suggestionsList.IsVisible = true;
            }
            else
            {
                suggestionsList.IsVisible = false;
            }
        }

        /// <summary>
        /// Handler for change in the item selected from list of suggestions.
        /// We use ItemTapped in order to fire the event even when the same
        /// location is chosen multiple times.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private async void Handle_ItemTapped(object sender, Xamarin.Forms.ItemTappedEventArgs e)
        {
            if (suggestionsList.SelectedItem.ToString().Equals("No matching suggestions.")||
                (suggestionsList.SelectedItem.ToString().Equals("The place you specified was not found.")))
            {
                searchBar.Text = initialSearchBarText;
                suggestionsList.IsVisible = false;
                return;
            }
            searchBar.Text = suggestionsList.SelectedItem.ToString();
            suggestionsList.IsVisible = false;
            if (action != null)
            {
                await action.Invoke();
            }
        }

        /// <summary>
        /// Handler triggered when the search button is pressed.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private async void Handle_SearchButtonPressed(object sender, System.EventArgs e)
        {
            var suggestions = suburbs.Where(x => x.ToLower().Equals(searchBar.Text.ToLower()));
            if (suggestions.Count() == 1)
            {
                searchBar.Text = suggestions.First();
                suggestionsList.IsVisible = false;
                if (action != null)
                {
                    await action.Invoke();
                }
            }
            else
            {
                suggestionsList.ItemsSource = new string[] { "The place you specified was not found." };
                suggestionsList.IsVisible = true;
            }
        }

        /// <summary>
        /// Deregisters the event handlers
        /// for the search bar and the suggestion list.
        /// </summary>
        public void DeregisterHandlers()
        {
            searchBar.SearchButtonPressed -= Handle_SearchButtonPressed;
            searchBar.TextChanged -= Handle_TextChanged;
            suggestionsList.ItemTapped -= Handle_ItemTapped;
        }
    }
}

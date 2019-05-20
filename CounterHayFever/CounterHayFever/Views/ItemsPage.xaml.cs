using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using CounterHayFever.Models;
using CounterHayFever.Views;
using CounterHayFever.ViewModels;

namespace CounterHayFever.Views
{
    /// <summary>
    /// Page that displays all symptoms.
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ItemsPage : ContentPage
    {
        /// <summary>
        /// The view model for all symptoms.
        /// </summary>
        ItemsViewModel viewModel;

        public ItemsPage()
        {
            InitializeComponent();

            BindingContext = viewModel = new ItemsViewModel();
        }

        /// <summary>
        /// Handler for a symptom being selected from the list.
        /// </summary>
        /// <param name="sender">The selected symptom in the list view.</param>
        /// <param name="args">Arguments.</param>
        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            var item = args.SelectedItem as Item;
            if (item == null)
                return;

            await Navigation.PushAsync(new NavigationPage(new ItemDetailPage(new ItemDetailViewModel(item))));

            // Manually deselect item.
            ItemsListView.SelectedItem = null;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (viewModel.Items.Count == 0)
                viewModel.LoadItemsCommand.Execute(null);
        }
    }
}
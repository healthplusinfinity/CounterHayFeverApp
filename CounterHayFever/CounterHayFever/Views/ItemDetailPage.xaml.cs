using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using CounterHayFever.Models;
using CounterHayFever.ViewModels;

namespace CounterHayFever.Views
{
    /// <summary>
    /// The detailed suggestions page per symptom.
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ItemDetailPage : ContentPage
    {
        /// <summary>
        /// The view model for suggestions for each symptom.
        /// </summary>
        ItemDetailViewModel viewModel;

        public ItemDetailPage(ItemDetailViewModel viewModel)
        {
            InitializeComponent();

            BindingContext = this.viewModel = viewModel;
        }

        public ItemDetailPage()
        {
            InitializeComponent();

            var item = new Item
            {
                Text = "",
                Description = ""
            };

            viewModel = new ItemDetailViewModel(item);
            BindingContext = viewModel;
        }
    }
}
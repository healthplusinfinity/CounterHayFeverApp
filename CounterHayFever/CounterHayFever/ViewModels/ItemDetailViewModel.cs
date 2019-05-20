using System;

using CounterHayFever.Models;

namespace CounterHayFever.ViewModels
{
    /// <summary>
    /// The view model for suggestions per symptom.
    /// </summary>
    public class ItemDetailViewModel : BaseViewModel
    {
        /// <summary>
        /// The symptom suggestions as an Item.
        /// </summary>
        public Item Item { get; set; }
       
       public ItemDetailViewModel(Item item = null)
        {
            Title = item?.Text;
            Item = item;
        }
    }
}

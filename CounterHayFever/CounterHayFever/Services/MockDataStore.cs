using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CounterHayFever.Models;

namespace CounterHayFever.Services
{
    public class MockDataStore : IDataStore<Item>
    {
        List<Item> items;

        public MockDataStore()
        {
            items = new List<Item>();
            var mockItems = new List<Item>
            {
                new Item { Id = Guid.NewGuid().ToString(), Text = "Sneezing" 
                , Description="Applying a small amount of Vaseline (petroleum gel) to the nasal openings to trap pollen grains.\n\n" +
                "Try a saline nasal rinse which helps clear allergens like pollen from your nasal membranes, minimizing symptoms.\n\n" +
                "Keep all the windows closed during pollen season.\n\n" +
                "Use air conditioning in your home and car if possible.\n\n"+
                "Wear a pollen mask and take proper medication before going out" },

                new Item { Id = Guid.NewGuid().ToString(), Text = "Blocked/Runny Nose", 
                    Description="Try a saline nasal rinse which helps clear allergens like pollen from your nasal membranes, minimizing symptoms.\n\n" +
                        "Eating foods rich in beta carotene (carrots, spinach, any yellow fruit) and omega 3 (oily fish) are two more ways to soothe your blocked nose and painful sinuses.\n\n" +
                        "Drink two or three cups of fenugreek seed tea daily.\n\n" +
                        "Eating more ginger, onion, or garlic help prevent sinus congestion.\n\n" +
                        "Try a saline nasal rinse which helps clear allergens like pollen from your nasal membranes, minimizing symptoms." },

                new Item { Id = Guid.NewGuid().ToString(), Text = "Itchy/Swollen Eyes", 
                Description="Wear wraparound sunglasses to stop pollen getting in your eyes.\n\n" +
                    "Apply and rinse your eyes with saline eye drops after being outdoors to wash away pollen.\n\n" +
                    "Wear a hat with a wide brim, a facial mask and sunglasses to reduce the amount of pollen that blows into your eyes.\n\n" +
                    "Apply wet and cold washcloth to the skin under and around your eyes for a few minutes, using gentle pressure." },

                new Item { Id = Guid.NewGuid().ToString(), Text = "Itchy Throat", 
                Description="Gargle with warm salt water, 1 - 2 tablespoons of table salt in 8 ounces of warm water, to soothe a mildly sore throat.\n\n" +
                    "Try Lozenges and Hot Drinks.\n\n"+
                    "Wear a pollen mask and take proper medication before going out." },

                new Item { Id = Guid.NewGuid().ToString(), Text = "Cough", Description="Cover your mouth while going outside." },

                new Item { Id = Guid.NewGuid().ToString(), Text = "Headache", 
                Description="Don’t hang clothing outdoors to dry; pollen may cling to towels and sheets.\n\n" +
                    "Reduce exposure to dust mites, especially in the bedroom. Use “mite-proof” covers for pillows, comforters and duvets " +
                    "and mattresses and box springs. \n\n" +
                    "Wash your bedding frequently, using hot water (at least 130 degrees Fahrenheit).\n\n" +
                    "Apply a warm, moist washcloth to your face." },

                new Item { Id = Guid.NewGuid().ToString(), Text = "Fatigue", 
                Description="Having locally made honey gives your immune system a daily dose of the allergen," +
               "having locally made honey make your immune system resistent to allergen.\n\n" +
               "Apple cider vinegar as it can help reduce your allergic reactions.The quick approach is to swallow a tablespoon.\n\n" +
               "Eat Fruits and vegetables regularly during this season." }
            };

            foreach (var item in mockItems)
            {
                items.Add(item);
            }
        }

        public async Task<bool> AddItemAsync(Item item)
        {
            items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateItemAsync(Item item)
        {
            var oldItem = items.Where((Item arg) => arg.Id == item.Id).FirstOrDefault();
            items.Remove(oldItem);
            items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteItemAsync(string id)
        {
            var oldItem = items.Where((Item arg) => arg.Id == id).FirstOrDefault();
            items.Remove(oldItem);

            return await Task.FromResult(true);
        }

        public async Task<Item> GetItemAsync(string id)
        {
            return await Task.FromResult(items.FirstOrDefault(s => s.Id == id));
        }

        public async Task<IEnumerable<Item>> GetItemsAsync(bool forceRefresh = false)
        {
            return await Task.FromResult(items);
        }
    }
}
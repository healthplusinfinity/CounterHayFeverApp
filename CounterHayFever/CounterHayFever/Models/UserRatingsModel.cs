using System;
namespace CounterHayFever.Models
{
    public class UserRatingsModel
    {
        public string Suburb { get; set; }
        public int TotalRating { get; set; }
        public int RatingCount { get; set; }
        public double AverageRating { get; set; }
    }
}

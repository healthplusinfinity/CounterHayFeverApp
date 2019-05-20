using System;
namespace CounterHayFever.Models
{
    /// <summary>
    /// A class to hold weather Parameters.
    /// </summary>
    public class WeatherModel
    {
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double Pressure { get; set; }
        public double WindSpeed { get; set; }
        public double Score { get; set; }
        public int TreeCount { get; set; }
        public int ConstructionCount { get; set; }
        public WeatherSeverity Severity { get; set; }
    }

    /// <summary>
    /// Enumeration for Weather severity.
    /// </summary>
    public enum WeatherSeverity
    {
        Low = 1,
        Medium = 2,
        High = 3
    }
}

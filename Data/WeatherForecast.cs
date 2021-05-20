using System;

namespace WebDoAn.Data
{
    public class WeatherForecast
    {
        public DateTime Date { get; set; }

        public int doDuc { get; set; }
        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
        public float pH { get; set; }
        public float luuLuong { get; set; }
        
    }
    
}

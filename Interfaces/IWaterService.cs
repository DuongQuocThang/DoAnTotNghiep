using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebDoAn.Models;
using WebDoAn.Services;

namespace WebDoAn.Interfaces
{
    public interface IWaterService
    {
        public event WaterDelegate OnWaterChanged;
        IList<WaterData> GetCurrentWater();

        public IList<WaterData> GetWater(DateTime startDate, DateTime endDate);
    }
}

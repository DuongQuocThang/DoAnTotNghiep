using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebDoAn.Models;

namespace WebDoAn.Interfaces
{
    public interface IWaterService
    {
        public event WaterDelegate OnWaterChanged;
        IList<WaterData> GetCurrentWater();
    }
}

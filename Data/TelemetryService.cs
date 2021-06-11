using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebDoAn.Data
{
    public class TelemetryService
    {
        public event EventHandler<TelemetryDataPoint> InputMessageReceived;

        private async Task OnInputMessageReceived(TelemetryDataPoint telemetryDataPoint)
        {
            await Task.Run(() => { InputMessageReceived?.Invoke(this, telemetryDataPoint); });
        }

        public async Task SendTelemetry(TelemetryDataPoint telemetryDataPoint)
        {
            if (telemetryDataPoint != null)
            {
                await OnInputMessageReceived(telemetryDataPoint);
            }
        }
    }

    public class TelemetryDataPoint
    {
        public string Temperature { get; set; }
        public string Turbidity { get; set; }
        public string PH { get; set; }
        public string Waterflow { get; set; }
    };
}

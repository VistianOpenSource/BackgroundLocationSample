using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;

namespace BackgroundLocationSample
{
    public class SensorMonitoring
    {
        public IDisposable Setup(Action<LocationEvent> action)
        {
            var options= new LocationOptions()
            {Accuracy = LocationAccuracy.HundredMeters, 
                ReportInterval = TimeSpan.FromSeconds(15),
                FastestInterval=TimeSpan.FromSeconds(15)};

            var obs = ILocationProviderMixins.Provider.CreateObservable(options);
            return obs.
                Do(action).
                Subscribe();
        }
    }
}

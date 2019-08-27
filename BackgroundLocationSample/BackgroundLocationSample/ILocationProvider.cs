using System;
using System.Collections.Generic;
using System.Text;

namespace BackgroundLocationSample
{
    public interface ILocationProvider
    {
        IObservable<LocationEvent> LastLocationObservable();
        IObservable<LocationEvent> CreateObservable(LocationOptions options);
    }

    public static class ILocationProviderMixins
    {
        public static ILocationProvider Provider { get; set; }
    }
}

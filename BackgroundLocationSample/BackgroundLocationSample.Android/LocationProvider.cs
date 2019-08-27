using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Gms.Location;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace BackgroundLocationSample.Droid
{
    public sealed class LocationProvider : ILocationProvider
    {
        private readonly Context _context;
        private readonly FusedLocationProviderClient _fusedLocationProviderClient;

        public LocationProvider(Context context)
        {
            _context = context;
            _fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(context);
        }

        private readonly Subject<LocationEvent> _lastEvent = new Subject<LocationEvent>();

        public IObservable<LocationEvent> LastLocationObservable()
        {
            return _lastEvent.AsObservable();
        }

        /// <summary>
        /// Create a location observable using the specified options.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public IObservable<LocationEvent> CreateObservable(LocationOptions options)
        {
            var rootObservable = CreateLocationObservable(options).Do(e => _lastEvent.OnNext(e));

            if (options.Duration < TimeSpan.MaxValue)
            {
                rootObservable = rootObservable.Timeout(DateTimeOffset.Now+options.Duration).
                    Catch(Observable.Empty<LocationEvent>());
            }

            // if number of requests is limited then restrict lifetime of this observable
            if (options.NumberOfUpdates > 0 && options.NumberOfUpdates < int.MaxValue)
            {
                return rootObservable.Take(options.NumberOfUpdates).
                    Publish().
                    RefCount();                
            }
            else
            {
                return rootObservable.
                    Publish().
                    RefCount();                
            }
        }

        /// <summary>
        /// Convert our generic accuracy into nearest android versions.
        /// </summary>
        /// <returns></returns>
        private int GetPriority(LocationAccuracy accuracy)
        {
            switch (accuracy)
            {
                case LocationAccuracy.Best:
                case LocationAccuracy.TenMeters:
                    return LocationRequest.PriorityHighAccuracy;

                case LocationAccuracy.HundredMeters:
                case LocationAccuracy.Kilometer:
                    return LocationRequest.PriorityBalancedPowerAccuracy;

                case LocationAccuracy.ThreeKilometers:
                    return LocationRequest.PriorityLowPower;

                default:
                    return LocationRequest.PriorityLowPower;
            }
        }

        private LocationRequest CreateRequest(LocationOptions options)
        {
            var request = LocationRequest.Create();

            request.SetSmallestDisplacement(options.MovementThreshold);
            request.SetPriority(GetPriority(options.Accuracy));
            request.SetInterval((long)options.ReportInterval.TotalMilliseconds);

            if (options.FastestInterval.HasValue)
            {
                request.SetFastestInterval((long) options.FastestInterval.Value.TotalMilliseconds);
            }


            if (options.NumberOfUpdates > 0 && options.NumberOfUpdates < int.MaxValue)
                request.SetNumUpdates(options.NumberOfUpdates);

            if (options.Duration != TimeSpan.MaxValue)
            {
                request.SetExpirationDuration((long)options.Duration.TotalMilliseconds);
            }

            return request;
        }


        public static int NoActiveObservables = 0;

        private IObservable<LocationEvent> CreateLocationObservable(LocationOptions options)
        {
            return Observable.Create<LocationEvent>(async (o) =>
            {
                var lc = new MyLocationCallback((lr) =>
                {

                    foreach (var item in lr.Locations)
                    {
                        double? verticalAccuracy = null;

                        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
                        {
                            if (item.HasVerticalAccuracy) verticalAccuracy = item.VerticalAccuracyMeters;
                        }

                        var location = new Location(item.Latitude, item.Longitude,
                            item.HasAccuracy ? item.Accuracy : (double?)null, 
                            item.HasAltitude ? item.Altitude : (double?)null,
                            verticalAccuracy,
                            item.HasBearing ? item.Bearing : (double?)null, 
                            item.HasSpeed ? item.Speed : (double?)null);

                        DateTimeOffset dto;

                        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.JellyBeanMr1)
                        {
                            var en = item.ElapsedRealtimeNanos;

                            var sn = SystemClock.ElapsedRealtimeNanos();

                            var age = (sn - en) / 1000000;

                            dto = DateTimeOffset.FromUnixTimeMilliseconds(
                                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - age);
                        }
                        else
                        {
                             dto = DateTimeOffset.Now;
                        }

                        var locationEvent = new LocationEvent(dto, location);

                        o.OnNext(locationEvent);
                    }
                });


                var request = CreateRequest(options);

                await _fusedLocationProviderClient.RequestLocationUpdatesAsync(request, lc, _context.MainLooper).ConfigureAwait(false);

                Interlocked.Increment(ref NoActiveObservables);

                return Disposable.Create(() =>
                    {
                        Interlocked.Decrement(ref NoActiveObservables);

                        _fusedLocationProviderClient.RemoveLocationUpdates(lc);
                    });
            });
        }
    }

    public sealed class MyLocationCallback : LocationCallback
    {
        private readonly Action<LocationResult> _resultAction;

        public MyLocationCallback(Action<LocationResult> resultAction)
        {
            _resultAction = resultAction;
        }
        public override void OnLocationAvailability(LocationAvailability locationAvailability)
        {
            base.OnLocationAvailability(locationAvailability);
        }

        public override void OnLocationResult(LocationResult result)
        {
            _resultAction(result);
            base.OnLocationResult(result);
        }
    }
}
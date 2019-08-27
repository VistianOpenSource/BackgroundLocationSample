using System;
using System.Collections.Generic;
using System.Text;

namespace BackgroundLocationSample
{
    /// <summary>
    /// Specifies options around how locations should be constructed and used.
    /// </summary>
    public class LocationOptions
    {
        /// <summary>
        /// Get or set the accuracy of location updates
        /// </summary>
        public LocationAccuracy Accuracy { get; set; }

        /// <summary>
        /// Get or set the frequency for updates of location.
        /// </summary>
        public TimeSpan ReportInterval { get; set; }

        /// <summary>
        /// Get or set the movement threshold for updates in meters
        /// </summary>
        public int MovementThreshold { get; set; }

        /// <summary>
        /// Get or set the maximum number of numbers the user should see
        /// </summary>
        public int NumberOfUpdates { get; set; }

        /// <summary>
        /// Get or set the max duration that each observable should be waited for.
        /// </summary>
        public TimeSpan Duration { get; set; }

        public TimeSpan? FastestInterval { get; set; }

        /// <summary>
        /// Setup with default accuracy , reporting and threshold.
        /// </summary>
        public LocationOptions()
        {
            Accuracy = LocationAccuracy.HundredMeters;
            ReportInterval = TimeSpan.FromMilliseconds(1000);
            FastestInterval = null;
            MovementThreshold = 0;
            NumberOfUpdates = int.MaxValue;
            Duration = TimeSpan.MaxValue;
        }
    }

    /// <summary>
    /// Represents required accuracy from location providers.
    /// </summary>
    public enum LocationAccuracy
    {
        Best,
        TenMeters,
        HundredMeters,
        Kilometer,
        ThreeKilometers
    }
}

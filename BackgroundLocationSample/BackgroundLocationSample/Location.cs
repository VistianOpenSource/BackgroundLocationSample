using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace BackgroundLocationSample
{
    /// <summary>
    /// Standardized Location
    /// </summary>
    public class Location:IEquatable<Location>
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public double? Accuracy { get; set; }

        public double? Altitude { get; set; }

        public double? VerticalAccuracy { get; set; }

        public double? Bearing { get; set; }

        public double? Speed { get; set; }


        /// <summary>
        /// For LiteDb only 
        /// </summary>
        public Location()
        {
        }

        public Location(double latitude, double longitude, double? accuracy = default, double? altitude = default,double? verticalAccuracy=default,double? bearing=default,double? speed=default)
        {
            Latitude = latitude;
            Longitude = longitude;
            Accuracy = accuracy;
            Altitude = altitude;
            VerticalAccuracy = verticalAccuracy;
            Bearing = bearing;
            Speed = speed;
        }

        public bool Equals(Location other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Latitude.Equals(other.Latitude) && Longitude.Equals(other.Longitude) && Accuracy.Equals(other.Accuracy) && Altitude.Equals(other.Altitude) && VerticalAccuracy.Equals(other.VerticalAccuracy) && Bearing.Equals(other.Bearing) && Speed.Equals(other.Speed);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Location) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Latitude.GetHashCode();
                hashCode = (hashCode * 397) ^ Longitude.GetHashCode();
                hashCode = (hashCode * 397) ^ Accuracy.GetHashCode();
                hashCode = (hashCode * 397) ^ Altitude.GetHashCode();
                hashCode = (hashCode * 397) ^ VerticalAccuracy.GetHashCode();
                hashCode = (hashCode * 397) ^ Bearing.GetHashCode();
                hashCode = (hashCode * 397) ^ Speed.GetHashCode();
                return hashCode;
            }
        }
    }
}

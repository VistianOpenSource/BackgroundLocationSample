using System;
using System.Collections.Generic;
using System.Text;

namespace BackgroundLocationSample
{
    public sealed class LocationEvent:IEquatable<LocationEvent>
    {
        public DateTimeOffset Date { get; }
        public Location Location { get; }

        public LocationEvent(DateTimeOffset date, Location location)
        {
            Date = date;
            Location = location;
        }

        public bool Equals(LocationEvent other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Date.Equals(other.Date) && Equals(Location, other.Location);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is LocationEvent other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Date.GetHashCode() * 397) ^ (Location != null ? Location.GetHashCode() : 0);
            }
        }
    }
}

using System;
using Microsoft.SPOT;

namespace uPLibrary.IoT.ThingSpeak
{
    /// <summary>
    /// Optional Location parameters for channel update
    /// </summary>
    public class Location
    {
        /// <summary>
        /// Latitude in decimal degrees
        /// </summary>
        public double Latitude;

        /// <summary>
        /// Longitude in decimal degrees
        /// </summary>
        public double Longitude;

        /// <summary>
        /// Elevation in meters
        /// </summary>
        public double Elevation;
    }
}

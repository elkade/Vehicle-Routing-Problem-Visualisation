using System;
using System.Globalization;

namespace gis_vrp.Models
{
    public class Point
    {
        public int Id { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public PointType Type { get; set; }
        public int Capacity { get; set; }
        public DateTime OccurTime { get; set; }
        public int UnloadTime { get; set; }

        public override string ToString()
        {
            return string.Format("{0},{1}", Lat.ToString(CultureInfo.InvariantCulture).Replace(",", "."), Lng.ToString(CultureInfo.InvariantCulture).Replace(",", "."));
        }
    }

    public enum PointType
    {
        Client = 0,
        Depot = 1
    }
}
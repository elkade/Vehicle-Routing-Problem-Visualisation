using System;
using System.Collections.Generic;
using gis_vrp.Models;

namespace gis_vrp.Algorithms
{
    public interface IVehicleRouteFinder
    {
        IEnumerable<int> FindRoute(IEnumerable<Point> points, int[,] distances,TimeSpan timeLimit=new TimeSpan());
    }
}

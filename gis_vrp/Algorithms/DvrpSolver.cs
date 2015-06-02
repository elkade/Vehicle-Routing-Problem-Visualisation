using System;
using System.Collections.Generic;
using System.Linq;
using gis_vrp.Algorithms.Dvrp;
using Point = gis_vrp.Models.Point;

namespace gis_vrp.Algorithms
{
    public class DvrpSolver : IVehicleRouteFinder
    {
        public IEnumerable<int> FindRoute(IEnumerable<Point> points, int[,] distances, TimeSpan timeLimit)
        {
            Client.DistanceMatrix = distances;
            List<Point> pointList = points.ToList();
            Dictionary<int, int> demandSection = new Dictionary<int, int>();
            Dictionary<int, int> depotLocationSection = new Dictionary<int, int>();
            Dictionary<int, TimeWindow> depoTimeWindowSection = new Dictionary<int, TimeWindow>();
            depoTimeWindowSection[0] = new TimeWindow(0, int.MaxValue);
            depotLocationSection[0] = 0;
            int[] depots = { 0 };
            Dictionary<int, int> durationSection = new Dictionary<int, int>();
            Dictionary<int, Dvrp.Point> locationCoordSection = new Dictionary<int, Dvrp.Point>();
            locationCoordSection[0] = new Dvrp.Point((int)pointList[0].Lat, (int)pointList[0].Lng);
            string name = "cvrp";
            int numCapacities = 1;
            int numDepots = 1;
            int numLocations = pointList.Count;
            int numVehicles = 1;
            int numVisits = pointList.Count - 1;
            Dictionary<int, int> timeAvailSection = new Dictionary<int, int>();
            int timeStep = 1;
            Dictionary<int, int> visitLocationSection = new Dictionary<int, int>();
            for (int i = 1; i < pointList.Count; i++)
            {
                demandSection[i] = pointList[i].Capacity;
                durationSection[i] = pointList[i].UnloadTime;
                locationCoordSection[i] = new Dvrp.Point((int)pointList[i].Lat, (int)pointList[i].Lng);
                timeAvailSection[i] = (int)(pointList[i].OccurTime - DateTime.Now).TotalMilliseconds;
                visitLocationSection[i] = i;
            }
            Dvrp.Dvrp dvrp = new Dvrp.Dvrp
            {
                Capacities = 1,
                DemandSection = demandSection,
                DepotLocationSection = depotLocationSection,
                DepotTimeWindowSection = depoTimeWindowSection,
                Depots = depots,
                DurationSection = durationSection,
                LocationCoordSection = locationCoordSection,
                Name = name,
                NumCapacities = numCapacities,
                NumDepots = numDepots,
                NumLocations = numLocations,
                NumVehicles = numVehicles,
                NumVisits = numVisits,
                TimeAvailSection = timeAvailSection,
                TimeStep = timeStep,
                VisistLocationSection = visitLocationSection,

            };
            dvrp.Solve(TimeSpan.MaxValue);
            var path = dvrp.BestRoute;
            return path.Select(c => c.Id);
        }

    }
}
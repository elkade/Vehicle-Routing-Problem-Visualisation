using System;

namespace gis_vrp.Algorithms.Dvrp
{
    internal class Client
    {
        public static int[,] DistanceMatrix;
        public int Id { get; set; }
        public Point Point { get; set; }
        public int Duration { get; set; }
        public int TimeAvail { get; set; }
        public int Demand { get; set; }
        public double LoadWhenLeft { get; set; }
        public double TimeWhenHandled { get; set; }

        public double CostToGetTo(Client destination)
        {
            double magnitude;
            if (DistanceMatrix == null)
                magnitude =
                    Math.Sqrt(Math.Pow(this.Point.X - destination.Point.X, 2) +
                              Math.Pow(this.Point.Y - destination.Point.Y, 2));
            else
                magnitude = DistanceMatrix[Id, destination.Id];
            return magnitude;
        }

        public Client Clone()
        {
            return new Client()
            {
                Id=Id,
                Point = new Point(Point.X, Point.Y),
                Demand = Demand,
                Duration = Duration,
                TimeAvail = TimeAvail,
                LoadWhenLeft = LoadWhenLeft
            };
        }
    }
}

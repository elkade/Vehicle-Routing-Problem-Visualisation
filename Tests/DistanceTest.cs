using System;
using gis_vrp.Models;
using gis_vrp.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class DistanceTest
    {
        [TestMethod]
        public void LimitExceedTest()
        {
            const int n = 11;
            var googleApiService = new GoogleApiService();

            var points = new Point[n];

            var r = new Random();

            for (int i = 0; i < n; i++)
            {
                points[i]= new Point
                {
                    Capacity = 1,
                    Lat = r.NextDouble()+52,
                    Lng = r.NextDouble()+20,
                    OccurTime = new DateTime(),
                    Type = PointType.Client,
                    UnloadTime = 1
                };
            }

            var meters = googleApiService.GetDistances(points);
            Assert.IsNotNull(meters);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data.Spatial;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using gis_vrp.Models;
using Newtonsoft.Json;

namespace gis_vrp.Services
{
    public interface IGoogleApiService
    {
        int[,] GetDistances(IList<Point> pointList);
        int GetDistanceBetweenTwoPoints(Point p1, Point p2);
    }

    public class GoogleApiService : IGoogleApiService
    {
        private const string ApiKey = "AIzaSyBxaRXv2wr70PLe90vBECYIf4ZHyj79yfI";
        private const string BaseAddress = "https://maps.googleapis.com/maps/api/";
        private const int Limit = 5;

        public int[,] GetDistances(IList<Point> pointList)
        {
            var distanceRatios = new List<double>();
            var distances = new int?[pointList.Count, pointList.Count];
            var halfLengthFloor = (int) Math.Floor(pointList.Count/2d);
            var halfLengthCeil = (int) Math.Ceiling(pointList.Count/2d);
            List<Point> first = pointList.Take(Limit > halfLengthFloor ? halfLengthFloor : Limit).ToList();
            List<Point> last =
                pointList.Reverse().Take(Limit > halfLengthCeil ? halfLengthCeil : Limit).Reverse().ToList();

            string message = SendAndGetResponse(PrepareRequest(first, last));
            //bierzemy początek i koniec każdy z każdym - więcej nie możemy
            var result = JsonConvert.DeserializeObject<GoogleResponse>(message);
            Debug.WriteLine("Google status: " + result.status);
            if (result.status != "OK"){
                first = new List<Point>();
                last = new List<Point>();
            }
            for (int i = 0; i < first.Count; i++)
            {
                for (int j = 0; j < last.Count; j++)
                {
                    if (result.rows[0].elements[0].status != "OK")
                        distances[i, pointList.Count - last.Count + j] = null;
                    else
                    {
                        distances[i, pointList.Count - last.Count + j] = result.rows[i].elements[j].distance.value;


                        var distanceAb = GetRealDistance(pointList[i], pointList[j]);

                        if (distanceAb < Double.Epsilon)
                            continue;
                        var dist = distances[i, pointList.Count - last.Count + j];
                        if (dist != null)
                            distanceRatios.Add(dist.Value / distanceAb);
                    }
                }
            }
            double averageDistanceRatio;
            if (distanceRatios.Count == 0) averageDistanceRatio = 1;
            else averageDistanceRatio = distanceRatios.Count>0?distanceRatios.Average():1;
            // i przybliżamy resztę
            int[,] notNullDistances = new int[pointList.Count, pointList.Count];
            for (int i = 0; i < pointList.Count; i++)
            {
                for (int j = 0; j < pointList.Count; j++)
                {
                    if (distances[i, j] == null)
                    {
                        double distanceAb = GetRealDistance(pointList[i], pointList[j]);

                        notNullDistances[i, j] = (int) (distanceAb*averageDistanceRatio);
                    }
                    else
                        notNullDistances[i, j] = distances[i, j].Value;
                }
            }

            return notNullDistances;
        }

        public int GetDistanceBetweenTwoPoints(Point p1, Point p2)
        {
            return GetDistances(new[]{p1,p2})[0, 0];
        }


        private string PrepareRequest(IEnumerable<Point> p1, IEnumerable<Point> p2)
        {
            string sp1 = String.Join("|", p1);
            string sp2 = String.Join("|", p2);

            return "distancematrix/json?origins=" + sp1 + "&destinations=" + sp2 + "&mode=driving&language=pl-PL&key=" + ApiKey;

        }

        private string SendAndGetResponse(string requestUri)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseAddress);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.GetAsync(requestUri).Result;
                response.EnsureSuccessStatusCode();

                string result = response.Content.ReadAsStringAsync().Result;
                return result;
            }
        }

        private double GetRealDistance(Point p1, Point p2)
        {
            double longitudeA = p1.Lng;
            double latitudeA = p1.Lat;

            double longitudeB = p2.Lng;
            double latitudeB = p2.Lat;

            const int coordinateSystemId = 4326;

            DbGeography pointA =
                DbGeography.FromText(string.Format(CultureInfo.InvariantCulture, "POINT({0} {1})", longitudeA, latitudeA), coordinateSystemId);
            DbGeography pointB =
                DbGeography.FromText(string.Format(CultureInfo.InvariantCulture, "POINT({0} {1})", longitudeB, latitudeB), coordinateSystemId);

            double? distanceAb = pointA.Distance(pointB);
            if (distanceAb == null)
                throw new NullReferenceException("Null nie wiadomo skąd");
            return distanceAb.Value;
        }



    }

    public class GoogleTopLevelException : Exception
    {
        public GoogleTopLevelException(string message = null)
            : base(message)
        {
        }
    }

}
using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using gis_vrp.Algorithms;
using gis_vrp.Models;
using gis_vrp.Services;
using Microsoft.AspNet.SignalR;

namespace gis_vrp.Hubs
{
    public class DvrpRouteHub : Hub
    {
        private readonly IGoogleApiService _googleApiService;
        private readonly IVehicleRouteFinder _vehicleRouteFinder;

        public DvrpRouteHub(IGoogleApiService googleApiService, IVehicleRouteFinder vehicleRouteFinder)
        {
            _googleApiService = googleApiService;
            _vehicleRouteFinder = vehicleRouteFinder;
        }

        public void SendMessage(string number, List<int> route)
        {
            Clients.All.sendMessage(number,route);
        }

        public void SendProblem(string data)
        {
            var jss = new JavaScriptSerializer();

            var signalrDto = jss.Deserialize<SignalrDto>(data);

             var meters = _googleApiService.GetDistances(signalrDto.Points);

            var computeTime = 25;

            if (!string.IsNullOrEmpty(signalrDto.Time))
            {
                int.TryParse(signalrDto.Time, out computeTime);
            }

            _vehicleRouteFinder.FindRoute(signalrDto.Points, meters, TimeSpan.FromSeconds(computeTime));
        }
    }
}
using System.Collections.Generic;
using gis_vrp.Hubs;
using Microsoft.AspNet.SignalR;

namespace gis_vrp.Algorithms.DvrpAnt
{
    public class DvrpProgressNotifier : IDvrpProgressNotification
    {
        public void Notify(string title, List<int> content)
        {
            GlobalHost.ConnectionManager.GetHubContext<DvrpRouteHub>().Clients.All.SendMessage(title, content);
        }
    }
}
using System.Threading.Tasks;
using gis_vrp.Services;
using Microsoft.AspNet.SignalR;

namespace gis_vrp.Hubs
{
    public class TestHub : Hub
    {
        private readonly ITestService _testService;

        public TestHub(ITestService testService)
        {
            _testService = testService;
        }

        public void Send(string test)
        {
            Clients.All.broadcastMessage(test);
        }
    }
}
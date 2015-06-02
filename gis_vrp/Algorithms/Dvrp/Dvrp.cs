using System;
using System.Collections.Generic;
using System.Linq;

namespace gis_vrp.Algorithms.Dvrp
{
    internal class Dvrp
    {
        public const double CUT_OFF_TIME = 0.5;
        public string Name;
        public int NumDepots;
        public int NumCapacities;
        public int NumVisits;
        public int NumLocations;
        public int NumVehicles;
        public int Capacities;
        public IList<int> Depots;
        public Dictionary<int, int> DemandSection = new Dictionary<int, int>();
        public Dictionary<int, Point> LocationCoordSection=new Dictionary<int, Point>();
        public Dictionary<int, int> DepotLocationSection=new Dictionary<int, int>();
        public Dictionary<int, int> VisistLocationSection=new Dictionary<int, int>();
        public Dictionary<int, int> DurationSection=new Dictionary<int, int>();
        public Dictionary<int, TimeWindow> DepotTimeWindowSection=new Dictionary<int, TimeWindow>();
        public double TimeStep;
        public Dictionary<int, int> TimeAvailSection=new Dictionary<int, int>();

        private Client[] _clients;
        private List<Client> _route;
        private List<Client> _bestRoute;
        private double _cost;
        private bool[] _used;
        private int N { get { return _clients.Length; } }
        private double _capacity;

        //private double _currentLoad;
        private List<double> _addedTimes;
        private double _minCost = int.MaxValue;
        private double _totalTime = double.MaxValue;
        private double[] _vehiclesTimes;
        private double _timeWindow;
        private DateTime t = DateTime.Now;
        private TimeSpan _endTime;
        public double Solve(TimeSpan endTime)
        {
            //CutOffTime();
            _endTime = endTime;
            InitializeClients();
            Reset();
            

            _route.Add(_clients[0]);
            process(0, _capacity, 0,0, new bool[N],  0);

            //var cost = GetCost(_bestRoute);
            return _minCost;
        }

        private void process(int k, double currentLoad, double currentTime,
            int currentVehicle, bool[] checkedNearDepot, double tripStartTime)
        {
            //if ((DateTime.Now - t).TotalMilliseconds >= _endTime.TotalMilliseconds)
            //    return;
            double timeToReturn = _route[_route.Count-1].CostToGetTo(_clients[0]);
            if (GetCost(_route) >= _cost)
                return;
            double currentTimeWithLastDepot = currentTime + timeToReturn;
            double reversedTimeWithLastDepot = GetReversedTimeSinceRecentDepotWithLastDepot(_route, tripStartTime);
            double betterTimeWithLastDepot =
                reversedTimeWithLastDepot < currentTimeWithLastDepot ? reversedTimeWithLastDepot : currentTimeWithLastDepot;
            if (betterTimeWithLastDepot > _timeWindow)
                return;
            if (k == N-1)
            {
                double maxTime = betterTimeWithLastDepot > _vehiclesTimes.Max()
                    ? betterTimeWithLastDepot
                    : _vehiclesTimes.Max();
                _route.Add(_clients[0]);
                double kosztBuf = GetCost(_route,true);
                if (kosztBuf < _cost || _totalTime > maxTime)
                {
                    _cost = kosztBuf;
                    _bestRoute.Clear();
                    for (int x = 0; x < _route.Count; x++)
                    {
                        _bestRoute.Add(_route[x].Clone());
                    }
                    _totalTime = maxTime;

                    var vehiclesUsed = _vehiclesTimes.Where(v => v > 0).Count();
                    Console.WriteLine("{0,10:F3}\t{1,10:F3}\t{2,10:F3}\t{3,10}", (DateTime.Now - t).TotalSeconds, CostOfRoute(_bestRoute.ToArray()), _totalTime, vehiclesUsed);
                }
                _route.RemoveAt(_route.Count - 1);
                return;
            }
            bool areAllTooBig = true;
            bool[] newCheckedNearDepot = new bool[checkedNearDepot.Length];
            checkedNearDepot.CopyTo(newCheckedNearDepot, 0);
            for (int m = N - 1; m >= 0; --m)
            {
                //if(k==2)
                //    Console.WriteLine("\n\n" + String.Join(",", _route.ToArray().Select(p => p.Id.ToString()).ToArray()) + "," + m + "\n\n" + (DateTime.Now - t) +"\t\t" + _minCost+"\n");

                if (_previousZero)
                {
                    if (newCheckedNearDepot[m])
                        continue;
                    newCheckedNearDepot[m] = true;
                }
                if (!_used[m])
                {
                    if (currentLoad >= _clients[m].Demand)
                    {
                        _clients[m].LoadWhenLeft = currentLoad;
                        if (m == 0)
                        {
                            if (_previousZero || !areAllTooBig) return;
                            if (_route.Count > 1 && newCheckedNearDepot[(_route[_route.Count - 1]).Id])
                                return;
                            newCheckedNearDepot[(_route[_route.Count - 1]).Id] = true;

                            double timeDiff = betterTimeWithLastDepot - _vehiclesTimes[currentVehicle];

                            _vehiclesTimes[currentVehicle] = betterTimeWithLastDepot;

                            _route.Add(_clients[m]);
                            process(k, _capacity, _vehiclesTimes.Min(), _vehiclesTimes.ToList().IndexOf(_vehiclesTimes.Min()), newCheckedNearDepot, _vehiclesTimes.Min());
                            _route.RemoveAt(_route.Count - 1);

                            _vehiclesTimes[currentVehicle] -= timeDiff;
                        }
                        else
                        {
                            double travelTime = _clients[m].CostToGetTo(_route[_route.Count - 1]);
                            double nextTime = currentTime > _clients[m].TimeAvail
                                ? currentTime + travelTime
                                : _clients[m].TimeAvail + travelTime;

                            nextTime += _clients[m].Duration;
                            areAllTooBig = false;
                            _route.Add(_clients[m]);
                            _used[m] = true;
                            process(k + 1, currentLoad - _clients[m].Demand, nextTime, currentVehicle, newCheckedNearDepot, tripStartTime);
                            _used[m] = false;
                            _route.RemoveAt(_route.Count - 1);
                        }
                    }
                }
            }
        }

        public IEnumerable<Client> BestRoute { get { return _bestRoute; } }
        private double GetReversedTimeSinceRecentDepotWithLastDepot(List<Client> route, double tripStartTime)
        {
            route.Add(_clients[0]);
            double result = GetReversedTimeSinceRecentDepot(route, tripStartTime);
            route.RemoveAt(route.Count - 1);
            return result;
        }

        private double GetReversedTimeSinceRecentDepot(List<Client> route, double tripStartTime)
        {
            if (route.Count == 1)
                return double.MaxValue;
            double time = tripStartTime;

            for (int i = route.Count - 1; ; i--)//jak zaczynamy to jesteśmy w depocie
            {
                if (route[i].Id == 0 && i != route.Count - 1)
                    return time;
                double timeBetween = route[i].CostToGetTo(route[i - 1]);


                if (time < route[i-1].TimeAvail)
                    time = route[i-1].TimeAvail + timeBetween;
                else
                    time += timeBetween;
                time += route[i - 1].Duration;
            }
        }

        private void Reset()
        {
            _route = new List<Client>();
            _bestRoute = new List<Client>();
            _cost = int.MaxValue;
            _used = new bool[N];

            _capacity = Capacities;
            //_currentLoad = _capacity;

            _clients[0].Demand = 0;
            _clients[0].TimeAvail = 0;
            _clients[0].Duration = 0;
            _addedTimes = new List<double>();

            foreach (var client in _clients)
            {
                client.Demand = Math.Abs(client.Demand);
            }
            t = DateTime.Now;
            _vehiclesTimes = new double[NumVehicles];
            _timeWindow = DepotTimeWindowSection[Depots.First()].EndTime;
        }

        private void InitializeClients()
        {
            _clients = new Client[VisistLocationSection.Count + 1];
            _clients[0] = new Client() {Id=0, Point = LocationCoordSection[DepotLocationSection[0]], Demand = 0, TimeAvail = DepotTimeWindowSection[0].StartTime, Duration = 0 };
            for (int i = 1; i < _clients.Length; i++)
                _clients[i] = new Client() {Id=i, Point = LocationCoordSection[VisistLocationSection[i]], Demand = DemandSection[i], Duration = DurationSection[i], TimeAvail = TimeAvailSection[i] };
        }

        private bool _previousZero
        {
            get { return _route[_route.Count - 1].Id == 0; }
        }

        double GetCost(List<Client> route,bool b=false)
        {
            double cost = 0;
            double unloadCost = 0;
            for (int i = 0; i < route.Count; i++)
            {
                cost += route[i].CostToGetTo((route[(i + 1) % route.Count]));
                unloadCost += route[i].Duration;
            }
            if (b && cost < _minCost)
                    _minCost = cost;
            return cost / TimeStep + unloadCost + _addedTimes.Sum();
        }

        private double CostOfRoute(Client[] Route)
        {
            // go through each edge in the route and add up the cost. 
            int x;
            Client here;
            double cost = 0D;

            for (x = 0; x < Route.Length - 1; x++)
            {
                here = Route[x];
                cost += here.CostToGetTo(Route[x + 1]);
            }
            return cost;
        }


        private void CutOffTime()
        {
            var maxRequestTime = CUT_OFF_TIME * DepotTimeWindowSection.Values.Select(v => v.EndTime).Max();
            var tempDic = new Dictionary<int, int>();
            foreach (var pair in TimeAvailSection)
            {
                int requestTime = pair.Value;
                if (requestTime > maxRequestTime)
                    requestTime = 0;
                tempDic.Add(pair.Key, requestTime);
            }
            TimeAvailSection = tempDic;
        }
        public bool IsItPossibleToSolve()
        {
            CutOffTime();
            InitializeClients();
            Reset();

            int maxTime = DepotTimeWindowSection[Depots.First()].EndTime;
            foreach (var client in _clients)
            {
                if (client.TimeAvail + client.Duration + client.CostToGetTo(_clients[0])/TimeStep > maxTime)
                    return false;
            }
            return true;
        }
    }
}

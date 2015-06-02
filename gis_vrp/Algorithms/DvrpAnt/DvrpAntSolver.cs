using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web;
using gis_vrp.Models;

namespace gis_vrp.Algorithms.DvrpAnt
{
    public class DvrpAntSolver : IVehicleRouteFinder
    {
        private const double PheromoneWeight = 2;
        private const double DistanceWeight = 6;
        private const double PheromoneEvaporateValue = 0.25;

        private IEnumerable<Point> _points;
        private int[,] _distances;
        private IEnumerable<Edge> _edges;
        private IEnumerable<Edge> _candidatesList;
        private IEnumerable<Ant> _ants; 
        private Stopwatch _clock=new Stopwatch();
        private double _initialPheromone;
        private IEnumerable<Edge> _bestPath;
        private double _bestPathCost=double.MaxValue;
        private DateTime _bestDate = DateTime.MaxValue;
        private int _clientsCount;// { get { return _points.Count() - 1; } }
        private int _capacity=100;
        //private int[] _clientsDemand;
        private int _bestRouteNumber = 0;
        private IEnumerable<int> _depots; 

        private IDvrpProgressNotification _dvrpProgressNotification;

        public DvrpAntSolver(IDvrpProgressNotification dvrpProgressNotification=null)
        {
            _dvrpProgressNotification = dvrpProgressNotification;
        }

        public IEnumerable<int> FindRoute(IEnumerable<Point> points, int[,] distances,TimeSpan timeLimit)
        {
            var finishedSize = 0;
            var count = 0;
            _points = points;
            _distances = distances;
            InitGenerallData();
            InitGraph();
            InitPheromone();
           // InitDemands();
            InitAnts();
            _clock.Start();
            while (_clock.ElapsedMilliseconds < timeLimit.TotalMilliseconds)
            {
                var finished = new List<Ant>();
                foreach (var ant in _ants)
                {
                    if (ant.IsFinished) continue;
                    bool fullPath = false;
                    FillCandidatesList(ant.CurrentClient);
                    Edge nextEdge = SelectVertexToVisist(ant);
                    if (nextEdge == null)
                    {
                        if (_depots.All(d => d != ant.CurrentClient)) //ant.CurrentClient != 0) 
                        {
                            var edge = GetShortestReturnEdgeToDepot(_points.ElementAt(ant.CurrentClient));
                            ant.MoveTo(edge);//_edges.First(e => e.From == ant.CurrentClient && _depots.Any(d=>d==e.To)));
                            ant.CurrentLoad = _capacity;
                            ant.CurrentTime = GetDateTimeArriveToClientAndUnload(ant.CurrentTime, edge);// ant.CurrentTime.AddMinutes(edge.Distance);
                        }
                        int to = FirstNotVisitedClient(ant);
                        if (to == -1)
                        {                           
                            //fullPath = true;
                            finishedSize++;
                            ant.IsFinished = true;
                            finished.Add(ant);
                            ant.CurrentLoad = _capacity;
                            ant.FoundPath = ant.VisistedClients;
                        }
                        else
                        {
                            var point = _points.ElementAt(to);
                            var edge = _edges.FirstOrDefault(e => e.From == ant.CurrentClient && e.To == to);
                           // if (point.OccurTime > ant.CurrentTime)
                            //    ant.CurrentTime = point.OccurTime;

                            if (edge != null)
                            {
                                ant.MoveTo(edge);
                                ant.CurrentLoad -= _points.ElementAt(to).Capacity;
                                ant.CurrentTime = GetDateTimeArriveToClientAndUnload(ant.CurrentTime, edge);// ant.CurrentTime.AddMinutes(edge.Distance + point.UnloadTime);
                            }
                            //else
                            //{
                                
                            //}
                        }
                    }
                    else
                    {
                        ant.MoveTo(nextEdge);
                        ant.CurrentLoad -= _points.ElementAt(nextEdge.To).Capacity;
                        ant.CurrentTime = GetDateTimeArriveToClientAndUnload(ant.CurrentTime, nextEdge);// ant.CurrentTime.AddMinutes(nextEdge.Distance + _points.ElementAt(nextEdge.To).UnloadTime);
                        UpdatePheromoneLocal(nextEdge);
                    }                
                        
                    //fixme: mozna zastosowac heurystyke 2-OPT
                }

                if (finishedSize == _ants.Count())
                {
                    count++;
                    UpdateBestPath(finished);//.Select(a => a.FoundPath));
                    UpdatePheromoneGlobalElitist(finished);
                    //finished.ForEach(a => a.Reset());
                    //finished.ForEach(a => a.CurrentLoad = _capacity - _points.ElementAt(a.CurrentClient).Capacity);
                    InitAnts();
                    finished.Clear();
                    finishedSize = 0;
                }
               
            }

            var result = new List<int>(_bestPath.Select(e => e.From));
            var ed = GetShortestReturnEdgeToDepot(_points.ElementAt(result.Last()));
            result.Add(ed.To);
            var depots = DevideDepots(result);
            return depots.SelectMany(d => d);
        }

        private void InitGraph()
        {
            var edges = new List<Edge>(_points.Count()*(_points.Count() - 1));
            for(int i=0;i<_points.Count();i++)
                for (int j = 0; j < _points.Count(); j++)
                    if (i != j)
                    {
                        var edge = new Edge() {From = i, To = j};
                        edge.Distance = i < j ? _distances[i, j] : _distances[j, i];
                        edge.Savings = 0;// CalculateClarkWrightSaving(i, j);
                        if (edge.Savings < 1)
                            edge.N = 1/edge.Distance;
                        else
                            edge.N = edge.Savings/edge.Distance;
                        edges.Add(edge);
                    }

            _edges = edges;
        }

        private double CalculateClarkWrightSaving(int i1, int i2)
        {
            int distance1 = _distances[_depots.First(), i1];
            int distance2 = _distances[_depots.First(), i2];
            int distanceBetween = _distances[i1, i2];
            return 2*distance1 + 2*distance2 - (distance1 + distance2 + distanceBetween);
        }

        private void InitGenerallData()
        {
            _clientsCount = _points.Count(p => p.Type == PointType.Client);
            var list = new List<int>();
            for (int i = 0; i < _points.Count(); i++)
            {
                var point = _points.ElementAt(i);
                if (point.Type == PointType.Depot)
                {
                    list.Add(i);
                    point.UnloadTime = 0;
                }
                point.Id = i;
            }

            _depots = list;
        }

        private void InitPheromone()
        {
            _initialPheromone = 1.0/(_clientsCount); //TODO

            foreach (var edge in _edges)
            {
                edge.Pheromone = _initialPheromone;
            }
        }

        private void InitAnts()
        {            
            var ants = new List<Ant>();
            var minDate = _points.Where(p => p.Type == PointType.Client).Min(p => p.OccurTime);
            foreach (var point in _points)
            {
                if (point.Type == PointType.Client && point.OccurTime == minDate)
                {
                    var edge =GetShortestEdgeForClientFromNearestDepot(point);
                    var ant = new Ant() { CurrentLoad = _capacity - point.Capacity,CurrentTime = GetDateTimeArriveToClientAndUnload(_points.ElementAt(edge.To).OccurTime,edge) };
                    ant.MoveTo(edge);
                    ants.Add(ant);
                }
            }
            //for (int i = 0; i < _clientsCount; i++)
            //{
            //    var ant = new Ant() {CurrentLoad = _capacity - _clientsDemand[i + 1]};
            //    ant.MoveTo(_edges.ElementAt(i));
            //    ants.Add(ant);
            //}
            _ants = ants;
        }

        private void FillCandidatesList(int from)
        {
            var sumAttractiveness = 0.0;
            foreach (var edge in _edges)
            {
                edge.AttractivenessValue = Math.Pow(edge.Pheromone, PheromoneWeight)*Math.Pow(edge.N, DistanceWeight);
                sumAttractiveness += edge.AttractivenessValue;
            }

            var candidatesList = new List<Edge>();
            foreach (var edge in _edges)
            {
                edge.Probability = edge.AttractivenessValue/sumAttractiveness;
                if (edge.From == from)
                    candidatesList.Add(edge);
            }

            candidatesList.Sort(new Comparison<Edge>((e1, e2) => e1.Probability < e2.Probability ? 1 : e1.Probability == e2.Probability ? 0 : -1));
            _candidatesList = candidatesList;
        }

        private Edge SelectVertexToVisist(Ant ant)
        {
            foreach (var edge in _candidatesList)
            {
                if (ant.VisistedClients.Count(e=> e==edge || e.From ==edge.To || e.To==edge.To)==0)
                {
                    var point = _points.ElementAt(edge.To);
                    if (ant.CurrentLoad - point.Capacity  >= 0 && ant.CurrentTime>=  point.OccurTime)                    //TODO  czas
                        return edge;                  
                }
            }
            return null;
        }

        private int FirstNotVisitedClient(Ant ant)
        {
            var list = new List<Point>();
            for (int i = 0; i < _points.Count(); i++)
            {
                var p = _points.ElementAt(i);
                if (p.Type == PointType.Client)
                {
                    if (ant.VisistedClients.Count(e => e.From == i || e.To == i) == 0)
                    {
                        if (ant.CurrentTime >= p.OccurTime)
                            return i;
                        list.Add(p);
                        //return i; 
                    }
                    
                }
            }
            if (list.Count > 0)
            {
                var min = list.Min(p => p.OccurTime);
                return list.First(p => p.OccurTime == min).Id;
            }
            //for (int i = 1; i <= _clientsCount; i++)
            //{
            //    if (ant.VisistedClients.Count(e=>e.From==i || e.To==i)==0)
            //        return i;
            //}
            return -1;
        }

        private void UpdatePheromoneLocal(Edge choosenEdge)
        {
            choosenEdge.Pheromone = (1 - PheromoneEvaporateValue)*choosenEdge.Pheromone;
        }

        private void UpdatePheromoneGlobalElitist(IEnumerable<Ant> ants )
        {
            foreach (var edge in _edges)
            {
                edge.Pheromone = (1 - PheromoneEvaporateValue) * edge.Pheromone;
                edge.Pheromone = Math.Max(edge.Pheromone, _initialPheromone*0.3);
            }

            foreach (var ant in ants)
            {
                if (ant.FoundPath != null)
                {
                    var cost = CalculateDistance(ant.FoundPath);
                    foreach (var edge in ant.FoundPath)
                    {
                        edge.Pheromone += 1/cost;
                        if (_bestPath.Contains(edge))
                            edge.Pheromone += _clientsCount*1/_bestPathCost;
                    }
                }
            }
        }

        private double CalculateDistance(IEnumerable<Edge> path)
        {
            var sum = 0.0;
            foreach (var edge in path)
            {
                sum += edge.Distance;
            }
            return sum;
        }

        private void UpdateBestPath(IEnumerable<Ant> paths)
        {
            foreach (var path in paths)
            {
                var cost = CalculateDistance(path.FoundPath);
                if (cost < _bestPathCost || (cost==_bestPathCost && path.CurrentTime < _bestDate))
                {
                    _bestPathCost = cost;
                    _bestPath = path.FoundPath;
                    _bestDate = path.CurrentTime;
                    Debug.WriteLine("{0} {1} {2}", _bestPathCost, _clock.Elapsed.Seconds, _bestDate);
                    //_bestRouteNumber++;
                    if (_dvrpProgressNotification != null)
                    {
                        var result = new List<int>(_bestPath.Select(e => e.From));
                        var edge = GetShortestReturnEdgeToDepot(_points.ElementAt(result.Last()));
                        result.Add(edge.To);
                        var depots = DevideDepots(result);
                        Debug.WriteLine("num of routes {0}",depots.Count());
                        foreach (var depot in depots)
                        {
                            _dvrpProgressNotification.Notify((++_bestRouteNumber).ToString(), depot.ToList());
                        }
                        
                    }
                }
            }
        }

        //private void InitDemands()
        //{
        //    _clientsDemand = new int[_clientsCount+1];
        //    for (int i = 1; i < _clientsDemand.Length; i++)
        //        _clientsDemand[i] = _points.ElementAt(i).Capacity;
        //}

        private Edge GetShortestEdgeForClientFromNearestDepot(Point point)
        {
            var list = _edges.Where(e => e.To == point.Id && _depots.Any(d => d == e.From));
            return FindMinEdge(list);
        }

        private Edge GetShortestReturnEdgeToDepot(Point point)
        {
            var list = _edges.Where(e => e.From == point.Id && _depots.Any(d => d == e.To));
            return FindMinEdge(list);
        }

        private Edge FindMinEdge(IEnumerable<Edge> list)
        {
            Edge ed = null;
            var distance = double.MaxValue;
            foreach (var edge in list)
            {
                if (edge.Distance < distance)
                {
                    distance = edge.Distance;
                    ed = edge;
                }
            }
            return ed;
        }

        private IEnumerable<IEnumerable<int>> DevideDepots(IList<int> list)
        {
            var depots = new List<List<int>>();
            if (list.Count == 0) return depots;
            Debug.WriteLine(string.Join(" ", list));
            var current = new List<int>();
            var fromLastDepot = new List<int>();
            int idLastDepot = list.First();
            current.Add(idLastDepot);
            for (int i = 1; i < list.Count; i++)
            {
                var point = _points.ElementAt(list[i]);
                if (point.Type == PointType.Depot && point.Id == idLastDepot && fromLastDepot.Count>0)
                {
                    fromLastDepot.Add(point.Id);
                    current.AddRange(fromLastDepot);      
                    fromLastDepot = new List<int>();
                }
                else if (point.Type == PointType.Client)
                {
                    var edge = GetShortestReturnEdgeToDepot(point);
                    if (edge.To == idLastDepot)
                    {
                        fromLastDepot.Add(point.Id);
                    }
                    else
                    {
                        if(fromLastDepot.Count ==0 || fromLastDepot.Last() != idLastDepot)
                            fromLastDepot.Add(idLastDepot);
                        current.AddRange(fromLastDepot);
                        var exist = depots.FirstOrDefault(l => l.First() == idLastDepot);
                        if (exist != null)
                        {
                            exist.AddRange(current.Skip(1));
                            Debug.WriteLine(string.Join(" ", exist));
                        }
                        else
                        {
                            depots.Add(current);
                            Debug.WriteLine(string.Join(" ", current));
                        }
                       

                        current = new List<int>();
                        fromLastDepot = new List<int>();
                        idLastDepot = edge.To;
                        current.Add(idLastDepot);
                        current.Add(point.Id);
                    }
                }
                else if (point.Type == PointType.Depot && i==list.Count-1)
                {
                    fromLastDepot.Add(idLastDepot);
                    current.AddRange(fromLastDepot);
                    fromLastDepot = new List<int>();
                }
            }

            var e = depots.FirstOrDefault(l => l.First() == idLastDepot);
            if (e != null)
            {
                e.AddRange(current.Skip(1));
                Debug.WriteLine(string.Join(" ",e));
            }
            else
            {
                depots.Add(current);
                Debug.WriteLine(string.Join(" ", current));
            }

            return depots;
        }


        private DateTime GetDateTimeArriveToClientAndUnload(DateTime current,Edge edge)
        {
            var p = _points.ElementAt(edge.To);
            if (current < p.OccurTime)
                current = p.OccurTime;
            current = current.AddMinutes(edge.Distance);
            current = current.AddMinutes(p.UnloadTime);
            return current;
        }
    }
}
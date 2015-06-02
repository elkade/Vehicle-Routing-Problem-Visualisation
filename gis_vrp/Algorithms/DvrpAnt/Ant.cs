using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gis_vrp.Algorithms.DvrpAnt
{
    public class Ant
    {
        public Ant()
        {
            VisistedClients = new List<Edge>();
        }
        public int CurrentClient { get; set; }
        public IList<Edge> VisistedClients { get; set; }
        public IList<Edge> FoundPath { get; set; }

        public double CurrentLoad { get; set; }
        public bool IsFinished { get; set; }
        public DateTime CurrentTime { get; set; }
        public void MoveTo(Edge edge)
        {
            CurrentClient = edge.To;
            VisistedClients.Add(edge);
        }

        public void Reset()
        {
            var firstEdge = VisistedClients[0];
            CurrentClient = firstEdge.To;
            VisistedClients=new List<Edge>();
            VisistedClients.Add(firstEdge);
            IsFinished = false;
        }
    }
}
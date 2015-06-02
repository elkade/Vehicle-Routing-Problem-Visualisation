using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gis_vrp.Algorithms.DvrpAnt
{
    public class Edge
    {
        public int From { get; set; }
        public int To { get; set; }
        public double Distance { get; set; }
        public double Savings { get; set; }
        public double Pheromone { get; set; }
        public double AttractivenessValue { get; set; }
        public double Probability { get; set; }
        public double N { get; set; }
    }
}
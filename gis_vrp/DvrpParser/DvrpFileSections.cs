using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVRP
{
    /// <summary>
    /// Klasa reprezentujaca sekcje pliku zawierajacego opis problemu DVRP
    /// </summary>
    internal static class DvrpFileSections
    {
        public const string DATA_SECTION = "DATA_SECTION";
        public const string NAME = "NAME";
        public const string NUM_DEPOTS = "NUM_DEPOTS";
        public const string NUM_CAPACITIES = "NUM_CAPACITIES";
        public const string NUM_VISITS = "NUM_VISITS";
        public const string NUM_LOCATIONS = "NUM_LOCATIONS";
        public const string NUM_VEHICLES = "NUM_VEHICLES";
        public const string CAPACITIES = "CAPACITIES";
        public const string DEPOTS = "DEPOTS";
        public const string DEMAND_SECTION = "DEMAND_SECTION";
        public const string LOCATION_COORD_SECTION = "LOCATION_COORD_SECTION";
        public const string DEPOT_LOCATION_SECTION = "DEPOT_LOCATION_SECTION";
        public const string VISIT_LOCATION_SECTION = "VISIT_LOCATION_SECTION";
        public const string DURATION_SECTION = "DURATION_SECTION";
        public const string DEPOT_TIME_WINDOW_SECTION = "DEPOT_TIME_WINDOW_SECTION";
        public const string TIME_AVAIL_SECTION = "TIME_AVAIL_SECTION";
        public const string COMMENT_TIMESTEP = "COMMENT: TIMESTEP:";
        public const string COMMENT = "COMMENT";
        public const string TIMESTEP = "TIMESTEP:";
        public const string EOF = "EOF";
    }
}

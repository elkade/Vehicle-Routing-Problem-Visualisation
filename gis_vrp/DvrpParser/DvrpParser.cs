using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gis_vrp.Algorithms.Dvrp;

namespace DVRP
{
    /// <summary>
    /// Klasa parsujaca dane problemu DVRP
    /// </summary>
    internal class DvrpParser
    {
        private Dvrp _dvrp;

        /// <summary>
        /// Metoda parsujaca problem DVRP
        /// </summary>
        /// <param name="content">Dane o DVRP</param>
        /// <returns>Instancja DVRP</returns>
        public Dvrp Parse(string content)
        {
            _dvrp = new Dvrp();
            ParseDvrpParameters(content);
            return _dvrp;
        }

        private void ParseDvrpParameters(string content)
        {
            int dataSection = content.IndexOf(DvrpFileSections.DATA_SECTION, System.StringComparison.Ordinal);
            var data = content.Split(new []{"DATA_SECTION"}, StringSplitOptions.RemoveEmptyEntries);
            if (data.Count() < 2)
                throw new Exception("File has invalid format");
            ParseGenerallParameters(data[0]);
            string dataSectionContent = data[1];
            int depotsStart = dataSectionContent.IndexOf(DvrpFileSections.DEPOTS, System.StringComparison.Ordinal) + DvrpFileSections.DEPOTS.Length + 1;
            int depotsEnd = dataSectionContent.IndexOf(DvrpFileSections.DEMAND_SECTION, System.StringComparison.Ordinal) - 1;
            int demandSectionStart = dataSectionContent.IndexOf(DvrpFileSections.DEMAND_SECTION, System.StringComparison.Ordinal)+DvrpFileSections.DEMAND_SECTION.Length+1;
            int demandSectionEnd = dataSectionContent.IndexOf(DvrpFileSections.LOCATION_COORD_SECTION, System.StringComparison.Ordinal) - 1;
            int locationCoordSectionStart = dataSectionContent.IndexOf(DvrpFileSections.LOCATION_COORD_SECTION, System.StringComparison.Ordinal) +
                                       DvrpFileSections.LOCATION_COORD_SECTION.Length + 1;
            int locationCoordSectionEnd = dataSectionContent.IndexOf(DvrpFileSections.DEPOT_LOCATION_SECTION, System.StringComparison.Ordinal) - 1;
            int depotLocationSectionStart = dataSectionContent.IndexOf(DvrpFileSections.DEPOT_LOCATION_SECTION, System.StringComparison.Ordinal) +
                                       DvrpFileSections.DEPOT_LOCATION_SECTION.Length + 1;
            int depotLocationSectionEnd = dataSectionContent.IndexOf(DvrpFileSections.VISIT_LOCATION_SECTION, System.StringComparison.Ordinal) - 1;
            int visitLocationSectionStart = dataSectionContent.IndexOf(DvrpFileSections.VISIT_LOCATION_SECTION, System.StringComparison.Ordinal) +
                                       DvrpFileSections.VISIT_LOCATION_SECTION.Length + 1;
            int visitLocationSectionEnd = dataSectionContent.IndexOf(DvrpFileSections.DURATION_SECTION, System.StringComparison.Ordinal) - 1;
            int durationSectionStart = dataSectionContent.IndexOf(DvrpFileSections.DURATION_SECTION, System.StringComparison.Ordinal) +
                                  DvrpFileSections.DURATION_SECTION.Length + 1;
            int durationSectionEnd = dataSectionContent.IndexOf(DvrpFileSections.DEPOT_TIME_WINDOW_SECTION, System.StringComparison.Ordinal) - 1;
            int depotTimeWindowSectionStart = dataSectionContent.IndexOf(DvrpFileSections.DEPOT_TIME_WINDOW_SECTION, System.StringComparison.Ordinal) +
                                         DvrpFileSections.DEPOT_TIME_WINDOW_SECTION.Length + 1;
            int depotTimeWindowSectionEnd = dataSectionContent.IndexOf(DvrpFileSections.COMMENT, System.StringComparison.Ordinal) - 1;
            int commentTimeStepStart = dataSectionContent.IndexOf(DvrpFileSections.TIMESTEP, System.StringComparison.Ordinal) + DvrpFileSections.TIMESTEP.Length +
                                  1;
            int commentTimeStepEnd = dataSectionContent.IndexOf(DvrpFileSections.TIME_AVAIL_SECTION, System.StringComparison.Ordinal) - 1;
            int timeAvailSectionStart = dataSectionContent.IndexOf(DvrpFileSections.TIME_AVAIL_SECTION, System.StringComparison.Ordinal) +
                                   DvrpFileSections.TIME_AVAIL_SECTION.Length + 1;
            int timeAvailSectionEnd = dataSectionContent.IndexOf(DvrpFileSections.EOF, System.StringComparison.Ordinal) - 1;

            ParseDepots(dataSectionContent.Substring(depotsStart,depotsEnd-depotsStart));
            ParseToDictionaryInt(dataSectionContent.Substring(demandSectionStart,demandSectionEnd-demandSectionStart),2,_dvrp.DemandSection);
            ParseLocationCoordSection(dataSectionContent.Substring(locationCoordSectionStart, locationCoordSectionEnd - locationCoordSectionStart));
            ParseToDictionaryInt(dataSectionContent.Substring(depotLocationSectionStart, depotLocationSectionEnd - depotLocationSectionStart),2,_dvrp.DepotLocationSection);
            ParseToDictionaryInt(dataSectionContent.Substring(visitLocationSectionStart, visitLocationSectionEnd - visitLocationSectionStart),2,_dvrp.VisistLocationSection);
            ParseToDictionaryInt(dataSectionContent.Substring(durationSectionStart, durationSectionEnd - durationSectionStart),2,_dvrp.DurationSection);
            ParseDepotTimeWindowSection(dataSectionContent.Substring(depotTimeWindowSectionStart, depotTimeWindowSectionEnd - depotTimeWindowSectionStart));
            ParseToDictionaryInt(dataSectionContent.Substring(timeAvailSectionStart, timeAvailSectionEnd - timeAvailSectionStart),2,_dvrp.TimeAvailSection);
            _dvrp.TimeStep = double.Parse(dataSectionContent.Substring(commentTimeStepStart, commentTimeStepEnd - commentTimeStepStart));
        }

        private void ParseGenerallParameters(string content)
        {
            var lines = content.Split('\n');
            foreach (var line in lines)
            {
                var data = line.Split(':');
                if (data.Count() < 2)
                    continue;
                var key = data[0];
                var value = data[1];
                if (key.StartsWith("NAME"))
                    _dvrp.Name = value;
                else if (key.StartsWith("NUM_DEPOTS"))
                    _dvrp.NumDepots = int.Parse(value);
                else if (key.StartsWith("NUM_CAPACITIES"))
                    _dvrp.NumCapacities = int.Parse(value);
                else if (key.StartsWith("NUM_VISITS"))
                    _dvrp.NumVisits = int.Parse(value);
                else if (key.StartsWith("NUM_LOCATIONS"))
                    _dvrp.NumLocations = int.Parse(value);
                else if (key.StartsWith("NUM_VEHICLES"))
                    _dvrp.NumVehicles = int.Parse(value);
                else if (key.StartsWith("CAPACITIES"))
                    _dvrp.Capacities = int.Parse(value);
            }
        }

        private void ParseDepots(string content)
        {
            _dvrp.Depots = new List<int>();
            var lines = content.Split('\n');
            foreach (string line in lines)
            {
                int i;
                if(int.TryParse(line, out i))
                    _dvrp.Depots.Add(i);
            }
        }

        private void ParseToDictionaryInt(string content, int count, Dictionary<int, int> dictionary)
        {
            var lines = content.Split('\n');
            foreach (string l in lines)
            {
                var line = l.TrimStart(' ');
                var numbers = line.Split(' ');
                if (numbers.Count() < count)
                    continue;
                dictionary.Add(int.Parse(numbers[0]), int.Parse(numbers[1]));
            }
        }

        //private void ParseDemandSection(string content)
        //{
        //    _dvrp.DemandSection = new Dictionary<int, int>();
        //    var lines = content.Split('\n');
        //    foreach (string l in lines)
        //    {
        //        var line =l.TrimStart(' ');
        //        var numbers = line.Split(' ');
        //        if(numbers.Count()<2)
        //            continue;
        //        _dvrp.DemandSection.Add(int.Parse(numbers[0]),int.Parse(numbers[1]));
        //    }
        //}

        private void ParseLocationCoordSection(string content)
        {
            _dvrp.LocationCoordSection = new Dictionary<int, Point>();
            var lines = content.Split('\n');
            foreach (string l in lines)
            {
                var line = l.TrimStart(' ');
                var numbers = line.Split(' ');
                if (numbers.Count() < 3)
                    continue;
                int key = int.Parse(numbers[0]);
                int x =int.Parse(numbers[1]);
                int y = int.Parse(numbers[2]);
                _dvrp.LocationCoordSection.Add(key, new Point(x, y));
            }
        }

        //private void ParseDepotLocationSection(string content)
        //{
        //    _dvrp.DepotLocationSection = new Dictionary<int, int>();
        //    var lines = content.Split('\n');
        //    foreach (string l in lines)
        //    {
        //        var line = l.TrimStart(' ');
        //        var numbers = line.Split(' ');
        //        if (numbers.Count() < 2)
        //            continue;
        //        int key = int.Parse(numbers[0]);
        //        int value = int.Parse(numbers[1]);
        //        _dvrp.DepotLocationSection.Add(key, value);
        //    }
        //}

        //private void ParseVisitLocationSection(string content)
        //{
        //    _dvrp.VisistLocationSection = new Dictionary<int, int>();
        //    var lines = content.Split('\n');
        //    foreach (string l in lines)
        //    {
        //        var line = l.TrimStart(' ');
        //        var numbers = line.Split(' ');
        //        if (numbers.Count() < 2)
        //            continue;
        //        int key = int.Parse(numbers[0]);
        //        int value = int.Parse(numbers[1]);
        //        _dvrp.VisistLocationSection.Add(key, value);
        //    }
        //}

        //private void ParseDurationSection(string content)
        //{
        //    _dvrp.DurationSection = new Dictionary<int, int>();
        //    var lines = content.Split('\n');
        //    foreach (string l in lines)
        //    {
        //        var line = l.TrimStart(' ');
        //        var numbers = line.Split(' ');
        //        if (numbers.Count() < 2)
        //            continue;
        //        int key = int.Parse(numbers[0]);
        //        int value = int.Parse(numbers[1]);
        //        _dvrp.DurationSection.Add(key, value);
        //    }
        //}

        private void ParseDepotTimeWindowSection(string content)
        {
            _dvrp.DepotTimeWindowSection = new Dictionary<int, TimeWindow>();
            var lines = content.Split('\n');
            foreach (string l in lines)
            {
                var line = l.TrimStart(' ');
                var numbers = line.Split(' ');
                if (numbers.Count() < 3)
                    continue;
                int key = int.Parse(numbers[0]);
                int start = int.Parse(numbers[1]);
                int end = int.Parse(numbers[2]);
                _dvrp.DepotTimeWindowSection.Add(key, new TimeWindow(start,end));
            }
        }

        //private void ParseTimeAvailSection(string content)
        //{
        //    _dvrp.TimeAvailSection = new Dictionary<int, int>();
        //    var lines = content.Split('\n');
        //    foreach (string l in lines)
        //    {
        //        var line = l.TrimStart(' ');
        //        var numbers = line.Split(' ');
        //        if (numbers.Count() < 2)
        //            continue;
        //        int key = int.Parse(numbers[0]);
        //        int value = int.Parse(numbers[1]);
        //        _dvrp.TimeAvailSection.Add(key, value);
        //    }
        //}
    }
}

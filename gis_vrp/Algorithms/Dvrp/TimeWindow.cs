namespace gis_vrp.Algorithms.Dvrp
{
    internal class TimeWindow
    {
        public int StartTime;
        public int EndTime;

        public TimeWindow(int start, int end)
        {
            StartTime = start;
            EndTime = end;
        }
    }
}

using System.Collections.Generic;

namespace gis_vrp.Algorithms
{
    public interface IDvrpProgressNotification
    {
        void Notify(string title,List<int> content);
    }
}

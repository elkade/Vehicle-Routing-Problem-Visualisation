using System.Threading;

namespace gis_vrp.Services
{
    public interface ITestService
    {
        void Calculate();
    }

    public class TestService : ITestService
    {
        public void Calculate()
        {
            Thread.Sleep(2010);
        } 
    }
}
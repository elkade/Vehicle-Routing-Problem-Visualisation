using System.Web.Mvc;

namespace gis_vrp.Controllers
{
    public class HomeController : AsyncController
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
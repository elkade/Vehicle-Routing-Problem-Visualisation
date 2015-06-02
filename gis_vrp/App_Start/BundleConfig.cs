using System.Web.Optimization;

namespace gis_vrp
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                    "~/Scripts/jquery-{version}.min.js",
                    "~/Scripts/jquery-ui.min.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/signalR").Include(
                    "~/Scripts/jquery.signalR-{version}.min.js"
                ));
            
            bundles.Add(new ScriptBundle("~/bundles/mapScript").Include(
                    "~/Scripts/mapScript.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/metro").Include(
                    "~/Scripts/metro.min.js"
                ));

            bundles.Add(new StyleBundle("~/bundles/styles").Include(
                    "~/Styles/css/style.css"
                ));

            bundles.Add(new StyleBundle("~/bundles/css/metro").Include(
                    "~/Styles/css/metro.min.css",
                    "~/Styles/css/metro-icons.min.css"
                ));

            BundleTable.EnableOptimizations = true;
        }
    }
}
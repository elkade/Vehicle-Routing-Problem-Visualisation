using System;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using gis_vrp.Algorithms;
using gis_vrp.Algorithms.DvrpAnt;
using gis_vrp.Hubs;
using Microsoft.AspNet.SignalR;
using RegistrationExtensions = Autofac.Integration.SignalR.RegistrationExtensions;

namespace gis_vrp
{
    public class Global : HttpApplication
    {   
        protected void Application_Start(object sender, EventArgs e)
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);


            var builder = new ContainerBuilder();

            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly()).AsImplementedInterfaces();
            builder.RegisterControllers(Assembly.GetExecutingAssembly());

            builder.RegisterType<DvrpAntSolver>().As<IVehicleRouteFinder>();

            RegistrationExtensions.RegisterHubs(builder);

            builder.RegisterType<TestHub>();
            builder.RegisterType<DvrpRouteHub>();

            var container = builder.Build();

            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            GlobalHost.DependencyResolver = new Autofac.Integration.SignalR.AutofacDependencyResolver(container);
        }
    }
}
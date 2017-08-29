using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace PayNearMe
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //routes.MapRoute(
            //    name: "Login",
            //    url: "LogIn",
            //    defaults: new { controller = "Account", action = "Login"}
            //    //defaults: new { controller = "Account", action = "Login", id = UrlParameter.Optional }
            //);

            //routes.MapRoute(
            //    name: "RedirectU",
            //    url: "RedirectU",
            //    defaults: new { controller = "Account", action = "RedirectUniteller", id = UrlParameter.Optional }
            //);

            //routes.MapRoute(
            //    name: "Authenticate",
            //    url: "Verify",
            //    defaults: new { controller = "Register", action = "Authenticate", id = UrlParameter.Optional }
            //);

            //routes.MapRoute(
            //    name: "Home",
            //    url: "Home",
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            //);

           // routes.MapRoute(
           //     name: "Profile",
           //     url: "Profile",
           //     defaults: new { controller = "Profile", action = "Index", id = UrlParameter.Optional }
           // );

           // routes.MapRoute(
           //    name: "Benefeciary",
           //    url: "Benefeciary",
           //    defaults: new { controller = "Benefeciary", action = "Index", id = UrlParameter.Optional }
           //);

           // routes.MapRoute(
           //    name: "Benefeciaries",
           //    url: "Benefeciaries",
           //    defaults: new { controller = "Benefeciary", action = "Benefeciaries", id = UrlParameter.Optional }
           //);

           // routes.MapRoute(
           //     name: "Register",
           //     url: "Registration",
           //     defaults: new { controller = "Register", action = "Index", id = UrlParameter.Optional }
           // );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Account", action = "Login", id = UrlParameter.Optional }
            );
        }
    }
}

using System;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;


namespace PayNearMe.App_Start
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // TODO: Add any additional configuration code.

            // Web API routes
            config.MapHttpAttributeRoutes();

            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{action}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);

            config.Routes.MapHttpRoute(
              name: "Api UriPathExtension ID",
              routeTemplate: "api/{controller}/{ext}/{action}/{id}",
              defaults: new { id = RouteParameter.Optional, extension = RouteParameter.Optional }
            ); 


            //GlobalConfiguration.Configuration.Formatters.JsonFormatter.MediaTypeMappings
            //                    .Add(new RequestHeaderMapping("Accept",
            //                  "text/html",
            //                  StringComparison.InvariantCultureIgnoreCase,
            //                  true,
            //                  "application/xml"));
          //  config.Formatters.XmlFormatter.WriterSettings.OmitXmlDeclaration = false;
            config.Formatters.JsonFormatter.AddUriPathExtensionMapping("json", "application/json");
            config.Formatters.XmlFormatter.AddUriPathExtensionMapping("xml", "text/xml");
            

            // //WebAPI when dealing with JSON & JavaScript!
            // //Setup json serialization to serialize classes to camel (std. Json format)
            //var formatter = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            //formatter.SerializerSettings.ContractResolver =
            //    new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
        }
    }
}
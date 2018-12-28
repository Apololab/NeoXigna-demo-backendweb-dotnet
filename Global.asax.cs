using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Apololab.Common.Http;
using Apololab.Common.Http.Rest;

namespace NeoXignaDemo
{
    public class Global : HttpApplication
    {
        public const string API_KEY = "876edc64-d991-11e8-9f8b-f2801f1b9fd1";

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes); 
            ApoloHttpClient.DebugDelegate = Console.WriteLine;
        }

        public static string HandleError(Exception ex)
        {
            AggregateException agregateEx = ex as AggregateException;
            WebServiceException webEx = agregateEx != null ?
                agregateEx.InnerExceptions[0] as WebServiceException : ex as WebServiceException;
            Console.WriteLine("***** Error " + ex.StackTrace);
            return webEx != null ? webEx.Message : "Error inesperado";
        }

    }
}

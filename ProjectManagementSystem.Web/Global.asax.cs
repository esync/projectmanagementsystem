using System;
using System.Web; // Added for HttpContext, Server
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Serilog;
using Serilog.Events;

namespace ProjectManagementSystem.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.File(
                    Server.MapPath("~/App_Data/Logs/pms_log-.txt"), // Log file path, changed name slightly for uniqueness
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    shared: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(1))
                .CreateLogger();

            Log.Information("Application Starting Up...");
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            // Get the last exception
            var exception = Server.GetLastError();
            if (exception != null)
            {
                // Log the exception with Serilog
                Log.Error(exception, "An unhandled exception occurred in the application.");

                // Optional: Clear the error if you've handled it and don't want ASP.NET's default error page
                // Server.ClearError();

                // Optional: Redirect to a custom error page
                // Response.Redirect("~/Error/Index"); // Make sure you have an ErrorController and Index view
            }
        }

        protected void Application_End()
        {
            Log.Information("Application Shutting Down...");
            Log.CloseAndFlush();
        }
    }
}

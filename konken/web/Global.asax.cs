using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using AutoMapper;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Serilog;
using Serilog.Core;

namespace web
{
	public class MvcApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);

			var storage = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

			string loggerTableStorageContainer = "logger"
#if DEBUG
			+ "test"
#endif
			;

			Log.Logger = new LoggerConfiguration()
				.WriteTo.AzureTableStorage(storageAccount: storage, storageTableName: loggerTableStorageContainer)
				.CreateLogger();

			Log.Information("Logging.");

			var builder = new ContainerBuilder();

			// Register your MVC controllers. (MvcApplication is the name of
			// the class in Global.asax.)
			builder.RegisterControllers(typeof(MvcApplication).Assembly).PropertiesAutowired();

			////AutoMapper
			builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly()).AssignableTo(typeof(Profile)).As<Profile>();

			builder.Register(c => new MapperConfiguration(cfg =>
			{
				foreach (var profile in c.Resolve<IEnumerable<Profile>>())
				{
					cfg.AddProfile(profile);
				}
			})).AsSelf().SingleInstance();

			builder.Register(ctx => ctx.Resolve<MapperConfiguration>().CreateMapper()).As<IMapper>().PropertiesAutowired();
			//var autoMapperProfileTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()
			//	.Where(p => typeof(Profile).IsAssignableFrom(p) && p.IsPublic && !p.IsAbstract));
			//var autoMapperProfiles = autoMapperProfileTypes.Select(p => (Profile) Activator.CreateInstance(p));
			//builder.Register(ctx => new MapperConfiguration(cfg =>
			//{
			//	foreach (var profile in autoMapperProfiles)
			//	{
			//		cfg.AddProfile(profile);
			//	}
			//})).AsSelf().SingleInstance();
			//builder.Register(ctx => ctx.Resolve<MapperConfiguration>().CreateMapper()).As<IMapper>().InstancePerLifetimeScope()
			//	.PropertiesAutowired();

			// Set the dependency resolver to be Autofac.
			var container = builder.Build();
			DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

			Log.Information("Application started.");
		}
	}
}

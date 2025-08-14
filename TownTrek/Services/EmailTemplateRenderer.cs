using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TownTrek.Services.Interfaces;

namespace TownTrek.Services
{
	public class EmailTemplateRenderer : IEmailTemplateRenderer
	{
		private readonly IServiceProvider _serviceProvider;

		public EmailTemplateRenderer(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public async Task<string> RenderAsync(string viewPath, object model)
		{
			using var scope = _serviceProvider.CreateScope();
			var serviceProvider = scope.ServiceProvider;
            var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };
            var routeData = new Microsoft.AspNetCore.Routing.RouteData();
            var actionDescriptor = new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor();
            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor, new ModelStateDictionary());

			var viewEngine = serviceProvider.GetRequiredService<IRazorViewEngine>();
			var tempDataProvider = serviceProvider.GetRequiredService<ITempDataProvider>();
			var view = viewEngine.GetView(executingFilePath: null, viewPath: viewPath, isMainPage: false);
			if (!view.Success)
			{
				throw new InvalidOperationException($"Email view not found: {viewPath}");
			}

			await using var sw = new StringWriter();
			var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
			{
				Model = model
			};
			var viewContext = new ViewContext(
				actionContext,
				view.View,
				viewDictionary,
				new TempDataDictionary(actionContext.HttpContext, tempDataProvider),
				sw,
				new HtmlHelperOptions()
			);

			await view.View.RenderAsync(viewContext);
			return sw.ToString();
		}
	}
}



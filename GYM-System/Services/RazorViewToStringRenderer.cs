using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace GYM_System.Services
{
    /// <summary>
    /// Interface for rendering Razor views to HTML strings.
    /// Used by DinkToPdfService to convert Razor views to HTML before PDF generation.
    /// </summary>
    public interface IRazorViewToStringRenderer
    {
        /// <summary>
        /// Renders a Razor view to an HTML string.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="viewName">The name or path of the view (e.g., "~/Views/Pdf/DietPlan.cshtml").</param>
        /// <param name="model">The model to pass to the view.</param>
        /// <returns>The rendered HTML as a string.</returns>
        Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model);
    }

    /// <summary>
    /// Implementation of IRazorViewToStringRenderer.
    /// Renders Razor views to HTML strings for use in PDF generation.
    /// </summary>
    public class RazorViewToStringRenderer : IRazorViewToStringRenderer
    {
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;

        public RazorViewToStringRenderer(
            IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }

        public async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model)
        {
            var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            using var stringWriter = new StringWriter();

            var viewResult = _viewEngine.FindView(actionContext, viewName, false);

            if (viewResult.View == null)
            {
                // Try to find view by absolute path
                viewResult = _viewEngine.GetView(executingFilePath: null, viewPath: viewName, isMainPage: false);
            }

            if (viewResult.View == null)
            {
                throw new InvalidOperationException($"Unable to find view '{viewName}'. Searched locations: {string.Join(", ", viewResult.SearchedLocations ?? Array.Empty<string>())}");
            }

            var viewDictionary = new ViewDataDictionary<TModel>(
                new EmptyModelMetadataProvider(),
                new ModelStateDictionary())
            {
                Model = model
            };

            var viewContext = new ViewContext(
                actionContext,
                viewResult.View,
                viewDictionary,
                new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
                stringWriter,
                new HtmlHelperOptions());

            await viewResult.View.RenderAsync(viewContext);

            return stringWriter.ToString();
        }
    }
}

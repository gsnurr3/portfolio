using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace RESTfulAPI.Presentation.Conventions
{
    /// <summary>
    /// Applies a common route prefix to all attribute-routed controllers.
    /// This convention prepends the given prefix (e.g., "api") to every
    /// controller route template, allowing a centralized configuration
    /// of base routes across the application.
    /// </summary>
    public class RoutePrefixConvention : IApplicationModelConvention
    {
        private readonly AttributeRouteModel _prefix;
        public RoutePrefixConvention(string prefix)
            => _prefix = new AttributeRouteModel(new RouteAttribute(prefix));

        public void Apply(ApplicationModel app)
        {
            foreach (var controller in app.Controllers)
            {
                foreach (var selector in controller.Selectors
                                                   .Where(s => s.AttributeRouteModel != null))
                {
                    selector.AttributeRouteModel =
                        AttributeRouteModel.CombineAttributeRouteModel(_prefix,
                                                                       selector.AttributeRouteModel);
                }
            }
        }
    }
}

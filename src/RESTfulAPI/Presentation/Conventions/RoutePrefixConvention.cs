using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

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
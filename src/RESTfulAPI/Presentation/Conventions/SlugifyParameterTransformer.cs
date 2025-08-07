namespace RESTfulAPI.Presentation.Conventions
{
    /// <summary>
    /// Transforms outbound route parameter values into a URL-friendly “slug” by converting them to lowercase.
    /// </summary>
    public class SlugifyParameterTransformer : IOutboundParameterTransformer
    {
        public string? TransformOutbound(object? value)
            => value?.ToString()?.ToLowerInvariant();
    }
}

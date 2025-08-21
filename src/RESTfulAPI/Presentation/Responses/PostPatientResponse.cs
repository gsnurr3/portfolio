using RESTfulAPI.Application.DTOs;

namespace RESTfulAPI.Presentation.Responses
{
    public sealed class PostPatientResponse
    {
        public PostPatientDTO Patient { get; init; } = default!;
    }
}
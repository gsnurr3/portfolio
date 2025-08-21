using RESTfulAPI.Application.DTOs;

namespace RESTfulAPI.Presentation.Responses
{
    public class GetPatientsResponse
    {
        public ICollection<GetPatientsDTO> Patients { get; set; } = [];
    }
}

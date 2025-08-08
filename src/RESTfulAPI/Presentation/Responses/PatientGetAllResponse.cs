using RESTfulAPI.Domain.Entities;

namespace RESTfulAPI.Presentation.Responses
{
    public class PatientGetAllResponse
    {
        public ICollection<Patient> Patients { get; set; } = [];
    }
}

using FluentValidation;
using RESTfulAPI.Application.Requests;

namespace RESTfulAPI.Application.Validators
{
    public class PatientGetAllRequestValidator : AbstractValidator<PatientGetAllRequest>
    {
        public PatientGetAllRequestValidator()
        {

        }
    }
}

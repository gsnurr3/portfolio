using RESTfulAPI.Domain.Entities;

namespace RESTfulAPI.Infrastructure.Repositories.Interfaces
{
    public interface IPatientRepository
    {
        Task<Patient> CreateAsync(Patient patient, CancellationToken cancellationToken);
        Task<IReadOnlyList<Patient>> GetAllAsync(CancellationToken cancellationToken);
    }
}
using RESTfulAPI.Domain.Entities;

namespace RESTfulAPI.Infrastructure.Repositories.Interfaces
{
    public interface IPatientRepository
    {
        Task<IReadOnlyList<Patient>> GetAllAsync(CancellationToken cancellationToken);
    }
}
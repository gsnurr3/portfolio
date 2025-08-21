using Microsoft.EntityFrameworkCore;
using RESTfulAPI.Domain.Entities;
using RESTfulAPI.Infrastructure.Repositories.Interfaces;
using RESTfulAPI.Persistence;

namespace RESTfulAPI.Infrastructure.Repositories
{
    public class PatientRepository(AppDbContext _appDbContext) : IPatientRepository
    {
        public async Task<Patient> CreateAsync(Patient patient, CancellationToken cancellationToken)
        {
            _appDbContext.Patients.Add(patient);

            await _appDbContext.SaveChangesAsync(cancellationToken);

            return patient;
        }

        public async Task<IReadOnlyList<Patient>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _appDbContext.Patients
                            .AsNoTracking()
                            .ToListAsync(cancellationToken);
        }
    }
}
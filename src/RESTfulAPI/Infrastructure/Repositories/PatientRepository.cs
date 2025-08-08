using Microsoft.EntityFrameworkCore;
using RESTfulAPI.Domain.Entities;
using RESTfulAPI.Infrastructure.Repositories.Interfaces;
using RESTfulAPI.Persistence;

namespace RESTfulAPI.Infrastructure.Repositories
{
    public class PatientRepository(AppDbContext _appDbContext) : IPatientRepository
    {
        public async Task<ICollection<Patient>> GetAllPatientsAsync()
        {
            return await _appDbContext.Patients.ToListAsync();
        }
    }
}
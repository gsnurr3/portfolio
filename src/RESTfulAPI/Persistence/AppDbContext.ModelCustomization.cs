using Microsoft.EntityFrameworkCore;
using RESTfulAPI.Domain.Entities;
using RESTfulAPI.Domain.Enums;
using RESTfulAPI.Persistence.Converters;

namespace RESTfulAPI.Persistence
{
    public partial class AppDbContext
    {
        // This method is called from the scaffolded OnModelCreating(...)
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            // Enum <-> CHAR(1) mapping for Patient.Gender
            modelBuilder.Entity<Patient>(e =>
            {
                e.Property(p => p.Gender)
                 .HasConversion(new GenderCodeConverter())
                 .HasMaxLength(1)
                 .IsUnicode(false)
                 .HasColumnType("char(1)")
                 .HasColumnName("Gender");
            });

            // Enum <-> tinyint mapping for Payment.Status
            modelBuilder.Entity<Payment>(e =>
            {
                e.Property(p => p.Status)
                    .HasConversion(
                        v => (byte)v,
                        v => (PaymentStatus)v)
                    .HasColumnType("tinyint");
            });
        }
    }
}
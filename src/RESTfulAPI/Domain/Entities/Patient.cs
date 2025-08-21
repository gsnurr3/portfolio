using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using RESTfulAPI.Domain.Enums;

namespace RESTfulAPI.Domain.Entities;

[Index("MedicalRecordNumber", Name = "UQ__Patients__8E549ED05C9BE46D", IsUnique = true)]
public partial class Patient
{
    [Key]
    public Guid PatientId { get; set; }

    [StringLength(20)]
    public string MedicalRecordNumber { get; set; } = null!;

    [StringLength(50)]
    public string FirstName { get; set; } = null!;

    [StringLength(50)]
    public string LastName { get; set; } = null!;

    public DateOnly DateOfBirth { get; set; }

    public Gender Gender { get; set; }

    [StringLength(200)]
    public string? Address { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? PhoneNumber { get; set; }

    [StringLength(100)]
    public string? Email { get; set; }

    public int? InsuranceProviderId { get; set; }

    [StringLength(50)]
    public string? InsurancePolicyNumber { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    [ForeignKey("InsuranceProviderId")]
    [InverseProperty("Patients")]
    public virtual InsuranceProvider? InsuranceProvider { get; set; }

    [InverseProperty("Patient")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

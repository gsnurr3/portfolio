using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RESTfulAPI.Domain.Entities;

[Index("Name", Name = "UQ__Insuranc__737584F60072F5B3", IsUnique = true)]
public partial class InsuranceProvider
{
    [Key]
    public int InsuranceProviderId { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string? PhoneNumber { get; set; }

    [StringLength(200)]
    public string? Address { get; set; }

    public DateTime CreatedAt { get; set; }

    [InverseProperty("InsuranceProvider")]
    public virtual ICollection<Patient> Patients { get; set; } = new List<Patient>();
}

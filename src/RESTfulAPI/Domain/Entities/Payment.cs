using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RESTfulAPI.Domain.Entities;

[Index("DepartmentId", Name = "IX_Payments_DepartmentId")]
[Index("PatientId", Name = "IX_Payments_PatientId")]
public partial class Payment
{
    [Key]
    public int PaymentId { get; set; }

    public Guid PatientId { get; set; }

    public int DepartmentId { get; set; }

    public DateTime PaymentDate { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    public byte PaymentMethodId { get; set; }

    [StringLength(50)]
    public string? InsuranceClaimNumber { get; set; }

    public byte Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    [ForeignKey("DepartmentId")]
    [InverseProperty("Payments")]
    public virtual Department Department { get; set; } = null!;

    [ForeignKey("PatientId")]
    [InverseProperty("Payments")]
    public virtual Patient Patient { get; set; } = null!;

    [ForeignKey("PaymentMethodId")]
    [InverseProperty("Payments")]
    public virtual PaymentMethod PaymentMethod { get; set; } = null!;
}

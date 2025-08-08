using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RESTfulAPI.Domain.Entities;

[Index("MethodName", Name = "UQ__PaymentM__218CFB1752AA4271", IsUnique = true)]
public partial class PaymentMethod
{
    [Key]
    public byte PaymentMethodId { get; set; }

    [StringLength(50)]
    public string MethodName { get; set; } = null!;

    [InverseProperty("PaymentMethod")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

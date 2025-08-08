using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace RESTfulAPI.Domain.Entities;

[Index("CorrelationId", Name = "IX_RequestLogs_CorrelationId")]
[Index("Path", Name = "IX_RequestLogs_Path")]
[Index("RequestDate", Name = "IX_RequestLogs_RequestDate")]
[Index("RequestTime", Name = "IX_RequestLogs_RequestTime", AllDescending = true)]
[Index("StatusCode", "RequestTime", Name = "IX_RequestLogs_StatusCode", IsDescending = new[] { false, true })]
[Index("UserId", Name = "IX_RequestLogs_UserId")]
[Index("RequestId", Name = "UQ_RequestLogs_RequestId", IsUnique = true)]
public partial class RequestLog
{
    [Key]
    public long LogId { get; set; }

    public Guid RequestId { get; set; }

    public Guid? CorrelationId { get; set; }

    [StringLength(64)]
    public string? UserId { get; set; }

    public DateTime RequestTime { get; set; }

    [StringLength(7)]
    [Unicode(false)]
    public string Method { get; set; } = null!;

    [StringLength(5)]
    [Unicode(false)]
    public string Scheme { get; set; } = null!;

    [StringLength(255)]
    public string Host { get; set; } = null!;

    [StringLength(2048)]
    public string Path { get; set; } = null!;

    [StringLength(4000)]
    public string? QueryString { get; set; }

    public short StatusCode { get; set; }

    public int DurationMs { get; set; }

    [StringLength(45)]
    [Unicode(false)]
    public string? RemoteIp { get; set; }

    [StringLength(512)]
    public string? UserAgent { get; set; }

    [StringLength(512)]
    public string? Referrer { get; set; }

    [StringLength(100)]
    public string? RequestContentType { get; set; }

    [StringLength(100)]
    public string? ResponseContentType { get; set; }

    public int? BytesReceived { get; set; }

    public int? BytesSent { get; set; }

    public string? RequestHeaders { get; set; }

    public string? ResponseHeaders { get; set; }

    public string? RequestBody { get; set; }

    public string? ResponseBody { get; set; }

    [StringLength(200)]
    public string? ExceptionType { get; set; }

    [StringLength(2000)]
    public string? ExceptionMessage { get; set; }

    public string? ExceptionStackTrace { get; set; }

    [StringLength(128)]
    public string ServerName { get; set; } = null!;

    [StringLength(32)]
    public string? Environment { get; set; }

    public DateOnly? RequestDate { get; set; }
}

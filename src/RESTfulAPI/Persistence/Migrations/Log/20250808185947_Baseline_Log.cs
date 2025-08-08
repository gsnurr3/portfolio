using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RESTfulAPI.Persistence.Migrations.Log
{
    /// <inheritdoc />
    public partial class Baseline_Log : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RequestLogs",
                columns: table => new
                {
                    LogId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newsequentialid())"),
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    RequestTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    Method = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: false),
                    Scheme = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: false),
                    Host = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Path = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    QueryString = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    StatusCode = table.Column<short>(type: "smallint", nullable: false),
                    DurationMs = table.Column<int>(type: "int", nullable: false),
                    RemoteIp = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Referrer = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    RequestContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ResponseContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BytesReceived = table.Column<int>(type: "int", nullable: true),
                    BytesSent = table.Column<int>(type: "int", nullable: true),
                    RequestHeaders = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseHeaders = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExceptionType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ExceptionMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ExceptionStackTrace = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServerName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, defaultValueSql: "(host_name())"),
                    Environment = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    RequestDate = table.Column<DateOnly>(type: "date", nullable: true, computedColumnSql: "(CONVERT([date],[RequestTime]))", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestLogs", x => x.LogId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequestLogs_CorrelationId",
                table: "RequestLogs",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestLogs_Errors",
                table: "RequestLogs",
                column: "RequestTime",
                descending: new bool[0],
                filter: "([StatusCode]>=(500))");

            migrationBuilder.CreateIndex(
                name: "IX_RequestLogs_Path",
                table: "RequestLogs",
                column: "Path");

            migrationBuilder.CreateIndex(
                name: "IX_RequestLogs_RequestDate",
                table: "RequestLogs",
                column: "RequestDate");

            migrationBuilder.CreateIndex(
                name: "IX_RequestLogs_RequestTime",
                table: "RequestLogs",
                column: "RequestTime",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_RequestLogs_StatusCode",
                table: "RequestLogs",
                columns: new[] { "StatusCode", "RequestTime" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_RequestLogs_UserId",
                table: "RequestLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UQ_RequestLogs_RequestId",
                table: "RequestLogs",
                column: "RequestId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequestLogs");
        }
    }
}

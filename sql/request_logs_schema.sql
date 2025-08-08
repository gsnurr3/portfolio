CREATE TABLE dbo.RequestLogs
(
    LogId                  BIGINT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_RequestLogs PRIMARY KEY CLUSTERED,

    -- Stable, external-safe id for correlation across systems
    RequestId              UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT DF_RequestLogs_RequestId DEFAULT NEWSEQUENTIALID(),
    CONSTRAINT UQ_RequestLogs_RequestId UNIQUE (RequestId),

    -- Correlation/trace & identity
    CorrelationId          UNIQUEIDENTIFIER NULL,
    UserId                 NVARCHAR(64) NULL,          -- e.g., AAD object id or app user key

    -- Request basics
    RequestTime            DATETIME2 NOT NULL
        CONSTRAINT DF_RequestLogs_RequestTime DEFAULT SYSUTCDATETIME(),
    Method                 VARCHAR(7)  NOT NULL,
    Scheme                 VARCHAR(5)  NOT NULL,       -- http / https
    Host                   NVARCHAR(255) NOT NULL,
    Path                   NVARCHAR(2048) NOT NULL,
    QueryString            NVARCHAR(4000) NULL,

    -- Response summary
    StatusCode             SMALLINT NOT NULL,
    DurationMs             INT      NOT NULL,          -- end - start in ms

    -- Client & headers (avoid PII in production)
    RemoteIp               VARCHAR(45) NULL,           -- IPv4/IPv6
    UserAgent              NVARCHAR(512) NULL,
    Referrer               NVARCHAR(512) NULL,

    -- Content metadata & sizes
    RequestContentType     NVARCHAR(100) NULL,
    ResponseContentType    NVARCHAR(100) NULL,
    BytesReceived          INT NULL,
    BytesSent              INT NULL,

    -- Optional payload/header snapshots (redact in prod)
    RequestHeaders         NVARCHAR(MAX) NULL,
    ResponseHeaders        NVARCHAR(MAX) NULL,
    RequestBody            NVARCHAR(MAX) NULL,
    ResponseBody           NVARCHAR(MAX) NULL,

    -- Error info
    ExceptionType          NVARCHAR(200)  NULL,
    ExceptionMessage       NVARCHAR(2000) NULL,
    ExceptionStackTrace    NVARCHAR(MAX)  NULL,

    -- Operational
    ServerName             NVARCHAR(128) NOT NULL
        CONSTRAINT DF_RequestLogs_ServerName DEFAULT HOST_NAME(),
    Environment            NVARCHAR(32) NULL,

    -- Handy for partitioning/rollups
    RequestDate AS CAST(RequestTime AS DATE) PERSISTED,

    -- Guard rails
    CONSTRAINT CK_RequestLogs_Method
        CHECK (Method IN ('GET','POST','PUT','PATCH','DELETE','HEAD','OPTIONS')),
    CONSTRAINT CK_RequestLogs_Scheme
        CHECK (Scheme IN ('http','https')),
    CONSTRAINT CK_RequestLogs_StatusCode
        CHECK (StatusCode BETWEEN 100 AND 599),
    CONSTRAINT CK_RequestLogs_DurationMs
        CHECK (DurationMs >= 0),
    CONSTRAINT CK_RequestLogs_BytesReceived
        CHECK (BytesReceived IS NULL OR BytesReceived >= 0),
    CONSTRAINT CK_RequestLogs_BytesSent
        CHECK (BytesSent IS NULL OR BytesSent >= 0)
);
GO

CREATE INDEX IX_RequestLogs_RequestTime
    ON dbo.RequestLogs (RequestTime DESC);

CREATE INDEX IX_RequestLogs_StatusCode
    ON dbo.RequestLogs (StatusCode, RequestTime DESC)
    INCLUDE (Method, Path, CorrelationId);

CREATE INDEX IX_RequestLogs_CorrelationId
    ON dbo.RequestLogs (CorrelationId);

CREATE INDEX IX_RequestLogs_UserId
    ON dbo.RequestLogs (UserId);

CREATE INDEX IX_RequestLogs_Path
    ON dbo.RequestLogs (Path)
    INCLUDE (Method, StatusCode, DurationMs);

-- Quick “errors only” investigations
CREATE INDEX IX_RequestLogs_Errors
    ON dbo.RequestLogs (RequestTime DESC)
    INCLUDE (StatusCode, Method, Path, CorrelationId)
    WHERE StatusCode >= 500;

-- Daily rollups / retention helpers
CREATE INDEX IX_RequestLogs_RequestDate
    ON dbo.RequestLogs (RequestDate);
GO
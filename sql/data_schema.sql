CREATE TABLE Departments (
    DepartmentId    INT             IDENTITY(1,1)   PRIMARY KEY,
    Name            NVARCHAR(100)   NOT NULL        UNIQUE,
    Location        NVARCHAR(100)   NOT NULL,
    PhoneNumber     VARCHAR(20)     NULL,
    HeadOfDept      NVARCHAR(100)   NULL,           -- e.g. “Dr. Alice Smith”
    CreatedAt       DATETIME2       NOT NULL        DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2       NOT NULL        DEFAULT SYSUTCDATETIME()
);

CREATE TABLE InsuranceProviders (
    InsuranceProviderId   INT             IDENTITY(1,1) PRIMARY KEY,
    Name                  NVARCHAR(100)   NOT NULL      UNIQUE,
    PhoneNumber           VARCHAR(20)     NULL,
    Address               NVARCHAR(200)   NULL,
    CreatedAt             DATETIME2       NOT NULL      DEFAULT SYSUTCDATETIME()
);

CREATE TABLE PaymentMethods (
    PaymentMethodId  TINYINT   PRIMARY KEY,
    MethodName       NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE Patients (
    PatientId               UNIQUEIDENTIFIER   NOT NULL    PRIMARY KEY
        DEFAULT NEWSEQUENTIALID(),
    MedicalRecordNumber     NVARCHAR(20)       NOT NULL    UNIQUE,
    FirstName               NVARCHAR(50)       NOT NULL,
    LastName                NVARCHAR(50)       NOT NULL,
    DateOfBirth             DATE               NOT NULL,
    Gender                  CHAR(1)            NOT NULL    CHECK (Gender IN ('M','F','O')),
    Address                 NVARCHAR(200)      NULL,
    PhoneNumber             VARCHAR(20)        NULL,
    Email                   NVARCHAR(100)      NULL,
    InsuranceProviderId     INT                NULL,
    InsurancePolicyNumber   NVARCHAR(50)       NULL,
    CreatedAt               DATETIME2          NOT NULL    DEFAULT SYSUTCDATETIME(),
    UpdatedAt               DATETIME2          NOT NULL    DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Patients_InsuranceProviders
        FOREIGN KEY (InsuranceProviderId)
        REFERENCES InsuranceProviders(InsuranceProviderId)
);

CREATE TABLE Payments (
    PaymentId           INT             IDENTITY(1,1)   PRIMARY KEY,
    PatientId           UNIQUEIDENTIFIER NOT NULL,
    DepartmentId        INT             NOT NULL,
    PaymentDate         DATETIME2       NOT NULL        DEFAULT SYSUTCDATETIME(),
    Amount              DECIMAL(18,2)   NOT NULL        CHECK (Amount > 0),
    PaymentMethodId     TINYINT         NOT NULL,
    InsuranceClaimNumber    NVARCHAR(50)    NULL,
    Status              TINYINT         NOT NULL        DEFAULT 1,  -- e.g. 1=Pending,2=Completed,3=Denied,4=Refunded
    CreatedAt           DATETIME2       NOT NULL        DEFAULT SYSUTCDATETIME(),
    UpdatedAt           DATETIME2       NOT NULL        DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Payments_Patients
        FOREIGN KEY (PatientId) REFERENCES Patients(PatientId),
    CONSTRAINT FK_Payments_Departments
        FOREIGN KEY (DepartmentId) REFERENCES Departments(DepartmentId),
    CONSTRAINT FK_Payments_PaymentMethods
        FOREIGN KEY (PaymentMethodId) REFERENCES PaymentMethods(PaymentMethodId)
);
CREATE INDEX IX_Payments_PatientId ON Payments(PatientId);
CREATE INDEX IX_Payments_DepartmentId ON Payments(DepartmentId);
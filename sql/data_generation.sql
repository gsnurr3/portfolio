-- 1) Departments (10 sample depts)
INSERT INTO Departments (Name, Location, PhoneNumber, HeadOfDept)
VALUES
  ('Cardiology',    'Bldg A – 2nd Fl.', '(555)100-0001', 'Dr. Alice Heart'),
  ('Radiology',     'Bldg B – 1st Fl.', '(555)100-0002', 'Dr. Bob X-Ray'),
  ('Oncology',      'Bldg A – 3rd Fl.', '(555)100-0003', 'Dr. Carol Cure'),
  ('Neurology',     'Bldg C – 2nd Fl.', '(555)100-0004', 'Dr. Dan Brain'),
  ('Pediatrics',    'Bldg D – 1st Fl.', '(555)100-0005', 'Dr. Eve Child'),
  ('Psychiatry',    'Bldg C – 3rd Fl.', '(555)100-0006', 'Dr. Frank Mind'),
  ('Orthopedics',   'Bldg B – 2nd Fl.', '(555)100-0007', 'Dr. Grace Bones'),
  ('Emergency',     'ER Wing',          '(555)100-0008', 'Dr. Henry Quick'),
  ('Laboratory',    'Bldg E – LL',      '(555)100-0009', 'Dr. Ivy Lab'),
  ('Pharmacy',      'Bldg E – 1st Fl.', '(555)100-0010', 'Dr. Jack Dose');

-- 2) InsuranceProviders (15)
INSERT INTO InsuranceProviders (Name, PhoneNumber, Address)
VALUES
  ('Blue Cross Blue Shield',    '(555)200-0001', '123 Health St, Anytown, USA'),
  ('UnitedHealthcare',          '(555)200-0002', '456 Care Ave, MedCity, USA'),
  ('Aetna',                     '(555)200-0003', '789 Wellness Rd, Welltown, USA'),
  ('Cigna',                     '(555)200-0004', '101 Coverage Blvd, Insureville, USA'),
  ('Kaiser Permanente',         '(555)200-0005', '202 Stream Ln, Healthyburg, USA'),
  ('Humana',                    '(555)200-0006', '303 Vitality Dr, FitCity, USA'),
  ('Health Net',                '(555)200-0007', '404 Wellness Pkwy, Healthport, USA'),
  ('Centene',                   '(555)200-0008', '505 Benefit Ct, Policyton, USA'),
  ('Highmark',                  '(555)200-0009', '606 Shield St, Protectown, USA'),
  ('Molina Healthcare',         '(555)200-0010', '707 Care Cir, Insurham, USA'),
  ('Guardian Life',             '(555)200-0011', '808 Wellness Way, Safecity, USA'),
  ('Amerigroup',                '(555)200-0012', '909 Benefit Blvd, Covertown, USA'),
  ('WellCare',                  '(555)200-0013', '1001 Health Ave, Aidville, USA'),
  ('Magellan Health',           '(555)200-0014', '1102 Mind St, Psychburg, USA'),
  ('Oscar Health',              '(555)200-0015', '1203 Policy Rd, Careham, USA');

-- 3) PaymentMethods (5) — if not already seeded
INSERT INTO PaymentMethods (PaymentMethodId, MethodName)
VALUES
  (1,'Cash'),
  (2,'Credit Card'),
  (3,'Insurance'),
  (4,'Check'),
  (5,'Other');

-- 4) Generate Patients (100 randomized)
DECLARE 
  @i     INT,
  @mrn   NVARCHAR(20),
  @first NVARCHAR(50),
  @last  NVARCHAR(50),
  @dob   DATE,
  @gender CHAR(1),
  @addr  NVARCHAR(200),
  @phone VARCHAR(20),
  @email NVARCHAR(100),
  @insId INT,
  @pol   NVARCHAR(50);

SET @i = 1;
WHILE @i <= 100
BEGIN
  -- MedicalRecordNumber
  SET @mrn = 'MRN' + RIGHT('000000' + CAST(@i AS NVARCHAR(6)), 6);

  -- FirstName
  SELECT TOP 1 @first = Name
    FROM (VALUES
      ('John'),('Jane'),('Michael'),('Emily'),('David'),
      ('Sarah'),('Robert'),('Jessica'),('William'),('Ashley'),
      ('James'),('Amanda'),('Richard'),('Melissa'),('Joseph')
    ) AS fn(Name)
  ORDER BY NEWID();

  -- LastName
  SELECT TOP 1 @last = Name
    FROM (VALUES
      ('Smith'),('Johnson'),('Williams'),('Brown'),('Jones'),
      ('Garcia'),('Miller'),('Davis'),('Rodriguez'),('Martinez'),
      ('Hernandez'),('Lopez'),('Gonzalez'),('Wilson'),('Anderson')
    ) AS ln(Name)
  ORDER BY NEWID();

  -- DateOfBirth (up to ~100 years ago)
  SET @dob = DATEADD(DAY, - (ABS(CHECKSUM(NEWID())) % 36500), CAST(GETDATE() AS DATE));

  -- Gender distribution ~47% M, ~47% F, ~6% O
  SET @gender = CASE
                  WHEN ABS(CHECKSUM(NEWID())) % 100 < 47 THEN 'M'
                  WHEN ABS(CHECKSUM(NEWID())) % 100 < 94 THEN 'F'
                  ELSE 'O'
                END;

  -- Address
  SET @addr = 
       CAST(@i AS NVARCHAR(10))
     + ' Main St, City' + CAST(@i AS NVARCHAR(10))
     + ', ST ' + RIGHT('00' + CAST(1 + ABS(CHECKSUM(NEWID())) % 99 AS NVARCHAR(2)), 2);

  -- PhoneNumber
  SET @phone = 
       '(555)' 
     + RIGHT('0000000' + CAST(ABS(CHECKSUM(NEWID())) % 9000000 + 1000000 AS VARCHAR(7)), 7);

  -- Email
  SET @email = LOWER(@first + '.' + @last + CAST(@i AS NVARCHAR(10)) + '@example.com');

  -- Random InsuranceProvider
  SELECT TOP 1 @insId = InsuranceProviderId
    FROM InsuranceProviders
  ORDER BY NEWID();

  -- Policy Number
  SET @pol = 'POL' 
    + RIGHT('000000' + CAST(ABS(CHECKSUM(NEWID())) % 1000000 AS NVARCHAR(6)), 6);

  INSERT INTO Patients
    (MedicalRecordNumber, FirstName, LastName, DateOfBirth, Gender,
     Address, PhoneNumber, Email, InsuranceProviderId, InsurancePolicyNumber)
  VALUES
    (@mrn, @first, @last, @dob, @gender,
     @addr, @phone, @email, @insId, @pol);

  SET @i = @i + 1;
END;


-- 5) Generate Payments (100 randomized)
DECLARE
  @j     INT,
  @pid   UNIQUEIDENTIFIER,
  @did   INT,
  @pmid  TINYINT,
  @amt   DECIMAL(18,2),
  @stat  TINYINT,
  @claim NVARCHAR(50),
  @pdate DATETIME2;

SET @j = 1;
WHILE @j <= 100
BEGIN
  -- Random FK picks
  SELECT TOP 1 @pid  = PatientId      FROM Patients      ORDER BY NEWID();
  SELECT TOP 1 @did  = DepartmentId   FROM Departments   ORDER BY NEWID();
  SELECT TOP 1 @pmid = PaymentMethodId FROM PaymentMethods ORDER BY NEWID();

  -- Amount between $1.00 and $5,000.99
  SET @amt = CAST(
       (ABS(CHECKSUM(NEWID())) % 5000 + 1)
     + (ABS(CHECKSUM(NEWID())) % 100) / 100.0
    AS DECIMAL(18,2));

  -- Status 1–4
  SET @stat = CAST((ABS(CHECKSUM(NEWID())) % 4) + 1 AS TINYINT);

  -- Only insurance payments get a claim #
  IF @pmid = 3
    SET @claim = 'CLM' 
      + RIGHT('000000' + CAST(ABS(CHECKSUM(NEWID())) % 1000000 AS NVARCHAR(6)), 6);
  ELSE
    SET @claim = NULL;

  -- Within the last year
  SET @pdate = DATEADD(DAY, - (ABS(CHECKSUM(NEWID())) % 365), GETDATE());

  INSERT INTO Payments
    (PatientId, DepartmentId, PaymentDate, Amount,
     PaymentMethodId, InsuranceClaimNumber, Status)
  VALUES
    (@pid, @did, @pdate, @amt,
     @pmid, @claim, @stat);

  SET @j = @j + 1;
END;
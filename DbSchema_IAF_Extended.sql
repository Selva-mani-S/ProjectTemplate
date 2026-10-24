-- ==============================================================================
-- Indian Air Force Batch 1986 Reunion Management & Member Registry Database Schema
-- Baseline reference: DbTable.txt extended with approved enhancements
-- ==============================================================================

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'IAF')
BEGIN
    CREATE DATABASE IAF;
END
GO

USE IAF;
GO

-- 1. Trade Master Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Trade')
BEGIN
    CREATE TABLE Trade
    (
        TradeID VARCHAR(2) PRIMARY KEY,
        TradeName VARCHAR(100) NOT NULL
    );
END
GO

-- 2. State Master Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'State')
BEGIN
    CREATE TABLE State
    (
        StateID VARCHAR(2) PRIMARY KEY,
        StateName VARCHAR(100) NOT NULL
    );
END
GO

-- 3. IAF Personal Data Table (Member Registry)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IAF_PersonalData')
BEGIN
    CREATE TABLE IAF_PersonalData
    (
        ServiceNo VARCHAR(6) PRIMARY KEY,
        Name VARCHAR(100) NOT NULL,
        DOB DATE NULL,
        DOD DATE NULL,
        TradeID VARCHAR(2) NULL,
        Address1 VARCHAR(50) NULL,
        Address2 VARCHAR(50) NULL,
        StateID VARCHAR(2) NULL,
        MobileNo VARCHAR(13) NULL,
        TechnicalSkills VARCHAR(250) NULL,
        PlacesWorked VARCHAR(250) NULL,
        AdditionalDetails VARCHAR(250) NULL,
        Achievements VARCHAR(200) NULL,
        VRSReason VARCHAR(200) NULL,
        CONSTRAINT FK_PersonalData_Trade
            FOREIGN KEY (TradeID) REFERENCES Trade(TradeID),
        CONSTRAINT FK_PersonalData_State
            FOREIGN KEY (StateID) REFERENCES State(StateID)
    );
END
GO

-- 4. IAF Family Data Table
-- Added FamilyMemberId primary key for reliable individual record management
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IAF_FamilyData')
BEGIN
    CREATE TABLE IAF_FamilyData
    (
        FamilyMemberId INT IDENTITY(1,1) PRIMARY KEY,
        ServiceNo VARCHAR(6) NOT NULL,
        MemberName VARCHAR(100) NOT NULL,
        Relationship VARCHAR(30) NULL,
        Gender CHAR(1) NULL,
        Occupation VARCHAR(100) NULL,
        Remarks VARCHAR(200) NULL,
        CONSTRAINT FK_FamilyData_PersonalData
            FOREIGN KEY (ServiceNo) REFERENCES IAF_PersonalData(ServiceNo)
            ON DELETE CASCADE
    );
END
GO

-- 5. IAF Additional Data Table (Photographs)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IAF_AdditionalData')
BEGIN
    CREATE TABLE IAF_AdditionalData
    (
        ServiceNo VARCHAR(6) PRIMARY KEY,
        Photo1 VARBINARY(MAX) NULL,
        Photo2 VARBINARY(MAX) NULL,
        Photo3 VARBINARY(MAX) NULL,
        Photo4 VARBINARY(MAX) NULL,
        Photo5 VARBINARY(MAX) NULL,
        CONSTRAINT FK_AdditionalData_PersonalData
            FOREIGN KEY (ServiceNo) REFERENCES IAF_PersonalData(ServiceNo)
            ON DELETE CASCADE
    );
END
GO

-- 6. IAF Reunion Event Configuration Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IAF_ReunionEvent')
BEGIN
    CREATE TABLE IAF_ReunionEvent
    (
        EventId INT IDENTITY(1,1) PRIMARY KEY,
        Title VARCHAR(200) NOT NULL,
        Batch VARCHAR(50) NOT NULL,
        EventDate DATE NOT NULL,
        EventEndDate DATE NULL,
        Venue VARCHAR(250) NOT NULL,
        RegistrationDeadline DATE NOT NULL,
        BaseFeeAmount DECIMAL(18,2) NOT NULL DEFAULT 5000.00,
        IsActive BIT NOT NULL DEFAULT 1,
        Description VARCHAR(2000) NULL,
        CreatedAtUtc DATETIME NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

-- 7. IAF Reunion Registration Table
-- Syntax issue in DbTable.txt corrected; added EventId & RegistrationStatus
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IAF_Reunion')
BEGIN
    CREATE TABLE IAF_Reunion
    (
        ReunionID INT IDENTITY(1,1) PRIMARY KEY,
        EventID INT NOT NULL,
        ServiceNo VARCHAR(6) NOT NULL,
        NoOfPersons INT NOT NULL DEFAULT 1,
        AccompDetails VARCHAR(100) NULL, 
        DateOfArrival DATETIME NULL,
        PlaceOfArrival VARCHAR(20) NULL, -- 'Airport', 'Egmore', 'Central', 'Other'
        Accommodation VARCHAR(100) NULL,    
        DateOfDeparture DATETIME NULL,
        PlaceOfDeparture VARCHAR(50) NULL,
        TShirt VARCHAR(10) NULL,         -- 'M', 'L', 'XL', 'XXL'
        Drinker INT NOT NULL DEFAULT 0,  -- 0 = Non-Drinker, 1 = Drinker
        VegNonVeg INT NOT NULL DEFAULT 0,-- 0 = Vegetarian, 1 = Non-Vegetarian
        RegistrationStatus VARCHAR(30) NOT NULL DEFAULT 'Pending',
        CreatedAtUtc DATETIME NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAtUtc DATETIME NULL,
        CONSTRAINT FK_Reunion_Event
            FOREIGN KEY (EventID) REFERENCES IAF_ReunionEvent(EventId),
        CONSTRAINT FK_Reunion_PersonalData
            FOREIGN KEY (ServiceNo) REFERENCES IAF_PersonalData(ServiceNo)
            ON DELETE CASCADE,
        CONSTRAINT UQ_Reunion_Event_ServiceNo
            UNIQUE (EventID, ServiceNo)
    );
END
GO

-- 8. IAF Payment Table (Dedicated Payment and Verification Tracking)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IAF_Payment')
BEGIN
    CREATE TABLE IAF_Payment
    (
        PaymentID INT IDENTITY(1,1) PRIMARY KEY,
        EventID INT NOT NULL,
        ServiceNo VARCHAR(6) NOT NULL,
        AmountPaid DECIMAL(18,2) NOT NULL,
        PaymentMode VARCHAR(30) NOT NULL, -- 'UPI', 'BankTransfer', 'Cash', 'Cheque', 'Other'
        AcknowledgementNumber VARCHAR(100) NULL,
        PaymentDate DATE NOT NULL,
        PaymentStatus VARCHAR(30) NOT NULL DEFAULT 'PendingVerification', -- 'Unpaid', 'PendingVerification', 'Verified', 'Rejected', 'PartiallyPaid'
        PaymentProofPath VARCHAR(500) NULL,
        AdditionalRemarks VARCHAR(500) NULL,
        SubmittedAtUtc DATETIME NOT NULL DEFAULT GETUTCDATE(),
        VerifiedAtUtc DATETIME NULL,
        VerifiedBy VARCHAR(256) NULL,
        RejectionReason VARCHAR(500) NULL,
        CreatedAtUtc DATETIME NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAtUtc DATETIME NULL,
        CONSTRAINT FK_Payment_Event
            FOREIGN KEY (EventID) REFERENCES IAF_ReunionEvent(EventId),
        CONSTRAINT FK_Payment_PersonalData
            FOREIGN KEY (ServiceNo) REFERENCES IAF_PersonalData(ServiceNo)
            ON DELETE CASCADE
    );
    CREATE INDEX IX_IAF_Payment_AcknowledgementNumber ON IAF_Payment(AcknowledgementNumber);
    CREATE INDEX IX_IAF_Payment_Status ON IAF_Payment(PaymentStatus);
END
GO

-- 9. IAF Announcement Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IAF_Announcement')
BEGIN
    CREATE TABLE IAF_Announcement
    (
        AnnouncementId INT IDENTITY(1,1) PRIMARY KEY,
        EventId INT NULL,
        Title VARCHAR(200) NOT NULL,
        Content VARCHAR(MAX) NOT NULL,
        IsPublished BIT NOT NULL DEFAULT 1,
        PublishedAtUtc DATETIME NOT NULL DEFAULT GETUTCDATE(),
        AuthorName VARCHAR(100) NOT NULL DEFAULT 'Organizing Committee',
        CONSTRAINT FK_Announcement_Event
            FOREIGN KEY (EventId) REFERENCES IAF_ReunionEvent(EventId)
            ON DELETE SET NULL
    );
END
GO

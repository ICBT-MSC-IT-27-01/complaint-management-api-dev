-- ============================================================
-- CMS Database Setup — Create All Tables
-- Run this script FIRST before any stored procedures
-- ============================================================

-- Users
CREATE TABLE Users (
    Id                BIGINT        IDENTITY(1,1) PRIMARY KEY,
    Name              NVARCHAR(200) NOT NULL,
    Email             NVARCHAR(200) NOT NULL UNIQUE,
    Username          NVARCHAR(100) NOT NULL UNIQUE,
    PhoneNumber       NVARCHAR(20)  NULL,
    PasswordHash      NVARCHAR(500) NOT NULL,
    Role              NVARCHAR(50)  NOT NULL DEFAULT 'Agent',
    Department        NVARCHAR(100) NULL,
    ReportingManagerId BIGINT       NULL REFERENCES Users(Id),
    IsActive          BIT           NOT NULL DEFAULT 1,
    IsLocked          BIT           NOT NULL DEFAULT 0,
    CreatedDateTime   DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy         BIGINT        NOT NULL DEFAULT 0,
    UpdatedDateTime   DATETIME2     NULL,
    UpdatedBy         BIGINT        NULL,
    LastLoginDateTime DATETIME2     NULL
);

-- Clients
CREATE TABLE Clients (
    Id               BIGINT        IDENTITY(1,1) PRIMARY KEY,
    ClientCode       NVARCHAR(50)  NOT NULL UNIQUE,
    CompanyName      NVARCHAR(300) NOT NULL,
    PrimaryEmail     NVARCHAR(200) NOT NULL,
    PrimaryPhone     NVARCHAR(20)  NULL,
    Address          NVARCHAR(500) NULL,
    ClientType       NVARCHAR(50)  NOT NULL DEFAULT 'Standard',
    AccountManagerId BIGINT        NULL REFERENCES Users(Id),
    IsActive         BIT           NOT NULL DEFAULT 1,
    CreatedDateTime  DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy        BIGINT        NOT NULL DEFAULT 0,
    UpdatedDateTime  DATETIME2     NULL,
    UpdatedBy        BIGINT        NULL
);

-- Categories
CREATE TABLE Categories (
    Id               BIGINT        IDENTITY(1,1) PRIMARY KEY,
    Name             NVARCHAR(150) NOT NULL,
    ParentCategoryId BIGINT        NULL REFERENCES Categories(Id),
    SortOrder        INT           NOT NULL DEFAULT 0,
    IsActive         BIT           NOT NULL DEFAULT 1,
    CreatedDateTime  DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy        BIGINT        NOT NULL DEFAULT 0,
    UpdatedDateTime  DATETIME2     NULL,
    UpdatedBy        BIGINT        NULL
);

-- SLA Policies
CREATE TABLE SLAPolicies (
    Id                     BIGINT        IDENTITY(1,1) PRIMARY KEY,
    CategoryId             BIGINT        NOT NULL REFERENCES Categories(Id),
    Priority               NVARCHAR(20)  NOT NULL,
    ResponseTimeHours      INT           NOT NULL,
    ResolutionTimeHours    INT           NOT NULL,
    EscalationThresholdPct INT           NOT NULL DEFAULT 80,
    IsActive               BIT           NOT NULL DEFAULT 1,
    CreatedDateTime        DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy              BIGINT        NOT NULL DEFAULT 0,
    CONSTRAINT UQ_SlaPolicy UNIQUE (CategoryId, Priority)
);

-- Complaint Status Lookup
CREATE TABLE ComplaintStatuses (
    Id    BIGINT        IDENTITY(1,1) PRIMARY KEY,
    Name  NVARCHAR(50)  NOT NULL UNIQUE
);
INSERT INTO ComplaintStatuses (Name) VALUES
    ('New'),('Assigned'),('InProgress'),('Pending'),('Escalated'),('Resolved'),('Closed'),('Rejected');

-- Complaint Channels Lookup
CREATE TABLE ComplaintChannels (
    Id   BIGINT       IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE
);
INSERT INTO ComplaintChannels (Name) VALUES ('Phone'),('Email'),('Portal'),('WalkIn');

-- Complaints
CREATE TABLE Complaints (
    Id                  BIGINT         IDENTITY(1,1) PRIMARY KEY,
    ComplaintNumber     NVARCHAR(20)   NULL UNIQUE,
    ClientId            BIGINT         NULL REFERENCES Clients(Id),
    Name                NVARCHAR(300)  NULL,   -- ClientName snapshot
    ClientEmail         NVARCHAR(200)  NULL,
    ClientMobile        NVARCHAR(20)   NULL,
    ComplaintChannelId  BIGINT         NOT NULL REFERENCES ComplaintChannels(Id),
    ComplaintCategoryId BIGINT         NOT NULL REFERENCES Categories(Id),
    SubCategoryId       BIGINT         NULL REFERENCES Categories(Id),
    Subject             NVARCHAR(300)  NOT NULL,
    Description         NVARCHAR(MAX)  NOT NULL,
    Priority            NVARCHAR(20)   NOT NULL DEFAULT 'Medium',
    ComplaintStatusId   BIGINT         NOT NULL DEFAULT 1 REFERENCES ComplaintStatuses(Id),
    AssignedToUserId    BIGINT         NULL REFERENCES Users(Id),
    AssignedDate        DATETIME2      NULL,
    DueDate             DATETIME2      NULL,
    SlaStatus           NVARCHAR(20)   NOT NULL DEFAULT 'WithinSLA',
    IsSlaBreached       BIT            NOT NULL DEFAULT 0,
    IsResolved          BIT            NOT NULL DEFAULT 0,
    ResolvedDate        DATETIME2      NULL,
    ResolutionNotes     NVARCHAR(MAX)  NULL,
    IsClosed            BIT            NOT NULL DEFAULT 0,
    ClosedDate          DATETIME2      NULL,
    IsActive            BIT            NOT NULL DEFAULT 1,
    CreatedDateTime     DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy           BIGINT         NOT NULL DEFAULT 0,
    UpdatedDateTime     DATETIME2      NULL,
    UpdatedBy           BIGINT         NULL
);

-- Auto-generate ComplaintNumber trigger
CREATE TRIGGER trg_Complaints_RefNumber
ON Complaints AFTER INSERT AS
BEGIN
    UPDATE Complaints
    SET ComplaintNumber = 'CMP-' + FORMAT(YEAR(GETUTCDATE()),'0000')
        + RIGHT('00000' + CAST(Id AS NVARCHAR), 5)
    WHERE Id IN (SELECT Id FROM inserted);
END;
GO

-- Cases
CREATE TABLE Cases (
    Id               BIGINT        IDENTITY(1,1) PRIMARY KEY,
    CaseNumber       NVARCHAR(20)  NULL UNIQUE,
    ComplaintId      BIGINT        NOT NULL REFERENCES Complaints(Id),
    AssignedToUserId BIGINT        NULL REFERENCES Users(Id),
    Status           NVARCHAR(30)  NOT NULL DEFAULT 'Open',
    Notes            NVARCHAR(MAX) NULL,
    OpenedAt         DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    ClosedAt         DATETIME2     NULL
);

CREATE TRIGGER trg_Cases_Number ON Cases AFTER INSERT AS
BEGIN
    UPDATE Cases SET CaseNumber = 'CASE-' + FORMAT(YEAR(GETUTCDATE()),'0000')
        + RIGHT('00000' + CAST(Id AS NVARCHAR), 5)
    WHERE Id IN (SELECT Id FROM inserted);
END;
GO

-- CaseActivities
CREATE TABLE CaseActivities (
    Id                BIGINT        IDENTITY(1,1) PRIMARY KEY,
    CaseId            BIGINT        NOT NULL REFERENCES Cases(Id),
    ActivityType      NVARCHAR(50)  NOT NULL DEFAULT 'Note',
    Description       NVARCHAR(MAX) NOT NULL,
    PerformedByUserId BIGINT        NOT NULL REFERENCES Users(Id),
    CreatedDateTime   DATETIME2     NOT NULL DEFAULT GETUTCDATE()
);

-- Complaint History
CREATE TABLE ComplaintHistory (
    Id                BIGINT        IDENTITY(1,1) PRIMARY KEY,
    ComplaintId       BIGINT        NOT NULL REFERENCES Complaints(Id),
    Action            NVARCHAR(50)  NOT NULL,
    OldStatus         NVARCHAR(50)  NULL,
    NewStatus         NVARCHAR(50)  NULL,
    Note              NVARCHAR(MAX) NULL,
    PerformedByUserId BIGINT        NOT NULL REFERENCES Users(Id),
    CreatedDateTime   DATETIME2     NOT NULL DEFAULT GETUTCDATE()
);

-- Escalations
CREATE TABLE Escalations (
    Id                BIGINT        IDENTITY(1,1) PRIMARY KEY,
    ComplaintId       BIGINT        NOT NULL REFERENCES Complaints(Id),
    EscalatedByUserId BIGINT        NOT NULL REFERENCES Users(Id),
    EscalatedToUserId BIGINT        NOT NULL REFERENCES Users(Id),
    Reason            NVARCHAR(MAX) NOT NULL,
    EscalationType    NVARCHAR(30)  NOT NULL DEFAULT 'Manual',
    EscalatedAt       DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    ResolvedAt        DATETIME2     NULL
);

-- Attachments
CREATE TABLE Attachments (
    Id            BIGINT         IDENTITY(1,1) PRIMARY KEY,
    ComplaintId   BIGINT         NOT NULL REFERENCES Complaints(Id),
    FileName      NVARCHAR(300)  NOT NULL,
    FileType      NVARCHAR(100)  NOT NULL,
    FileSizeBytes BIGINT         NOT NULL,
    StoredPath    NVARCHAR(1000) NOT NULL,
    UploadedBy    BIGINT         NOT NULL REFERENCES Users(Id),
    UploadedAt    DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    IsActive      BIT            NOT NULL DEFAULT 1
);

-- AuditLogs (immutable — never UPDATE or DELETE)
CREATE TABLE AuditLogs (
    Id                BIGINT         IDENTITY(1,1) PRIMARY KEY,
    EntityType        NVARCHAR(100)  NOT NULL,
    EntityId          BIGINT         NOT NULL,
    Action            NVARCHAR(30)   NOT NULL,
    OldValues         NVARCHAR(MAX)  NULL,
    NewValues         NVARCHAR(MAX)  NULL,
    PerformedByUserId BIGINT         NULL REFERENCES Users(Id),
    IPAddress         NVARCHAR(50)   NULL,
    CreatedDateTime   DATETIME2      NOT NULL DEFAULT GETUTCDATE()
);

-- Seed default admin user (temporary plaintext password: Admin@123 — change immediately!)
INSERT INTO Users (Name, Email, Username, PasswordHash, Role, IsActive, CreatedBy)
VALUES ('System Admin', 'admin@cms.com', 'admin',
    'Admin@123', 'Admin', 1, 0);
GO

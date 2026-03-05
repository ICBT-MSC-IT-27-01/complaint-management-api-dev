CREATE OR ALTER PROCEDURE CMS_User_Create
    @Name        NVARCHAR(200),
    @Email       NVARCHAR(200),
    @Username    NVARCHAR(100),
    @PhoneNumber NVARCHAR(20)  = NULL,
    @PasswordHash NVARCHAR(500),
    @Role        NVARCHAR(50)  = 'Agent',
    @Department  NVARCHAR(100) = NULL,
    @ReportingManagerId BIGINT = NULL,
    @ActorUserId BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Users (Name, Email, Username, PhoneNumber, PasswordHash, Role, Department, ReportingManagerId, CreatedBy)
    VALUES (@Name, @Email, @Username, @PhoneNumber, @PasswordHash, @Role, @Department, @ReportingManagerId, @ActorUserId);

    DECLARE @NewId BIGINT = SCOPE_IDENTITY();
    EXEC CMS_User_GetById @NewId;
END;
GO

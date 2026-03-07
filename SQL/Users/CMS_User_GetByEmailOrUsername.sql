CREATE OR ALTER PROCEDURE CMS_User_GetByEmailOrUsername @EmailOrUsername NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Lookup NVARCHAR(200) = LTRIM(RTRIM(@EmailOrUsername));

    SELECT u.Id, u.Name, u.Email, u.Username, u.PhoneNumber, u.PasswordHash, u.Role, u.Department, u.ReportingManagerId,
           rm.Name AS ReportingManagerName,
           IsActive, IsLocked, CreatedDateTime, LastLoginDateTime
    FROM Users u
    LEFT JOIN Users rm ON rm.Id = u.ReportingManagerId
    WHERE LOWER(u.Email) = LOWER(@Lookup) OR LOWER(u.Username) = LOWER(@Lookup);
END;
GO

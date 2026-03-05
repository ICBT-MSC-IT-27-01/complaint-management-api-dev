CREATE OR ALTER PROCEDURE CMS_User_GetByEmailOrUsername @EmailOrUsername NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT u.Id, u.Name, u.Email, u.Username, u.PhoneNumber, u.PasswordHash, u.Role, u.Department, u.ReportingManagerId,
           rm.Name AS ReportingManagerName,
           IsActive, IsLocked, CreatedDateTime, LastLoginDateTime
    FROM Users u
    LEFT JOIN Users rm ON rm.Id = u.ReportingManagerId
    WHERE (u.Email = @EmailOrUsername OR u.Username = @EmailOrUsername) AND u.IsActive = 1;
END;
GO

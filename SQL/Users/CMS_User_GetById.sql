CREATE OR ALTER PROCEDURE CMS_User_GetById @Id BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT u.Id, u.Name, u.Email, u.Username, u.PhoneNumber, u.Role, u.Department, u.ReportingManagerId,
           rm.Name AS ReportingManagerName,
           IsActive, IsLocked, CreatedDateTime, LastLoginDateTime
    FROM Users u
    LEFT JOIN Users rm ON rm.Id = u.ReportingManagerId
    WHERE u.Id = @Id;
END;
GO

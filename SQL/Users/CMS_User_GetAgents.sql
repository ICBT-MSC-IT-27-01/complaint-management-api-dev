CREATE OR ALTER PROCEDURE CMS_User_GetAgents
AS
BEGIN
    SELECT u.Id, u.Name, u.Email, u.Username, u.PhoneNumber, u.Role, u.Department, u.ReportingManagerId,
           rm.Name AS ReportingManagerName, u.IsActive, u.IsLocked, u.CreatedDateTime, u.LastLoginDateTime
    FROM Users u
    LEFT JOIN Users rm ON rm.Id = u.ReportingManagerId
    WHERE u.Role IN ('Agent','Supervisor') AND u.IsActive = 1 ORDER BY u.Name;
END;
GO

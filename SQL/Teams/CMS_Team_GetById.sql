CREATE OR ALTER PROCEDURE CMS_Team_GetById
    @Id BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        t.Id,
        t.TeamCode,
        t.TeamName,
        t.LeadUserId,
        ISNULL(u.Name, '') AS LeadName,
        t.IsActive,
        (SELECT COUNT(1) FROM TeamMembers m WHERE m.TeamId = t.Id) AS MemberCount,
        t.CreatedDateTime
    FROM Teams t
    LEFT JOIN Users u ON u.Id = t.LeadUserId
    WHERE t.Id = @Id;

    SELECT
        m.UserId,
        u.Name,
        u.Email,
        u.Role
    FROM TeamMembers m
    INNER JOIN Users u ON u.Id = m.UserId
    WHERE m.TeamId = @Id
    ORDER BY u.Name;
END;
GO

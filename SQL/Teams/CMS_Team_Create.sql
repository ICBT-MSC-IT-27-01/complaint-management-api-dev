CREATE OR ALTER PROCEDURE CMS_Team_Create
    @TeamName NVARCHAR(150),
    @LeadUserId BIGINT = NULL,
    @ActorUserId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    IF @TeamName IS NULL OR LTRIM(RTRIM(@TeamName)) = ''
    BEGIN
        THROW 50010, 'Team name is required.', 1;
    END

    IF EXISTS (SELECT 1 FROM Teams WHERE TeamName = @TeamName)
    BEGIN
        THROW 50011, 'Team name already exists.', 1;
    END

    IF @LeadUserId IS NOT NULL
    BEGIN
        IF NOT EXISTS (
            SELECT 1
            FROM Users
            WHERE Id = @LeadUserId AND Role = 'Supervisor' AND IsActive = 1
        )
        BEGIN
            THROW 50012, 'Lead user must be an active Supervisor.', 1;
        END
    END

    INSERT INTO Teams (TeamCode, TeamName, LeadUserId, CreatedBy)
    VALUES ('', LTRIM(RTRIM(@TeamName)), @LeadUserId, @ActorUserId);

    DECLARE @Id BIGINT = SCOPE_IDENTITY();
    DECLARE @TeamCode NVARCHAR(20) = 'TEAM-' + RIGHT('00000' + CAST(@Id AS NVARCHAR(20)), 5);

    UPDATE Teams
    SET TeamCode = @TeamCode
    WHERE Id = @Id;

    SELECT
        t.Id,
        t.TeamCode,
        t.TeamName,
        t.LeadUserId,
        ISNULL(u.Name, '') AS LeadName,
        t.IsActive,
        CAST(0 AS INT) AS MemberCount,
        t.CreatedDateTime
    FROM Teams t
    LEFT JOIN Users u ON u.Id = t.LeadUserId
    WHERE t.Id = @Id;
END;
GO

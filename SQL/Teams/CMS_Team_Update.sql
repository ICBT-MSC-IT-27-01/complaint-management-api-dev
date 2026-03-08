CREATE OR ALTER PROCEDURE CMS_Team_Update
    @Id BIGINT,
    @TeamName NVARCHAR(150),
    @LeadUserId BIGINT = NULL,
    @IsActive BIT,
    @ActorUserId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Teams WHERE Id = @Id)
    BEGIN
        THROW 50013, 'Team not found.', 1;
    END

    IF @TeamName IS NULL OR LTRIM(RTRIM(@TeamName)) = ''
    BEGIN
        THROW 50014, 'Team name is required.', 1;
    END

    IF EXISTS (SELECT 1 FROM Teams WHERE TeamName = @TeamName AND Id <> @Id)
    BEGIN
        THROW 50015, 'Team name already exists.', 1;
    END

    IF @LeadUserId IS NOT NULL
    BEGIN
        IF NOT EXISTS (
            SELECT 1
            FROM Users
            WHERE Id = @LeadUserId AND Role = 'Supervisor' AND IsActive = 1
        )
        BEGIN
            THROW 50016, 'Lead user must be an active Supervisor.', 1;
        END
    END

    UPDATE Teams
    SET
        TeamName = LTRIM(RTRIM(@TeamName)),
        LeadUserId = @LeadUserId,
        IsActive = @IsActive,
        UpdatedDateTime = GETUTCDATE(),
        UpdatedBy = @ActorUserId
    WHERE Id = @Id;
END;
GO

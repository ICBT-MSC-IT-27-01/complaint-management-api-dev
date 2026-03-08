CREATE OR ALTER PROCEDURE CMS_Team_AddMember
    @TeamId BIGINT,
    @UserId BIGINT,
    @ActorUserId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Teams WHERE Id = @TeamId AND IsActive = 1)
    BEGIN
        THROW 50017, 'Team not found or inactive.', 1;
    END

    IF NOT EXISTS (
        SELECT 1
        FROM Users
        WHERE Id = @UserId AND IsActive = 1 AND Role IN ('Agent', 'Supervisor')
    )
    BEGIN
        THROW 50018, 'Only active Agent or Supervisor users can be assigned.', 1;
    END

    IF EXISTS (SELECT 1 FROM TeamMembers WHERE UserId = @UserId)
    BEGIN
        THROW 50019, 'User is already assigned to another team.', 1;
    END

    IF (SELECT COUNT(1) FROM TeamMembers WHERE TeamId = @TeamId) >= 5
    BEGIN
        THROW 50020, 'Team member limit exceeded. Maximum is 5.', 1;
    END

    INSERT INTO TeamMembers (TeamId, UserId, CreatedBy)
    VALUES (@TeamId, @UserId, @ActorUserId);
END;
GO

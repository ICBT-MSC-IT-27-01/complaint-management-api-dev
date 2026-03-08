CREATE OR ALTER PROCEDURE CMS_Team_RemoveMember
    @TeamId BIGINT,
    @UserId BIGINT,
    @ActorUserId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM TeamMembers WHERE TeamId = @TeamId AND UserId = @UserId)
    BEGIN
        THROW 50021, 'Team member not found.', 1;
    END

    DELETE FROM TeamMembers
    WHERE TeamId = @TeamId AND UserId = @UserId;

    UPDATE Teams
    SET
        UpdatedDateTime = GETUTCDATE(),
        UpdatedBy = @ActorUserId
    WHERE Id = @TeamId;
END;
GO

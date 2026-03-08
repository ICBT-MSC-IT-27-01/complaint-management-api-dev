CREATE OR ALTER PROCEDURE CMS_Team_Delete
    @Id BIGINT,
    @ActorUserId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Teams WHERE Id = @Id)
    BEGIN
        THROW 50022, 'Team not found.', 1;
    END

    DELETE FROM TeamMembers
    WHERE TeamId = @Id;

    DELETE FROM Teams
    WHERE Id = @Id;
END;
GO

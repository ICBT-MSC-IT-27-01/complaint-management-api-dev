CREATE OR ALTER PROCEDURE CMS_Category_Deactivate
    @Id BIGINT,
    @ActorUserId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Categories WHERE Id = @Id)
    BEGIN
        THROW 50001, 'Category does not exist.', 1;
    END

    UPDATE Categories
    SET IsActive = 0,
        UpdatedDateTime = GETUTCDATE(),
        UpdatedBy = @ActorUserId
    WHERE Id = @Id;
END;
GO

CREATE OR ALTER PROCEDURE CMS_ParentCategory_Create
    @Name NVARCHAR(150),
    @SortOrder INT = 0,
    @ActorUserId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM ParentCategories
        WHERE Name = @Name
    )
    BEGIN
        THROW 50001, 'Parent category name already exists.', 1;
    END

    INSERT INTO ParentCategories (Name, SortOrder, CreatedBy)
    VALUES (@Name, @SortOrder, @ActorUserId);

    DECLARE @Id BIGINT = SCOPE_IDENTITY();

    SELECT p.Id, p.Name, CAST(NULL AS BIGINT) AS ParentCategoryId, CAST(NULL AS NVARCHAR(150)) AS ParentName, p.SortOrder, p.IsActive
    FROM ParentCategories p
    WHERE p.Id = @Id;
END;
GO

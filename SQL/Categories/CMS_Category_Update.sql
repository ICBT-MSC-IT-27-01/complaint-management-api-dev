CREATE OR ALTER PROCEDURE CMS_Category_Update
    @Id BIGINT,
    @Name NVARCHAR(150),
    @ParentCategoryId BIGINT,
    @SortOrder INT = 0,
    @ActorUserId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Categories WHERE Id = @Id)
    BEGIN
        THROW 50001, 'Category does not exist.', 1;
    END

    IF @ParentCategoryId IS NULL OR @ParentCategoryId <= 0
    BEGIN
        THROW 50002, 'Parent category is required.', 1;
    END

    IF NOT EXISTS (
        SELECT 1
        FROM ParentCategories
        WHERE Id = @ParentCategoryId
          AND IsActive = 1
    )
    BEGIN
        THROW 50003, 'Selected parent category does not exist or is inactive.', 1;
    END

    IF EXISTS (
        SELECT 1
        FROM Categories
        WHERE Name = @Name
          AND ParentCategoryId = @ParentCategoryId
          AND Id <> @Id
    )
    BEGIN
        THROW 50004, 'Category name already exists under the selected parent category.', 1;
    END

    UPDATE Categories
    SET Name = @Name,
        ParentCategoryId = @ParentCategoryId,
        SortOrder = @SortOrder,
        UpdatedDateTime = GETUTCDATE(),
        UpdatedBy = @ActorUserId
    WHERE Id = @Id;
END;
GO

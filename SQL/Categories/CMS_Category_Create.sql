CREATE OR ALTER PROCEDURE CMS_Category_Create
    @Name NVARCHAR(150),
    @ParentCategoryId BIGINT,
    @SortOrder INT = 0,
    @ActorUserId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    IF @ParentCategoryId IS NULL OR @ParentCategoryId <= 0
    BEGIN
        THROW 50001, 'Parent category is required.', 1;
    END

    IF NOT EXISTS (
        SELECT 1
        FROM ParentCategories
        WHERE Id = @ParentCategoryId
          AND IsActive = 1
    )
    BEGIN
        THROW 50002, 'Selected parent category does not exist or is inactive.', 1;
    END

    IF EXISTS (
        SELECT 1
        FROM Categories
        WHERE Name = @Name
          AND ParentCategoryId = @ParentCategoryId
    )
    BEGIN
        THROW 50003, 'Category name already exists under the selected parent category.', 1;
    END

    INSERT INTO Categories (Name, ParentCategoryId, SortOrder, CreatedBy)
    VALUES (@Name, @ParentCategoryId, @SortOrder, @ActorUserId);

    DECLARE @Id BIGINT = SCOPE_IDENTITY();

    SELECT c.Id, c.Name, c.ParentCategoryId, p.Name AS ParentName, c.SortOrder, c.IsActive
    FROM Categories c
    LEFT JOIN ParentCategories p ON p.Id = c.ParentCategoryId
    WHERE c.Id = @Id;
END;
GO

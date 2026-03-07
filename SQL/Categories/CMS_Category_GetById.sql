CREATE OR ALTER PROCEDURE CMS_Category_GetById
    @Id BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT c.Id, c.Name, c.ParentCategoryId, p.Name AS ParentName, c.SortOrder, c.IsActive
    FROM Categories c
    LEFT JOIN ParentCategories p ON p.Id = c.ParentCategoryId
    WHERE c.Id = @Id;
END;
GO

CREATE OR ALTER PROCEDURE CMS_Category_GetAll
AS
BEGIN
    SET NOCOUNT ON;

    SELECT c.Id, c.Name, c.ParentCategoryId, p.Name AS ParentName, c.SortOrder, c.IsActive
    FROM Categories c
    INNER JOIN ParentCategories p ON p.Id = c.ParentCategoryId
    ORDER BY p.SortOrder, p.Name, c.SortOrder, c.Name;
END;
GO

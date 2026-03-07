CREATE OR ALTER PROCEDURE CMS_ParentCategory_GetAll
AS
BEGIN
    SET NOCOUNT ON;

    SELECT p.Id, p.Name, CAST(NULL AS BIGINT) AS ParentCategoryId, CAST(NULL AS NVARCHAR(150)) AS ParentName, p.SortOrder, p.IsActive
    FROM ParentCategories p
    WHERE p.IsActive = 1
    ORDER BY p.SortOrder, p.Name;
END;
GO

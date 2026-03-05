CREATE OR ALTER PROCEDURE CMS_User_Search
    @Keyword  NVARCHAR(200) = NULL,
    @Role     NVARCHAR(50)  = NULL,
    @Department NVARCHAR(100) = NULL,
    @IsActive BIT           = NULL,
    @Page     INT           = 1,
    @PageSize INT           = 20
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@Page - 1) * @PageSize;
    SELECT u.Id, u.Name, u.Email, u.Username, u.PhoneNumber, u.Role, u.Department, u.ReportingManagerId,
           rm.Name AS ReportingManagerName, u.IsActive, u.IsLocked, u.CreatedDateTime, u.LastLoginDateTime
    FROM Users u
    LEFT JOIN Users rm ON rm.Id = u.ReportingManagerId
    WHERE (@Keyword  IS NULL OR u.Name LIKE '%' + @Keyword + '%' OR u.Email LIKE '%' + @Keyword + '%')
      AND (@Role     IS NULL OR u.Role = @Role)
      AND (@Department IS NULL OR u.Department = @Department)
      AND (@IsActive IS NULL OR u.IsActive = @IsActive)
    ORDER BY u.CreatedDateTime DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT_BIG(*) FROM Users u
    WHERE (@Keyword  IS NULL OR u.Name LIKE '%' + @Keyword + '%' OR u.Email LIKE '%' + @Keyword + '%')
      AND (@Role     IS NULL OR u.Role = @Role)
      AND (@Department IS NULL OR u.Department = @Department)
      AND (@IsActive IS NULL OR u.IsActive = @IsActive);
END;
GO

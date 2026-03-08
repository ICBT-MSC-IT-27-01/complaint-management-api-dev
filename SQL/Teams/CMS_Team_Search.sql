CREATE OR ALTER PROCEDURE CMS_Team_Search
    @Q NVARCHAR(150) = NULL,
    @IsActive BIT = NULL,
    @Page INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    IF @Page < 1 SET @Page = 1;
    IF @PageSize < 1 SET @PageSize = 20;
    IF @PageSize > 100 SET @PageSize = 100;

    ;WITH t AS
    (
        SELECT
            tm.Id,
            tm.TeamCode,
            tm.TeamName,
            tm.LeadUserId,
            ISNULL(u.Name, '') AS LeadName,
            tm.IsActive,
            (SELECT COUNT(1) FROM TeamMembers m WHERE m.TeamId = tm.Id) AS MemberCount,
            tm.CreatedDateTime
        FROM Teams tm
        LEFT JOIN Users u ON u.Id = tm.LeadUserId
        WHERE
            (@Q IS NULL OR @Q = '' OR tm.TeamName LIKE '%' + @Q + '%' OR tm.TeamCode LIKE '%' + @Q + '%')
            AND (@IsActive IS NULL OR tm.IsActive = @IsActive)
    )
    SELECT *
    FROM t
    ORDER BY TeamName
    OFFSET (@Page - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(1) AS TotalCount
    FROM Teams tm
    WHERE
        (@Q IS NULL OR @Q = '' OR tm.TeamName LIKE '%' + @Q + '%' OR tm.TeamCode LIKE '%' + @Q + '%')
        AND (@IsActive IS NULL OR tm.IsActive = @IsActive);
END;
GO

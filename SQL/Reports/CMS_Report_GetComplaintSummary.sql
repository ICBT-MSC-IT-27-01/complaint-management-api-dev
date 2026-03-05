CREATE OR ALTER PROCEDURE CMS_Report_GetComplaintSummary
    @From DATETIME2 = NULL,
    @To DATETIME2 = NULL,
    @AgentUserId BIGINT = NULL,
    @CategoryId BIGINT = NULL,
    @Department NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH Base AS
    (
        SELECT c.*
        FROM Complaints c
        LEFT JOIN Users u ON u.Id = c.AssignedToUserId
        WHERE c.IsActive = 1
          AND (@From IS NULL OR c.CreatedDateTime >= @From)
          AND (@To IS NULL OR c.CreatedDateTime <= @To)
          AND (@AgentUserId IS NULL OR c.AssignedToUserId = @AgentUserId)
          AND (@CategoryId IS NULL OR c.ComplaintCategoryId = @CategoryId)
          AND (@Department IS NULL OR u.Department = @Department)
    )
    SELECT
        COUNT(*) AS TotalComplaints,
        SUM(CASE WHEN ComplaintStatusId = 1 THEN 1 ELSE 0 END) AS NewCount,
        SUM(CASE WHEN ComplaintStatusId = 3 THEN 1 ELSE 0 END) AS InProgressCount,
        SUM(CASE WHEN ComplaintStatusId = 6 THEN 1 ELSE 0 END) AS ResolvedCount,
        SUM(CASE WHEN ComplaintStatusId = 7 THEN 1 ELSE 0 END) AS ClosedCount,
        SUM(CASE WHEN ComplaintStatusId = 5 THEN 1 ELSE 0 END) AS EscalatedCount,
        SUM(CASE WHEN SlaStatus = 'Breached' THEN 1 ELSE 0 END) AS SlaBreachedCount
    FROM Base;

    SELECT
        u.Id AS UserId,
        u.Name AS AgentName,
        COUNT(c.Id) AS Assigned,
        SUM(CASE WHEN c.ComplaintStatusId = 6 THEN 1 ELSE 0 END) AS Resolved,
        AVG(CASE WHEN c.IsResolved = 1 AND c.ResolvedDate IS NOT NULL THEN DATEDIFF(MINUTE, c.CreatedDateTime, c.ResolvedDate) / 60.0 END) AS AvgResolutionHours
    FROM Complaints c
    JOIN Users u ON u.Id = c.AssignedToUserId
    WHERE c.IsActive = 1
      AND (@From IS NULL OR c.CreatedDateTime >= @From)
      AND (@To IS NULL OR c.CreatedDateTime <= @To)
      AND (@AgentUserId IS NULL OR c.AssignedToUserId = @AgentUserId)
      AND (@Department IS NULL OR u.Department = @Department)
    GROUP BY u.Id, u.Name
    ORDER BY u.Name;

    SELECT cat.Name AS Category, COUNT(c.Id) AS Count
    FROM Complaints c
    JOIN Categories cat ON cat.Id = c.ComplaintCategoryId
    LEFT JOIN Users u ON u.Id = c.AssignedToUserId
    WHERE c.IsActive = 1
      AND (@From IS NULL OR c.CreatedDateTime >= @From)
      AND (@To IS NULL OR c.CreatedDateTime <= @To)
      AND (@CategoryId IS NULL OR c.ComplaintCategoryId = @CategoryId)
      AND (@Department IS NULL OR u.Department = @Department)
    GROUP BY cat.Name
    ORDER BY cat.Name;
END;
GO

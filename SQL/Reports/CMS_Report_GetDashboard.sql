CREATE OR ALTER PROCEDURE CMS_Report_GetDashboard
    @ActorUserId BIGINT,
    @Role NVARCHAR(50),
    @Period NVARCHAR(20) = '30d',
    @From DATETIME2 = NULL,
    @To DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @FromDate DATETIME2 = ISNULL(@From,
        CASE
            WHEN @Period='year' THEN DATEADD(YEAR,-1,GETUTCDATE())
            WHEN @Period='quarter' THEN DATEADD(MONTH,-3,GETUTCDATE())
            ELSE DATEADD(DAY,-30,GETUTCDATE())
        END);
    DECLARE @ToDate DATETIME2 = ISNULL(@To, GETUTCDATE());

    -- KPI row
    SELECT
        COUNT(*) AS TotalComplaints,
        SUM(CASE WHEN ComplaintStatusId NOT IN (6,7) THEN 1 ELSE 0 END) AS OpenComplaints,
        SUM(CASE WHEN IsResolved=1 AND CAST(ResolvedDate AS DATE)=CAST(GETUTCDATE() AS DATE) THEN 1 ELSE 0 END) AS ResolvedToday,
        SUM(CASE WHEN SlaStatus='Breached' THEN 1 ELSE 0 END) AS SlaBreached,
        SUM(CASE WHEN SlaStatus='AtRisk' THEN 1 ELSE 0 END) AS SlaAtRisk,
        SUM(CASE WHEN AssignedToUserId=@ActorUserId AND ComplaintStatusId NOT IN (6,7) THEN 1 ELSE 0 END) AS MyOpenComplaints,
        AVG(CASE WHEN IsResolved=1 AND ResolvedDate IS NOT NULL
            THEN DATEDIFF(MINUTE, CreatedDateTime, ResolvedDate) / 60.0 END) AS AvgResolutionHours
    FROM Complaints WHERE IsActive=1
        AND CreatedDateTime >= @FromDate AND CreatedDateTime <= @ToDate;

    -- By status
    SELECT cs.Name AS Status, COUNT(c.Id) AS Count
    FROM ComplaintStatuses cs LEFT JOIN Complaints c ON c.ComplaintStatusId=cs.Id AND c.IsActive=1
        AND c.CreatedDateTime >= @FromDate AND c.CreatedDateTime <= @ToDate
    GROUP BY cs.Id, cs.Name ORDER BY cs.Id;

    -- By priority
    SELECT Priority, COUNT(*) AS Count FROM Complaints
    WHERE IsActive=1 AND CreatedDateTime >= @FromDate AND CreatedDateTime <= @ToDate
    GROUP BY Priority ORDER BY Priority;
END;
GO

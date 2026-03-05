CREATE OR ALTER PROCEDURE CMS_Complaint_Search
    @StatusId         BIGINT       = NULL,
    @CategoryId       BIGINT       = NULL,
    @ChannelId        BIGINT       = NULL,
    @Department       NVARCHAR(100)= NULL,
    @Priority         NVARCHAR(20) = NULL,
    @AssignedToUserId BIGINT       = NULL,
    @Q                NVARCHAR(300)= NULL,
    @From             DATETIME2    = NULL,
    @To               DATETIME2    = NULL,
    @Page             INT          = 1,
    @PageSize         INT          = 20
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@Page - 1) * @PageSize;

    -- Result set 1: items
    SELECT
        c.Id, c.ComplaintNumber, c.Subject, c.Priority,
        cs.Name AS Status, c.ComplaintStatusId,
        cat.Name AS Category,
        c.Name AS ClientName,
        u.Name AS AssignedToName,
        c.SlaStatus, c.DueDate,
        c.CreatedDateTime, c.IsActive
    FROM Complaints c
    LEFT JOIN ComplaintStatuses cs ON cs.Id = c.ComplaintStatusId
    LEFT JOIN Categories cat       ON cat.Id = c.ComplaintCategoryId
    LEFT JOIN Users u              ON u.Id   = c.AssignedToUserId
    WHERE c.IsActive = 1
      AND (@StatusId   IS NULL OR c.ComplaintStatusId  = @StatusId)
      AND (@CategoryId IS NULL OR c.ComplaintCategoryId= @CategoryId)
      AND (@ChannelId  IS NULL OR c.ComplaintChannelId = @ChannelId)
      AND (@Department IS NULL OR u.Department = @Department)
      AND (@Priority   IS NULL OR c.Priority           = @Priority)
      AND (@AssignedToUserId IS NULL OR c.AssignedToUserId = @AssignedToUserId)
      AND (@Q          IS NULL OR c.Subject LIKE '%' + @Q + '%'
                               OR c.ComplaintNumber LIKE '%' + @Q + '%'
                               OR c.Name LIKE '%' + @Q + '%')
      AND (@From       IS NULL OR c.CreatedDateTime >= @From)
      AND (@To         IS NULL OR c.CreatedDateTime <= @To)
    ORDER BY c.CreatedDateTime DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

    -- Result set 2: total count
    SELECT COUNT_BIG(*) FROM Complaints c
    LEFT JOIN Users u ON u.Id = c.AssignedToUserId
    WHERE c.IsActive = 1
      AND (@StatusId   IS NULL OR c.ComplaintStatusId  = @StatusId)
      AND (@CategoryId IS NULL OR c.ComplaintCategoryId= @CategoryId)
      AND (@ChannelId  IS NULL OR c.ComplaintChannelId = @ChannelId)
      AND (@Department IS NULL OR u.Department = @Department)
      AND (@Priority   IS NULL OR c.Priority           = @Priority)
      AND (@AssignedToUserId IS NULL OR c.AssignedToUserId = @AssignedToUserId)
      AND (@Q          IS NULL OR c.Subject LIKE '%' + @Q + '%'
                               OR c.ComplaintNumber LIKE '%' + @Q + '%'
                               OR c.Name LIKE '%' + @Q + '%')
      AND (@From       IS NULL OR c.CreatedDateTime >= @From)
      AND (@To         IS NULL OR c.CreatedDateTime <= @To);
END;
GO

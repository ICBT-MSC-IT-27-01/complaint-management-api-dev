CREATE OR ALTER PROCEDURE CMS_Complaint_Resolve
    @ComplaintId      BIGINT,
    @ResolutionSummary NVARCHAR(MAX),
    @RootCause        NVARCHAR(MAX) = NULL,
    @FixApplied       NVARCHAR(MAX) = NULL,
    @ActorUserId      BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ResolvedAtUtc DATETIME2 = GETUTCDATE();

    UPDATE Complaints SET
        IsResolved        = 1,
        ResolvedDate      = @ResolvedAtUtc,
        ResolutionNotes   = @ResolutionSummary,
        ComplaintStatusId = 6, -- Resolved
        UpdatedDateTime   = @ResolvedAtUtc,
        UpdatedBy         = @ActorUserId
    WHERE Id = @ComplaintId;

    UPDATE Cases SET Status = 'Resolved', ClosedAt = @ResolvedAtUtc
    WHERE ComplaintId = @ComplaintId AND Status != 'Closed';

    INSERT INTO ComplaintHistory (ComplaintId, Action, NewStatus, Note, PerformedByUserId)
    VALUES (@ComplaintId, 'Resolve', 'Resolved', @ResolutionSummary, @ActorUserId);

    -- Send resolution email to client (non-blocking)
    BEGIN TRY
        DECLARE @ToEmail         NVARCHAR(200);
        DECLARE @ClientNameOut   NVARCHAR(300);
        DECLARE @ClientMobileOut NVARCHAR(20);
        DECLARE @ComplaintNo     NVARCHAR(20);
        DECLARE @Subject         NVARCHAR(300);
        DECLARE @Description     NVARCHAR(MAX);
        DECLARE @Priority        NVARCHAR(20);
        DECLARE @ChannelName     NVARCHAR(50);
        DECLARE @CategoryName    NVARCHAR(150);
        DECLARE @SubCategoryName NVARCHAR(150);
        DECLARE @ResolvedOnText  NVARCHAR(30);
        DECLARE @Body            NVARCHAR(MAX);
        DECLARE @MailSubject     NVARCHAR(300);

        SELECT
            @ToEmail         = c.ClientEmail,
            @ClientNameOut   = c.Name,
            @ClientMobileOut = c.ClientMobile,
            @ComplaintNo     = c.ComplaintNumber,
            @Subject         = c.Subject,
            @Description     = c.Description,
            @Priority        = c.Priority,
            @ChannelName     = ch.Name,
            @CategoryName    = cat.Name,
            @SubCategoryName = scat.Name
        FROM Complaints c
        LEFT JOIN ComplaintChannels ch ON ch.Id = c.ComplaintChannelId
        LEFT JOIN Categories cat       ON cat.Id = c.ComplaintCategoryId
        LEFT JOIN Categories scat      ON scat.Id = c.SubCategoryId
        WHERE c.Id = @ComplaintId;

        IF @ToEmail IS NOT NULL AND LTRIM(RTRIM(@ToEmail)) <> ''
        BEGIN
            SET @ResolvedOnText = CONVERT(NVARCHAR(19), @ResolvedAtUtc, 120) + ' UTC';

            SET @Body =
                N'<h3>Complaint Resolved</h3>' +
                N'<p>Your complaint has been marked as resolved.</p>' +
                N'<h4>Client Details</h4>' +
                N'<p>' +
                N'<b>Name:</b> ' + ISNULL(@ClientNameOut, N'-') + N'<br/>' +
                N'<b>Email:</b> ' + ISNULL(@ToEmail, N'-') + N'<br/>' +
                N'<b>Mobile:</b> ' + ISNULL(@ClientMobileOut, N'-') +
                N'</p>' +
                N'<h4>Complaint Details</h4>' +
                N'<p>' +
                N'<b>Complaint No:</b> ' + ISNULL(@ComplaintNo, N'-') + N'<br/>' +
                N'<b>Subject:</b> ' + ISNULL(@Subject, N'-') + N'<br/>' +
                N'<b>Description:</b> ' + ISNULL(@Description, N'-') + N'<br/>' +
                N'<b>Priority:</b> ' + ISNULL(@Priority, N'-') + N'<br/>' +
                N'<b>Channel:</b> ' + ISNULL(@ChannelName, N'-') + N'<br/>' +
                N'<b>Category:</b> ' + ISNULL(@CategoryName, N'-') + N'<br/>' +
                N'<b>Sub Category:</b> ' + ISNULL(@SubCategoryName, N'-') + N'<br/>' +
                N'<b>Resolved On:</b> ' + ISNULL(@ResolvedOnText, N'-') +
                N'</p>' +
                N'<h4>Action Taken</h4>' +
                N'<p>' +
                N'<b>Resolution Summary:</b> ' + ISNULL(@ResolutionSummary, N'-') + N'<br/>' +
                N'<b>Root Cause:</b> ' + ISNULL(@RootCause, N'-') + N'<br/>' +
                N'<b>Fix Applied:</b> ' + ISNULL(@FixApplied, N'-') +
                N'</p>';

            SET @MailSubject = N'Complaint Resolved - ' + ISNULL(@ComplaintNo, N'ID ' + CONVERT(NVARCHAR(20), @ComplaintId));

            EXEC msdb.dbo.sp_send_dbmail
                @profile_name = 'CMS Mail Profile',
                @recipients   = @ToEmail,
                @subject      = @MailSubject,
                @body         = @Body,
                @body_format  = 'HTML';
        END
    END TRY
    BEGIN CATCH
        -- Do not fail complaint resolution if email fails
        PRINT CONCAT('Complaint resolve mail failed for ComplaintId=', @ComplaintId, '. ', ERROR_MESSAGE());
    END CATCH
END;
GO

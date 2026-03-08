-- CMS_Complaint_Create
-- Creates a new complaint and automatically creates the linked Case
CREATE OR ALTER PROCEDURE CMS_Complaint_Create
    @ClientId            BIGINT         = NULL,
    @ClientName          NVARCHAR(300)  = NULL,
    @ClientEmail         NVARCHAR(200)  = NULL,
    @ClientMobile        NVARCHAR(20)   = NULL,
    @ComplaintChannelId  BIGINT,
    @ComplaintCategoryId BIGINT,
    @SubCategoryId       BIGINT         = NULL,
    @Subject             NVARCHAR(300),
    @Description         NVARCHAR(MAX),
    @Priority            NVARCHAR(20)   = 'Medium',
    @ActorUserId         BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @ComplaintChannelId IS NULL OR @ComplaintChannelId <= 0
        THROW 50001, 'ComplaintChannelId is required.', 1;

    IF @ComplaintCategoryId IS NULL OR @ComplaintCategoryId <= 0
        THROW 50002, 'ComplaintCategoryId is required.', 1;

    IF NOT EXISTS (SELECT 1 FROM ComplaintChannels WHERE Id = @ComplaintChannelId)
        THROW 50003, 'Invalid ComplaintChannelId.', 1;

    IF NOT EXISTS (SELECT 1 FROM Categories WHERE Id = @ComplaintCategoryId)
        THROW 50004, 'Invalid ComplaintCategoryId.', 1;

    IF @SubCategoryId IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM Categories WHERE Id = @SubCategoryId)
        THROW 50005, 'Invalid SubCategoryId.', 1;

    IF NOT EXISTS (SELECT 1 FROM Users WHERE Id = @ActorUserId)
        THROW 50006, 'Invalid ActorUserId.', 1;

    -- Calculate SLA due date
    DECLARE @DueDate DATETIME2 = NULL;
    DECLARE @ResolutionHours INT;
    SELECT @ResolutionHours = ResolutionTimeHours
    FROM SLAPolicies
    WHERE CategoryId = @ComplaintCategoryId AND Priority = @Priority AND IsActive = 1;
    IF @ResolutionHours IS NOT NULL
        SET @DueDate = DATEADD(HOUR, @ResolutionHours, GETUTCDATE());

    DECLARE @ComplaintId BIGINT;
    DECLARE @CreatedDateUtc DATETIME2;

    BEGIN TRY
        BEGIN TRANSACTION;
        -- Insert complaint
        INSERT INTO Complaints (
            ClientId, Name, ClientEmail, ClientMobile,
            ComplaintChannelId, ComplaintCategoryId, SubCategoryId,
            Subject, Description, Priority, DueDate, CreatedBy
        )
        VALUES (
            @ClientId, @ClientName, @ClientEmail, @ClientMobile,
            @ComplaintChannelId, @ComplaintCategoryId, @SubCategoryId,
            @Subject, @Description, @Priority, @DueDate, @ActorUserId
        );

        SET @ComplaintId = SCOPE_IDENTITY();

        IF @ComplaintId IS NULL
            THROW 50007, 'Failed to create complaint.', 1;

        -- Auto-create linked Case
        INSERT INTO Cases (ComplaintId, AssignedToUserId, Status)
        VALUES (@ComplaintId, NULL, 'Open');

        -- Log history
        INSERT INTO ComplaintHistory (ComplaintId, Action, NewStatus, PerformedByUserId)
        VALUES (@ComplaintId, 'Create', 'New', @ActorUserId);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH

    SET @CreatedDateUtc = (
        SELECT c.CreatedDateTime
        FROM Complaints c
        WHERE c.Id = @ComplaintId
    );

    -- Send email notification to client (non-blocking)
    BEGIN TRY
        DECLARE @ToEmail         NVARCHAR(200);
        DECLARE @ClientNameOut   NVARCHAR(300);
        DECLARE @ClientMobileOut NVARCHAR(20);
        DECLARE @ComplaintNo     NVARCHAR(20);
        DECLARE @ChannelName     NVARCHAR(50);
        DECLARE @CategoryName    NVARCHAR(150);
        DECLARE @SubCategoryName NVARCHAR(150);
        DECLARE @Body            NVARCHAR(MAX);
        DECLARE @CreatedOnText   NVARCHAR(30);
        DECLARE @MailSubject     NVARCHAR(300);

        SELECT
            @ToEmail         = c.ClientEmail,
            @ClientNameOut   = c.Name,
            @ClientMobileOut = c.ClientMobile,
            @ComplaintNo     = c.ComplaintNumber,
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
            SET @CreatedOnText = CONVERT(NVARCHAR(19), @CreatedDateUtc, 120) + ' UTC';

            SET @Body =
                N'<h3>Complaint Created</h3>' +
                N'<p>Your complaint has been successfully created.</p>' +
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
                N'<b>Created On:</b> ' + ISNULL(@CreatedOnText, N'-') +
                N'</p>';

            SET @MailSubject = N'Complaint Created - ' + ISNULL(@ComplaintNo, N'ID ' + CONVERT(NVARCHAR(20), @ComplaintId));

            EXEC msdb.dbo.sp_send_dbmail
                @profile_name = 'CMS Mail Profile',
                @recipients   = @ToEmail,
                @subject      = @MailSubject,
                @body         = @Body,
                @body_format  = 'HTML';
        END
    END TRY
    BEGIN CATCH
        -- Do not fail complaint creation if email fails
        PRINT CONCAT('Complaint create mail failed for ComplaintId=', @ComplaintId, '. ', ERROR_MESSAGE());
    END CATCH

    -- Return full complaint detail
    EXEC CMS_Complaint_GetById @ComplaintId;
END;
GO

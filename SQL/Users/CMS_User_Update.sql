CREATE OR ALTER PROCEDURE CMS_User_Update
    @Id          BIGINT, @Name NVARCHAR(200), @Email NVARCHAR(200),
    @Username    NVARCHAR(100), @PhoneNumber NVARCHAR(20) = NULL,
    @Role        NVARCHAR(50), @Department NVARCHAR(100) = NULL,
    @ReportingManagerId BIGINT = NULL, @ActorUserId BIGINT
AS
BEGIN
    UPDATE Users SET Name=@Name, Email=@Email, Username=@Username,
        PhoneNumber=@PhoneNumber, Role=@Role, Department=@Department, ReportingManagerId=@ReportingManagerId,
        UpdatedDateTime=GETUTCDATE(), UpdatedBy=@ActorUserId
    WHERE Id = @Id;
END;
GO

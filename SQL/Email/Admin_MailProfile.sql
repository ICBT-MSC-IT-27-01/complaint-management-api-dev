SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE Admin_MailProfile
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM msdb.dbo.sysmail_account
        WHERE name = 'CMS Mail Account'
    )
    BEGIN
        EXEC msdb.dbo.sysmail_add_account_sp
            @account_name    = 'CMS Mail Account',
            @description     = 'Complaint Management System Mail',
            @email_address   = 'support.complimatecms@gmail.com',
            @display_name    = 'Complaint Management System',
            @mailserver_name = 'smtp.gmail.com',
            @port            = 587,
            @enable_ssl      = 1,
            @username        = 'support.complimatecms@gmail.com',
            @password        = 'vnferkawxvvxcuss';
    END
    ELSE
    BEGIN
        EXEC msdb.dbo.sysmail_update_account_sp
            @account_name    = 'CMS Mail Account',
            @description     = 'Complaint Management System Mail',
            @email_address   = 'support.complimatecms@gmail.com',
            @display_name    = 'Complaint Management System',
            @mailserver_name = 'smtp.gmail.com',
            @port            = 587,
            @enable_ssl      = 1,
            @username        = 'support.complimatecms@gmail.com',
            @password        = 'vnferkawxvvxcuss';
    END

    IF NOT EXISTS (
        SELECT 1
        FROM msdb.dbo.sysmail_profile
        WHERE name = 'CMS Mail Profile'
    )
    BEGIN
        EXEC msdb.dbo.sysmail_add_profile_sp
            @profile_name = 'CMS Mail Profile',
            @description  = 'Mail profile for Complaint Management System';
    END

    IF NOT EXISTS (
        SELECT 1
        FROM msdb.dbo.sysmail_profileaccount pa
        INNER JOIN msdb.dbo.sysmail_profile p ON p.profile_id = pa.profile_id
        INNER JOIN msdb.dbo.sysmail_account a ON a.account_id = pa.account_id
        WHERE p.name = 'CMS Mail Profile'
          AND a.name = 'CMS Mail Account'
    )
    BEGIN
        EXEC msdb.dbo.sysmail_add_profileaccount_sp
            @profile_name    = 'CMS Mail Profile',
            @account_name    = 'CMS Mail Account',
            @sequence_number = 1;
    END

    IF NOT EXISTS (
        SELECT 1
        FROM msdb.dbo.sysmail_principalprofile pp
        INNER JOIN msdb.dbo.sysmail_profile p ON p.profile_id = pp.profile_id
        WHERE p.name = 'CMS Mail Profile'
          AND pp.is_default = 1
    )
    BEGIN
        EXEC msdb.dbo.sysmail_add_principalprofile_sp
            @profile_name   = 'CMS Mail Profile',
            @principal_name = 'public',
            @is_default     = 1;
    END
END
GO

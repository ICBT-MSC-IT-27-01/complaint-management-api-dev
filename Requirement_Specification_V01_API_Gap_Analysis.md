# Requirement Specification V01 vs Implemented API - Missing Points

## Scope and Source
- Requested source: `Requirement Specification_V01.pdf` (`c:\CMS\Project\icbt doc\Requirement Specification_V01.pdf`).
- Due to unavailable PDF text extraction utilities in this environment, the analysis used the paired source file `Requirement Specification_V01.docx` in the same folder (same version/time window) to derive requirements.
- API baseline reviewed from controllers, DTOs, services, and SQL stored procedures in this repo.

## Missing Points (Requirements Not Yet Covered by APIs)

### 1) Authentication and Account Security
- **Forgot password flow is missing**.
  - SRS includes a "Forgot Password" action on Login.
  - API has only `POST /api/v1/auth/login` and `POST /api/v1/auth/client/register`.
  - Evidence: `Cd.Cms.Api/Controllers/AuthController.cs:15`, `Cd.Cms.Api/Controllers/AuthController.cs:37`.

- **Failed-login lockout policy (e.g., lock after 5 attempts) is not implemented**.
  - SRS requires configurable lockout behavior.
  - Current auth checks `IsLocked` but does not increment failed-attempt counters or trigger lockout logic.
  - Evidence: `Cd.Cms.Application/Services/AuthService.cs:41`.

- **Password complexity rules are incomplete**.
  - SRS requires uppercase/lowercase/number complexity.
  - API enforces minimum length and confirm-password match only.
  - Evidence: `Cd.Cms.Application/Services/AuthService.cs:79`, `Cd.Cms.Application/Services/UserService.cs:26`.

- **Session inactivity timeout/revocation API is missing**.
  - SRS requires configurable inactivity timeout and account session controls.
  - No API for session listing/revocation; only JWT issuance/login.
  - Evidence: `Cd.Cms.Api/Controllers/AuthController.cs:15`.

### 2) Dashboard and Analytics
- **Required dashboard KPI set is incomplete**.
  - SRS asks for KPI cards including SLA Compliance % and Avg Resolution Time.
  - Dashboard DTO exposes a different KPI set and does not include those fields.
  - Evidence: `Cd.Cms.Application/DTOs/Reports/DashboardDto.cs:5`.

- **Dashboard period toggle support (Last 30 Days / Quarter / Year) is missing in API contract**.
  - `GET /reports/dashboard` has no period/date query input.
  - Evidence: `Cd.Cms.Api/Controllers/ReportsController.cs:18`.

- **High-priority/overdue case table endpoint is missing**.
  - SRS calls for a focused table with overdue highlighting.
  - Reports API exposes only `dashboard` and `complaints-summary`.
  - Evidence: `Cd.Cms.Api/Controllers/ReportsController.cs:18`, `Cd.Cms.Api/Controllers/ReportsController.cs:25`.

- **Report export API (PDF) is missing**.
  - SRS explicitly requires Export PDF.
  - No report export route exists.
  - Evidence: `Cd.Cms.Api/Controllers/ReportsController.cs:18`, `Cd.Cms.Api/Controllers/ReportsController.cs:25`.

- **Department filter for reports is missing**.
  - SRS requires filter by department.
  - Current report filter supports only `From`, `To`, `AgentUserId`, `CategoryId`.
  - Evidence: `Cd.Cms.Application/DTOs/Reports/ReportFilterRequest.cs:5`.

### 3) Complaint Master List and Creation
- **Team-based filtering is missing**.
  - SRS requires filter by Team.
  - Complaint search request has no team/dept parameter.
  - Evidence: `Cd.Cms.Application/DTOs/Complaints/ComplaintSearchRequest.cs:3`.

- **Complaint creation field validation is incomplete vs SRS**.
  - SRS specifies additional rules (e.g., description min length, email format, field max lengths, source enum validation, search minimum length).
  - Service currently validates only required `Subject`, `Description`, and `Priority`.
  - Evidence: `Cd.Cms.Application/Services/ComplaintService.cs:19`.

### 4) Complaint Workspace / Timeline
- **Resolve precondition is missing**.
  - SRS: cannot mark complaint resolved without at least one response.
  - API only checks `ResolutionSummary` presence.
  - Evidence: `Cd.Cms.Application/Services/ComplaintService.cs:46`.

- **Escalation auto-routing logic is missing**.
  - SRS expects escalation to auto-assign to Tier 2 manager.
  - API requires caller-supplied `EscalatedToUserId`.
  - Evidence: `Cd.Cms.Application/Services/ComplaintService.cs:40`, `SQL/Complaints/CMS_Complaint_Escalate.sql:4`.

- **Public/private timeline separation and client-notification hooks are not represented in API model**.
  - SRS requires timeline sections: public replies, private notes, system events.
  - Case activity model has only generic `ActivityType` + `Description` and no visibility/notification attributes.
  - Evidence: `Cd.Cms.Application/DTOs/Cases/AddCaseActivityRequest.cs:5`, `Cd.Cms.Api/Controllers/CasesController.cs:31`.

- **SLA countdown/overdue transition API support is missing**.
  - SRS requires dynamic SLA timer and overdue behavior when timer reaches zero.
  - Current API surface has no timer endpoint/operation.
  - Evidence: `Cd.Cms.Api/Controllers/CasesController.cs:10`, `Cd.Cms.Api/Controllers/ComplaintsController.cs:10`.

### 5) Client Support Portal
- **Client complaint submission API is missing**.
  - SRS requires client self-service complaint filing.
  - Complaint create endpoint is restricted to `Admin,Supervisor,Agent` (not `Client`).
  - Evidence: `Cd.Cms.Api/Controllers/ComplaintsController.cs:39`.

- **"My Complaints" endpoint for clients is missing**.
  - SRS requires clients to view their own complaint list.
  - No dedicated route exists for client-scoped complaint listing.
  - Evidence: `Cd.Cms.Api/Controllers/ComplaintsController.cs:10`.

- **Client reply endpoint is missing**.
  - SRS includes client-side reply action.
  - Activity add endpoint is restricted to `Admin,Supervisor,Agent`.
  - Evidence: `Cd.Cms.Api/Controllers/CasesController.cs:32`.

### 6) User Management
- **Department and Reporting Manager fields are missing in user API DTOs**.
  - SRS requires them for user creation/management.
  - Current user DTOs expose only name/email/username/phone/password/role.
  - Evidence: `Cd.Cms.Application/DTOs/Users/CreateUserRequest.cs:3`, `Cd.Cms.Application/DTOs/Users/UpdateUserRequest.cs:3`.

- **Cannot-delete-own-account rule is not enforced in API/service**.
  - SRS explicitly requires this guard.
  - Current delete flow calls repository directly; SQL deactivates requested id with no self-check.
  - Evidence: `Cd.Cms.Api/Controllers/UsersController.cs:80`, `Cd.Cms.Application/Services/UserService.cs:35`, `SQL/Users/CMS_User_Delete.sql:4`.

- **User export (CSV) API is missing**.
  - SRS includes export in User Management.
  - No export endpoint exists in `UsersController`.
  - Evidence: `Cd.Cms.Api/Controllers/UsersController.cs:11`.

### 7) Teams, Roles/Permissions, Account Settings
- **Teams management APIs are missing**.
  - SRS includes create/update/archive team functions.
  - No Team controller/table/procedures found in project API layer.
  - Evidence: controller list has no Teams controller: `Cd.Cms.Api/Controllers`.

- **Roles and permissions matrix APIs are missing**.
  - SRS includes configurable permission matrix and duplicate-role / permission audit views.
  - No Roles/Permissions controller or schema artifacts found.
  - Evidence: controller list has no Roles/Permissions controller: `Cd.Cms.Api/Controllers`.

- **2FA and device session management APIs are missing**.
  - SRS includes QR setup, OTP activation, session listing, session revocation.
  - No 2FA/session-management routes in auth/users controllers.
  - Evidence: `Cd.Cms.Api/Controllers/AuthController.cs:9`, `Cd.Cms.Api/Controllers/UsersController.cs:11`.

- **Self-service deactivate account endpoint is missing**.
  - SRS includes account deactivation in account settings.
  - Current user deactivation is admin path via delete/deactivate by id.
  - Evidence: `Cd.Cms.Api/Controllers/UsersController.cs:78`, `SQL/Users/CMS_User_Delete.sql:4`.

### 8) Cross-Cutting Non-Functional Requirements
- **Audit logging for all changes is not implemented**.
  - `AuditLogs` table exists, but no writes found in API/SPs.
  - Evidence: `SQL/Setup/00_CreateTables.sql:194` (table), and no `INSERT INTO AuditLogs` in SQL/procedures.

- **Export features required to respect active filters are not implementable yet**.
  - SRS requires filtered exports across dashboard/list/report contexts.
  - API currently lacks export endpoints in reports, complaints, and users.
  - Evidence: `Cd.Cms.Api/Controllers/ReportsController.cs:11`, `Cd.Cms.Api/Controllers/ComplaintsController.cs:10`, `Cd.Cms.Api/Controllers/UsersController.cs:11`.

## Notes
- Attachments up to 10MB are implemented and aligned with SRS.
  - Evidence: `Cd.Cms.Application/Services/AttachmentService.cs:28`.
- This report focuses on **missing** requirement points only.

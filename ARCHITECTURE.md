# Cd.Cms — Complaint Management System API

## Solution Structure (6 projects)
- Cd.Cms.Api              -> Controllers, DI extensions, Program.cs
- Cd.Cms.Application      -> Services, Interfaces, DTOs, Validators
- Cd.Cms.Infrastructure   -> Repositories (ADO.NET + Stored Procedures only)
- Cd.Cms.Domain           -> Domain entities
- Cd.Cms.Shared           -> ApiResponse<T>, DataReader, JwtSettings, ResponseCodes
- Cd.Cms.Tests            -> xUnit unit tests (Moq for all dependencies)

## Tech Stack
- .NET 8, ASP.NET Core Web API
- ADO.NET with Microsoft.Data.SqlClient - NO Entity Framework ever
- ALL database operations via Stored Procedures only
- JWT Bearer Authentication
- xUnit + Moq for unit tests

## Architecture Rules
- ALWAYS use Stored Procedures. Never write inline SQL strings.
- Repository interfaces live in: Application/Contracts/Repositories/
- Service interfaces live in:    Application/Contracts/Services/
- NEVER expose domain entities directly - always return DTOs
- Use DataReader helper (Cd.Cms.Shared/DataReader.cs) for all SqlDataReader reads
- Use ApiResponse<T> (Cd.Cms.Shared/Responses/ApiResponse.cs) for ALL controller responses
- All timestamps stored as UTC using DateTime.UtcNow
- GetActorUserId() reads from JWT claim "uid" - never hardcode

## SP Naming Convention
Pattern: CMS_[Entity]_[Action]
Examples: CMS_Complaint_Create, CMS_Client_GetById, CMS_Case_AddActivity

## Connection Factory
- IDbFactory -> DbFactory reads "DefaultConnection" from appsettings.json
- Always: using var conn = (SqlConnection)_dbFactory.CreateConnection();

## JWT Claims (set in AuthService)
- "uid" -> user.Id (used by GetActorUserId in all controllers)
- ClaimTypes.Role -> user.Role (Admin | Supervisor | Agent | Client)

## Reading JWT in Controllers
private long GetActorUserId() => long.Parse(User.FindFirst("uid")?.Value ?? "0");

## Roles: Admin | Supervisor | Agent | Client
## Complaint Status: New | Assigned | InProgress | Pending | Escalated | Resolved | Closed | Rejected
## Priority: Low | Medium | High | Critical
## SLA Status: WithinSLA | AtRisk | Breached

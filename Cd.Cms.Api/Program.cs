using Cd.Cms.Api.Auditing;
using Cd.Cms.Api.DependencyInjection;
using Cd.Cms.Application.Common.Auth;
using Cd.Cms.Application.Contracts.Repositories;
using Cd.Cms.Application.Contracts.Services;
using Cd.Cms.Application.Services;
using Cd.Cms.Infrastructure.Contracts;
using Cd.Cms.Infrastructure.Persistence;
using Cd.Cms.Infrastructure.Repositories.Attachments;
using Cd.Cms.Infrastructure.Repositories.Cases;
using Cd.Cms.Infrastructure.Repositories.Categories;
using Cd.Cms.Infrastructure.Repositories.Clients;
using Cd.Cms.Infrastructure.Repositories.Complaints;
using Cd.Cms.Infrastructure.Repositories.Reports;
using Cd.Cms.Infrastructure.Repositories.SLA;
using Cd.Cms.Infrastructure.Repositories.Users;

var builder = WebApplication.CreateBuilder(args);

// ── Controllers & Swagger ─────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithJwt();

// ── CORS (adjust origins for production) ─────────────────────────────────────
builder.Services.AddCors(o => o.AddPolicy("CmsPolicy", p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// ── JWT ───────────────────────────────────────────────────────────────────────
builder.Services.AddJwtAuth(builder.Configuration);
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IDbFactory, DbFactory>();

// ── Repositories ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IUserRepository,       UserRepository>();
builder.Services.AddScoped<IComplaintRepository,  ComplaintRepository>();
builder.Services.AddScoped<IClientRepository,     ClientRepository>();
builder.Services.AddScoped<ICaseRepository,       CaseRepository>();
builder.Services.AddScoped<ICategoryRepository,   CategoryRepository>();
builder.Services.AddScoped<ISlaRepository,        SlaRepository>();
builder.Services.AddScoped<IReportRepository,     ReportRepository>();
builder.Services.AddScoped<IAttachmentRepository, AttachmentRepository>();
builder.Services.AddScoped<IAuditWriter, AuditWriter>();

// ── Services ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService,       AuthService>();
builder.Services.AddScoped<IUserService,       UserService>();
builder.Services.AddScoped<IComplaintService,  ComplaintService>();
builder.Services.AddScoped<IClientService,     ClientService>();
builder.Services.AddScoped<ICaseService,       CaseService>();
builder.Services.AddScoped<ICategoryService,   CategoryService>();
builder.Services.AddScoped<ISlaService,        SlaService>();
builder.Services.AddScoped<IReportService,     ReportService>();
builder.Services.AddScoped<IAttachmentService, AttachmentService>();

// ── File upload size limit (10 MB) ────────────────────────────────────────────
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o =>
    o.MultipartBodyLengthLimit = 10 * 1024 * 1024);

var app = builder.Build();

// ── Middleware Pipeline ───────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CMS API v1"));
}

app.UseHttpsRedirection();
app.UseCors("CmsPolicy");

// ✅ CRITICAL: Authentication BEFORE Authorization
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AuditLoggingMiddleware>();

app.MapControllers();
app.Run();

using Talapker.Application.AI.Talapker;
using Talapker.Application.AI.TranslationAgent;
using Talapker.Application.UserAccess.Queries.GetUserByIdQuery;
using Talapker.Infrastructure.Auth;
using Talapker.Infrastructure.AuthZ;
using Talapker.Infrastructure.Data.Seeding;
using Talapker.Infrastructure.Data.UserAccess;
using Talapker.Infrastructure.Email;
using Talapker.Infrastructure.Exceptions;
using Talapker.Infrastructure.S3;
using Talapker.Infrastructure.Wolverine;
using Talapker.Notifications;
using Talapker.Notifications.Features.Commands;
using Talapker.UserAccess.Infrastructure.Logging;
using Talapker.Web.AppExtensions;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddCommandLine(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ILanguageContext, LanguageContext>();

builder.Services.ConfigureCors();
builder.Services.AddSignalR();

builder.Services.AddLocalizedRazor();

builder.Host
    .AddAndConfigureWolverine(builder.Configuration, [typeof(GetUserByIdQueryHandler).Assembly, typeof(SendPushCommand).Assembly]);

builder.Services.AddMemoryCache();

builder.Services
    .AddTalapkerAgent()
    .AddTranslationAgent()
    .AddS3Storage(builder.Configuration)
    .AddTalapkerDbContext(builder.Configuration)
    .AddDataSeeding()
    .AddEmailSender(builder.Configuration)
    .AddAndConfigureSerilog(builder.Configuration);


builder.Services
    .AddAspIdentity(builder.Configuration)
    .AddIdentityServer(builder.Configuration)
    .AddAuthZ()
    .AddAuthenticationAndAuthorization(builder.Configuration);

builder.Services.AddRazorPages();

builder.Services.AddNotificationModule(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddSwaggerGen();

builder.Services.AddExceptionHandler<CustomExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler("/error");

app.UseRequestLocalization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
// app.UseHttpsRedirection();

app.UseRouting();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<TalapkerHub>("/talapkerHub");
app.MapNotificationModuleRoutes();
app.MapControllers();
app.MapRazorPages();

app.Run();
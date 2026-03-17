using Talapker.Application.AI.Talapker;
using Talapker.Application.AI.TranslationAgent;
using Talapker.Application.ChatFeatures;
using Talapker.Application.UserAccess.Queries.GetUserByIdQuery;
using Talapker.Infrastructure.Auth;
using Talapker.Infrastructure.AuthZ;
using Talapker.Infrastructure.Data.Seeding;
using Talapker.Infrastructure.Data.UserAccess;
using Talapker.Infrastructure.Email;
using Talapker.Infrastructure.Exceptions;
using Talapker.Infrastructure.S3;
using Talapker.Infrastructure.Vault;
using Talapker.Infrastructure.Wolverine;
using Talapker.Notifications;
using Talapker.Notifications.Features.Commands;
using Talapker.TelegramBot;
using Talapker.Web.AppExtensions;
using Talapker.Web.Logging;
using Talapker.Web.Middlewares;
using Extensions = Talapker.Notifications.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddCommandLine(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ILanguageContext, LanguageContext>();

builder.Services.ConfigureCors();
builder.Services.AddSignalR();

builder.Services.AddLocalizedRazor();

builder.Host
    .AddAndConfigureWolverine(builder.Configuration, [typeof(GetUserByIdQueryHandler).Assembly, typeof(SendPushHandler).Assembly, typeof(TelegramWebhookController).Assembly]);

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
    .AddVaultStore(builder.Configuration)
    .AddAspIdentity(builder.Configuration)
    .AddIdentityServer(builder.Configuration)
    .AddAuthZ()
    .AddAuthenticationAndAuthorization(builder.Configuration);

builder.Services.AddRazorPages();

builder.Services
    .AddNotificationModule(builder.Configuration)
    .AddTelegramBotModule(builder.Configuration);

builder.Services.AddControllers()
    .AddApplicationPart(typeof(TelegramWebhookController).Assembly)
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
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
app.UseMiddleware<SecurityStampMiddleware>();

app.MapHub<TalapkerHub>("/talapkerHub");
app.MapHub<ChatHub>("/hubs/chat");
Extensions.MapNotificationModuleRoutes(app);
app.MapControllers();
app.MapRazorPages();

app.Run();
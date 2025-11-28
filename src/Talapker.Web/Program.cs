using Talapker.Application.UserAccess.Queries.GetUserByIdQuery;
using Talapker.Infrastructure.AI.TranslationAgent;
using Talapker.Infrastructure.Auth;
using Talapker.Infrastructure.AuthZ;
using Talapker.Infrastructure.Data.UserAccess;
using Talapker.Infrastructure.Email;
using Talapker.Infrastructure.Exceptions;
using Talapker.Infrastructure.S3;
using Talapker.Infrastructure.Wolverine;
using Talapker.UserAccess.Infrastructure.Logging;
using Talapker.Web.AppExtensions;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddCommandLine(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ILanguageContext, LanguageContext>();

builder.Services.ConfigureCors();

builder.Services.AddLocalizedRazor();

builder.Host
    .AddAndConfigureWolverine(builder.Configuration, typeof(GetUserByIdQueryHandler).Assembly);

builder.Services
    .AddTranslationAgent()
    .AddS3Storage(builder.Configuration)
    .AddTalapkerDbContext(builder.Configuration)
    .AddEmailSender(builder.Configuration)
    .AddAndConfigureSerilog(builder.Configuration);


builder.Services
    .AddAspIdentity(builder.Configuration)
    .AddIdentityServer(builder.Configuration)
    .AddAuthZ()
    .AddAuthenticationAndAuthorization(builder.Configuration);

builder.Services.AddRazorPages();
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

app.MapControllers();
app.MapRazorPages();

app.Run();
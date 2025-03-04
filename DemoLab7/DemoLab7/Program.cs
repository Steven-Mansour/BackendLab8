using Azure.Storage.Blobs;
using DemoLab7.Auth;
using DemoLab7.Models;
using DemoLab7.RabbitMQ;
using DemoLab7.Tenancy;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<CourseEventPublisher>();
builder.Services.AddScoped<EnrollmentEventPublisher>();
builder.Services.AddSingleton<ITenantProvider, TenantProvider>();
builder.Services.AddScoped<IClaimsTransformation, KeycloakRoleClaimsTransformation>();
builder.Services.AddSingleton<UniversityDbContext, UniversityDbContext>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "http://localhost:8080/realms/myrealm";
        options.RequireHttpsMetadata = false;
        options.Audience = "account"; 
        
    });
builder.Services.AddAuthorization();

var connectionString = builder.Configuration["AzureStorage:ConnectionString"];
var containerName = builder.Configuration["AzureStorage:ContainerName"]; 
builder.Services.AddSingleton(new BlobServiceClient(connectionString));
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<BlobServiceClient>().GetBlobContainerClient(containerName));


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri("http://localhost:8080/realms/myrealm/protocol/openid-connect/auth"),
                TokenUrl = new Uri("http://localhost:8080/realms/myrealm/protocol/openid-connect/token"),
                
            }
        }
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
            },
            new[] { "api.read", "api.write" }
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.OAuthAppName("Swagger UI");
        c.OAuthClientId("myclient"); 
    });
}
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.UseDirectoryBrowser(new DirectoryBrowserOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets")),
    RequestPath = "/assets"
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
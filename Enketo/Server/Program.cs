using Enketo.Server.Data.Context;
using Enketo.Server.Data.Settings;
using Enketo.Shared;
using Enketo.Shared.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAdB2C"));

// Configure MongoDb
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDb"));

builder.Services.AddSingleton<IMongoDbContext, MongoDbContext>();

// Configure Application Core
builder.Services.ConfigureApplication(builder.Configuration);

// ASP.NEt Core stuff
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Configure Swagger with AzureAD B2C Authorization
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(
        "v1",
        new OpenApiInfo { Title = "Enketo API" });
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme()
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows()
        {
            Implicit = new OpenApiOAuthFlow()
            {
                AuthorizationUrl = new Uri("https://enketo.b2clogin.com/enketo.onmicrosoft.com/b2c_1_susi/oauth2/v2.0/authorize"),
                TokenUrl = new Uri("https://enketo.b2clogin.com/enketo.onmicrosoft.com/b2c_1_susi/oauth2/v2.0/token"),
                Scopes = new Dictionary<string, string>
                {
                    { "openid", "Sign In Permissions" },
                    {"https://enketo.onmicrosoft.com/691f8ddf-0b78-4835-a311-bf93a09c9d3a/API.Access", "API Access" }
                }
            }
        }
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement() {
    {
        new OpenApiSecurityScheme {
            Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                        Id = "oauth2"
                },
                Scheme = "oauth2",
                Name = "oauth2",
                In = ParameterLocation.Header
        },
        new List < string > ()
    }
});
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blazor API V1");
    c.OAuthClientId("4f45b6b8-565f-4221-8a61-2e785ad91358");
    c.OAuthUseBasicAuthenticationWithAccessCodeGrant();
});

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

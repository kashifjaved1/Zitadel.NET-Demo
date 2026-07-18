using Microsoft.AspNetCore.Authentication.JwtBearer;
using Zitadel.NET.Demo.Extensions;

var builder = WebApplication.CreateBuilder(args);

var authSettings = builder.Configuration.GetSection("Authentication");

var authority = authSettings["Authority"];
var clientId = authSettings["ClientId"];

// Add services
builder.Services.AddControllers();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;

        // Local development only
        options.RequireHttpsMetadata =
            bool.Parse(authSettings["RequireHttpsMetadata"] ?? "true");

        options.TokenValidationParameters.ValidateAudience = false;

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine(
                    "JWT ERROR: " + context.Exception.Message
                );

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();


// Swagger
builder.Services.AddEndpointsApiExplorer();

builder.Services.ConfigureSwagger(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        c.OAuthClientId(clientId);
        c.OAuthUsePkce();
        c.OAuthScopes(
            builder.Configuration["SwaggerOAuth:Scopes"]
            ?? "openid profile email"
        );
    });
}


app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
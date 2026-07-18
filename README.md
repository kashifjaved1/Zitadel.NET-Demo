# ZITADEL Authentication with .NET 9 Web API + Swagger

A beginner-friendly learning project showing how to integrate **ZITADEL Identity Management** with a **.NET 9 Web API** using:

- OAuth 2.0 / OpenID Connect
- JWT Bearer Authentication
- Swagger OAuth2 Login
- Authorization Code Flow with PKCE

The goal of this project is to understand how a modern API authenticates users using an external Identity Provider.

---

# What We Are Building

The final flow looks like this:

```
User
 |
 | Login
 |
 v
ZITADEL Identity Provider
 |
 | Access Token (JWT)
 |
 v
.NET 9 Web API
 |
 | Validate JWT
 |
 v
Protected API Endpoint
```

Swagger will also be able to login through ZITADEL and automatically attach the JWT token to API requests.

---

# Prerequisites

Before starting, install:

## Required

- .NET 9 SDK

Check:

```bash
dotnet --version
```

- ZITADEL instance running locally

Example:

```
http://localhost:8080
```

- Visual Studio 2022 / VS Code / Rider

- Git

---

# 1. Create ZITADEL Project

Open ZITADEL Management Console.

Example:

```
http://localhost:8080
```

Login as administrator.

---

## Create a Project

Navigate:

```
Projects
    |
    +-- New Project
```

Example:

```
Project Name:
ZitadelDotNetDemo
```

Create the project.

---

# 2. Create Application

Inside your project:

```
Applications
      |
      +-- Create Application
```

Choose:

```
Application Type:
Web
```

Reason:

We want Swagger to login using the browser.

---

# 3. Configure Redirect URI

Go to:

```
Application
    |
    +-- Redirect Settings
```

Add:

```
https://localhost:7160/swagger/oauth2-redirect.html
```

Important:

The URL must exactly match your Swagger URL.

Example:

Swagger:

```
https://localhost:7160/swagger/index.html
```

OAuth callback:

```
https://localhost:7160/swagger/oauth2-redirect.html
```

They are different.

---

# 4. Development Mode

For local development enable:

```
Development Mode = ON
```

This allows local HTTP URLs if needed.

For production always use HTTPS.

---

# 5. Copy Client ID

From the application settings copy:

```
Client ID
```

Example:

```
000001111333345
```

You will use this in your .NET configuration.

No client secret is required because we use:

```
Authorization Code Flow + PKCE
```

---

# 6. Create .NET API

Create a Web API:

```bash
dotnet new webapi -n Zitadel.NET.Demo
```

Move into project:

```bash
cd Zitadel.NET.Demo
```

---

# 7. Install NuGet Packages

Install authentication:

```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

Swagger:

```bash
dotnet add package Swashbuckle.AspNetCore
```

OpenID support:

```bash
dotnet add package Microsoft.IdentityModel.Protocols.OpenIdConnect
```

---

# 8. Configure appsettings.json

Add ZITADEL configuration:

```json
{
  "Authentication": {
    "Authority": "http://localhost:8080",
    "ClientId": "YOUR_CLIENT_ID",
    "Audience": "YOUR_CLIENT_ID",
    "RequireHttpsMetadata": false
  },

  "SwaggerOAuth": {
    "AuthorizationUrl": "http://localhost:8080/oauth/v2/authorize",
    "TokenUrl": "http://localhost:8080/oauth/v2/token",
    "Scopes": "openid profile email"
  }
}
```

Replace:

```
YOUR_CLIENT_ID
```

with the Client ID from ZITADEL.

---

# 9. Configure JWT Authentication

In `Program.cs`:

```csharp
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority =
            builder.Configuration["Authentication:Authority"];

        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters
            .ValidateAudience = false;
    });
```

This tells .NET:

"Trust tokens issued by this ZITADEL server."

---

# 10. Enable Authentication Middleware

The order matters:

```csharp
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
```

Authentication must run before Authorization.

---

# 11. Configure Swagger OAuth Login

Swagger needs to know:

- Where the login page is
- Where to get tokens
- Which client ID to use

Configuration:

```csharp
app.UseSwaggerUI(c =>
{
    c.OAuthClientId(
        builder.Configuration["Authentication:ClientId"]
    );

    c.OAuthUsePkce();

    c.OAuthScopes(
        "openid profile email"
    );
});
```

---

# 12. Protect an API Endpoint

Example controller:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{

    [HttpGet("public")]
    public IActionResult Public()
    {
        return Ok("Anyone can access");
    }


    [Authorize]
    [HttpGet("private")]
    public IActionResult Private()
    {
        return Ok("You are authenticated");
    }
}
```

---

# 13. Run the API

Start:

```bash
dotnet run
```

Example:

```
https://localhost:7160
```

Open Swagger:

```
https://localhost:7160/swagger
```

---

# 14. Login Through Swagger

Click:

```
Authorize
```

Swagger redirects to:

```
ZITADEL Login Page
```

Login using your ZITADEL user.

After successful login:

ZITADEL returns:

```
Access Token (JWT)
```

Swagger automatically sends:

```
Authorization: Bearer <token>
```

---

# 15. Test Endpoints

Public:

```
GET /api/test/public
```

Response:

```json
{
  "message": "Anyone can access"
}
```

---

Protected:

```
GET /api/test/private
```

Without login:

```
401 Unauthorized
```

After ZITADEL login:

```
200 OK
```

Response:

```
You are authenticated
```

---

# Understanding the Important URLs

From ZITADEL OpenID configuration:

## Issuer

```
http://localhost:8080
```

Used by .NET to validate tokens.

---

## Authorization Endpoint

```
http://localhost:8080/oauth/v2/authorize
```

Where users login.

---

## Token Endpoint

```
http://localhost:8080/oauth/v2/token
```

Where authorization codes become access tokens.

---

## JWKS Endpoint

```
http://localhost:8080/oauth/v2/keys
```

Contains public keys used to validate JWT signatures.

---

# Project Structure

```
Zitadel.NET.Demo

│
├── Controllers
│      └── TestController.cs
│
├── Extensions
│      └── ServiceExtensions.cs
│
├── Program.cs
│
├── appsettings.json
│
└── README.md
```

---

# Common Problems

## 401 Unauthorized

Check:

- Is the token expired?
- Is the issuer correct?
- Is authentication middleware enabled?

Required:

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

---

## Redirect URI Error

Example:

```
redirect_uri missing in client configuration
```

Fix:

The URL in ZITADEL must exactly match:

```
https://localhost:7160/swagger/oauth2-redirect.html
```

---

## Swagger Authorize Button Does Nothing

Check:

- OAuth client ID
- Redirect URI
- PKCE enabled

---

# Technologies Used

- .NET 9
- ASP.NET Core Web API
- ZITADEL
- OAuth 2.0
- OpenID Connect
- JWT
- Swagger UI

---

# Important: Access Token Type Configuration

For this API integration, ZITADEL must issue a **signed JWT access token**.

The .NET API uses JWT Bearer authentication, which means it validates the access token directly using ZITADEL's public keys.

Make sure your ZITADEL project/application token settings are configured to use:

```
Access Token Type:
JWT
```

and **not**:

```
Encrypted JWT
```

---

## Why does this matter?

OAuth login returns multiple tokens:

```
ZITADEL
   |
   +-- id_token
   |
   +-- access_token
```

### ID Token

The `id_token` is used by the client application.

Example:

```
eyJhbGciOiJSUzI1Ni...
```

It contains user identity information and can be decoded as a normal JWT.

It is **not meant for calling APIs**.

---

### Access Token

The `access_token` is sent to your API:

```
Authorization: Bearer <access_token>
```

Your .NET API validates this token.

A correct JWT access token should look like:

```
xxxxx.yyyyy.zzzzz
```

with three parts separated by dots.

---

## Encrypted Access Token Problem

If ZITADEL is configured with:

```
Access Token Type:
Encrypted/Bearer JWT
```

the token will look like:

```
xxxxx.xxxxx.xxxxx.xxxxx.xxxxx
```

This is a JWE encrypted token.

ASP.NET Core cannot validate it without the decryption keys and will throw an error similar to:

```
IDX10609: Decryption failed. No Keys tried
```

---

## Troubleshooting

If you see:

```
401 Unauthorized

www-authenticate: Bearer error="invalid_token"
```

and the API logs:

```
IDX10609: Decryption failed
```

check the ZITADEL access token configuration.

After changing the token type:

1. Logout from Swagger.
2. Clear browser storage if required.
3. Login again through Swagger.
4. Verify that the new `access_token` is a signed JWT.

---

## Recommended OAuth Flow

This project uses:

```
Authorization Code Flow + PKCE
```

with:

```
Grant Type:
authorization_code
```

This is the recommended flow for Swagger and web applications.

No client secret is required because PKCE protects the authorization flow.
using CodingCleanProject.Data;
using CodingCleanProject.Interfaces;
using CodingCleanProject.Repository;
using Microsoft.EntityFrameworkCore;
using CodingCleanProject.Helpers;
using CodingCleanProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using CodingCleanProject.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddDatabaseService(builder.Configuration);


builder.Services.AddRepositories();

builder.Services.AddTransient<QueryObject>();

builder.Services.AddTokenService();

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireDigit = true;
}).AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddAuthenticationService(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter JWT token"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});


builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();

//app.Use(async (context, next) =>
//{
//    if (context.Request.Path == "/api/auth/login")
//    {
//        var token = "GENERATED-JWT-TOKEN";
//        context.Response.Cookies.Append("AuthToken", token, new CookieOptions
//        {
//            HttpOnly = true,
//            Secure = true, // mora da ide preko https 
//            SameSite = SameSiteMode.Strict, // osgurava da se izbjegne Cross-site request forgery
//            Expires = DateTimeOffset.UtcNow.AddMinutes(30)
//        });
//    }
//    await next();
//});
app.UseAuthorization();

app.MapControllers();

app.Run();

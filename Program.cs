using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WorldCupPolling.Data; // Your AppDbContext namespace
using WorldCupPolling.Services; // Your JwtService namespace

var builder = WebApplication.CreateBuilder(args);

// 1. Configure CORS (Allows Angular frontend to make requests to this API)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200") // Default Angular development port
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// 2. Configure Entity Framework with MySQL (Pomelo)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 3. Register custom application services
builder.Services.AddScoped<JwtService>();

// 4. Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

// 5. Add Controllers support (Replacing Minimal APIs)
builder.Services.AddControllers();

// Swagger for testing APIs
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 6. Apply the CORS policy BEFORE Authentication and Authorization
app.UseCors("AllowAngularApp");

// 7. Core Middleware Security Sequence (Authentication MUST come before Authorization)
app.UseAuthentication();
app.UseAuthorization();

// 8. Map the controllers
app.MapControllers();

app.Run();
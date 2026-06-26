using AutoMapper;
using JwtMusic.WebApi.Context;
using JwtMusic.WebApi.Entities;
using JwtMusic.WebApi.Services.ArtistFollowService;
using JwtMusic.WebApi.Services.ArtistServices;
using JwtMusic.WebApi.Services.LoginServices;
using JwtMusic.WebApi.Services.RegisterServices;
using JwtMusic.WebApi.Services.SongServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── CORS SERVİS TANIMLAMASI ──
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("https://localhost:7055") // UI Projesinin adresi
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddIdentity<AppUser, IdentityRole>().AddEntityFrameworkStores<JwtContext>().AddDefaultTokenProviders();

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
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"])),

        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddDbContext<JwtContext>();
builder.Services.AddScoped<IRegisterService, RegisterService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IArtistService, ArtistService>();
builder.Services.AddScoped<ISongService, SongService>();
builder.Services.AddScoped<IArtistFollowService, ArtistFollowService>();
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "JwtMusic.WebApi", Version = "v1" });
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Lütfen token değerini girin. Örnek: Bearer eyJhbGci...",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[]{}
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ── MIDDLEWARE SIRALAMASI (ÇOK KRİTİK) ──
// UseCors, Authentication ve Authorization işlemlerinden ÖNCE çağrılmalıdır.
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
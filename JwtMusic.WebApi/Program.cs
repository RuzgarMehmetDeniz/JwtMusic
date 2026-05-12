using AutoMapper;
using JwtMusic.WebApi.Context;
using JwtMusic.WebApi.Entities;
using JwtMusic.WebApi.Services.ArtistServices;
using JwtMusic.WebApi.Services.LoginServices;
using JwtMusic.WebApi.Services.RegisterServices;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentity<AppUser , IdentityRole>().AddEntityFrameworkStores<JwtContext>().AddDefaultTokenProviders();

builder.Services.AddDbContext<JwtContext>();
builder.Services.AddScoped<IRegisterService, RegisterService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IArtistService, ArtistService>();
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
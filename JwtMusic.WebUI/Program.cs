using JwtMusic.WebApi.Context;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<JwtContext>();

builder.Services.AddHttpClient();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // 500 Sunucu Hatası aldığında senin yazdığın ServerError action'ına gider
    app.UseExceptionHandler("/Error/ServerError");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// 401, 403 ve 404 durum kodlarını senin yazdığın spesifik action url'lerine eşliyoruz
app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;

    if (response.StatusCode == 404)
    {
        context.HttpContext.Response.Redirect("/Error/NotFound");
    }
    else if (response.StatusCode == 401)
    {
        context.HttpContext.Response.Redirect("/Error/Unauthorized");
    }
    else if (response.StatusCode == 403)
    {
        context.HttpContext.Response.Redirect("/Error/AccessDenied");
    }
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
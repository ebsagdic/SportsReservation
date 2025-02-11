using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:7122"); // API'nizin base URL'sini buraya yazýn.
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    //HTTP istemcisi (HttpClient) aracýlýðýyla yapýlan isteðin, sunucuya "Ben JSON formatýnda bir yanýt bekliyorum" þeklinde bilgi vermesi için kullanýlýr. Bu iþlem, HTTP isteðinin Accept baþlýðýný ayarlamayý saðlar.
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
             .AddCookie(options =>
             {
                 options.Cookie.Name = "MySessionCookie";
                 options.LoginPath = new PathString("/Account/Login");
                 options.SlidingExpiration = true;
                 options.Cookie.HttpOnly = true;
                 options.AccessDeniedPath = new PathString("/Account/Forbidden");
                 options.ExpireTimeSpan = TimeSpan.FromMinutes(1);
                 options.Cookie.MaxAge = options.ExpireTimeSpan;
                 options.SlidingExpiration = true;
                 options.LogoutPath = new PathString("/Account/Logout");
                 options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
             });


var app = builder.Build();

var cookiePolicyOptions = new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Strict,
    HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always,
    Secure = CookieSecurePolicy.None,
};
app.UseCookiePolicy(cookiePolicyOptions);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

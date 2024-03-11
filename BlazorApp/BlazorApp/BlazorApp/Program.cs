using Auth0.AspNetCore.Authentication;
using BlazorApp.AuthenticationStateSyncer;
using BlazorApp.Client.Pages;
using BlazorApp.Components;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Auth0 configuration
            builder.Services
                .AddAuth0WebAppAuthentication(options => {
                    options.Domain = builder.Configuration["Auth0:Domain"];
                    options.ClientId = builder.Configuration["Auth0:ClientId"];
                });

            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents();

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

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            // Auth0 sign-in and sign-out
            app.MapGet("/Account/Login", async (HttpContext httpContext, string redirectUri = "/") =>
            {
                var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
                    .WithRedirectUri(redirectUri)
                    .Build();

                await httpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
            });

            app.MapGet("/Account/Logout", async (HttpContext httpContext, string redirectUri = "/") =>
            {
                var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
                    .WithRedirectUri(redirectUri)
                    .Build();

                await httpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
                await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            });
            // End of Auth0 sign-in and sign-out

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(Counter).Assembly);

            app.Run();
        }
    }
}

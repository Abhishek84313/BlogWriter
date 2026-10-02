using BlogWriter.Web.Components;
using BlogWriter.Web.Services;
using Azure.Core;
using Azure.Identity;
using BlogWriter;
using BlogWriter.Web;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsEnvironment("Testing"))
{
    builder.WebHost.UseStaticWebAssets();
}

bool useTestingAuthentication = builder.Environment.IsEnvironment("Testing") &&
    builder.Configuration.GetValue<bool>("Authentication:UseTestingIdentity");
if (useTestingAuthentication)
{
    builder.Services.AddAuthentication(TestingAuthenticationHandler.SchemeName)
        .AddScheme<AuthenticationSchemeOptions, TestingAuthenticationHandler>(
            TestingAuthenticationHandler.SchemeName,
            _ => { });
}
else
{
    builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));
    builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.UsePkce = true;
        options.SaveTokens = true;
    });
}
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddControllersWithViews().AddMicrosoftIdentityUI();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddScoped<ISessionOwnerProvider, ClaimsSessionOwnerProvider>();
builder.Services.AddScoped<BlogWorkspaceService>();

TokenCredential resourceCredential = CreateResourceCredential(builder.Configuration, builder.Environment);
builder.Services.AddBlogWriterApplication(builder.Configuration, resourceCredential);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

static TokenCredential CreateResourceCredential(IConfiguration configuration, IHostEnvironment environment)
{
    string mode = configuration["AzureResources:CredentialMode"] ??
        (environment.IsDevelopment() ? "AzureCli" : "ManagedIdentity");
    string? tenantId = configuration["AzureAd:TenantId"];

    return mode switch
    {
        "AzureCli" when environment.IsDevelopment() => new AzureCliCredential(
            new AzureCliCredentialOptions { TenantId = tenantId }),
        "ManagedIdentity" => new ManagedIdentityCredential(ManagedIdentityId.SystemAssigned),
        _ => throw new InvalidOperationException(
            $"Azure resource credential mode '{mode}' is not valid for environment '{environment.EnvironmentName}'."),
    };
}

public partial class Program;

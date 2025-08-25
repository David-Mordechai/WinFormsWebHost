using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.IO;
using System.Reflection;
using WinFormsVue.WinForms;

namespace WinFormsVue;

internal class Program
{
    [STAThread]
    private static void Main()
    {
        var builder = WebApplication.CreateBuilder();

        builder.Configuration.AddJsonFile("appsettings.json",
            optional: false, reloadOnChange: true);

        builder.Services.AddControllers();
        builder.Services.AddSignalR();
        builder.Services.AddSingleton<MainForm>();

        builder.Services.AddCors();

        builder.Services.AddSingleton<ThemeHelper>();
        var configSettings = builder.Configuration.GetRequiredSection(nameof(Settings)).Get<Settings>()!;
        builder.Services.AddSingleton(configSettings);

        var app = builder.Build();

        var uiAssembly = Assembly.GetExecutingAssembly();
        var uiProvider = new ManifestEmbeddedFileProvider(uiAssembly, Path.Combine("WinForms", "WebUi"));
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = uiProvider,
            ContentTypeProvider = new FileExtensionContentTypeProvider()
        });

        app.UseCors(policyBuilder => policyBuilder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

        app.MapControllers();

        app.MapGet("api/hello", () => "Hello from WinForms!");

        app.Use(async (context, next) =>
        {
            var path = context.Request.Path;

            if (path.StartsWithSegments("/ws") || path.StartsWithSegments("/api"))
            {
                await next();
                return;
            }

            var index = uiProvider.GetFileInfo("index.html");
            if (index.Exists)
            {
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.SendFileAsync(index);
            }
        });

        app.MapHub<AppHub>("/ws");

        var settings = app.Services.GetRequiredService<Settings>();

        var vueDebugPort = Environment.GetEnvironmentVariable("VUE_DEBUG_PORT");
        if (vueDebugPort is not null)
            settings.FrontedPort = int.Parse(vueDebugPort);

        _ = app.RunAsync($"http://localhost:{settings.BackendPort}");

        //ApplicationConfiguration.Initialize();
        var mainForm = app.Services.GetRequiredService<MainForm>();
        Application.Run(mainForm);
    }
}
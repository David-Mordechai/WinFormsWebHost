using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Hosting;

namespace WinFormsWebHost;

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
        
        builder.Services.AddSingleton<Settings>();
        
        var app = builder.Build();

        var uiAssembly = Assembly.GetCallingAssembly();
        var uiProvider = new ManifestEmbeddedFileProvider(uiAssembly, "AppUi");
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
                await using var stream = index.CreateReadStream();
                using var reader = new StreamReader(stream);
                var html = await reader.ReadToEndAsync();

                var themeClass = ThemeHelper.IsDarkMode() ? "dark" : "light";

                html = html.Replace("<body>", $"<body class=\"{themeClass}\">");

                await context.Response.WriteAsync(html);
            }
        });

        app.MapHub<AppHub>("/ws");
        
        var settings = app.Services.GetRequiredService<Settings>();
        if (app.Environment.IsDevelopment())
            settings.FrontedPort = 5173;
        
        _ = app.RunAsync($"http://localhost:{settings.BackendPort}");

        ApplicationConfiguration.Initialize();
        var mainForm = app.Services.GetRequiredService<MainForm>();
        Application.Run(mainForm);
    }
}
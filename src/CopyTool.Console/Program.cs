using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.IO.Abstractions;

namespace CopyTool.Console
{
    internal class Program
    {
        private const string _settingsFileName = "settings.json";

        static async Task Main(string[] args)
        {
            IHost host = CreateHostBuilder(args)
                .Build();
            
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
            
            Log.Information("FileCopy starts");

            var service = host.Services.GetService<ICopyOperation>();

            bool isSuccess = false;

            if (service is not null)
            {
                isSuccess = await service.FileCopy(@"C:\Users\marku\OneDrive\Desktop\roll20.png", @"C:\Users\marku\OneDrive\Desktop\roll21.png");
            }
            else
            {
                Log.Error("FileCopy could not be initialized. Check your settings file.");
            }

            var message = isSuccess ? "successfully" : "with errors";
            Log.Information("FileCopy finished {message}", message);

        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            var hostBuilder = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.SetBasePath(Directory.GetCurrentDirectory())
                          .AddJsonFile(_settingsFileName, optional: false, reloadOnChange: true);
                        //.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                        //.AddEnvironmentVariables();
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IFileSystem, FileSystem>();
                    services.AddSingleton<ISettingsReader, SettingsReader>();
                    services.AddTransient<ICopyOperation, CopyOperation>();
                });

            return hostBuilder;
        }
    }
}
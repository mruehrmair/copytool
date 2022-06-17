using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO.Abstractions;
using System;
using CopyTool;

namespace CopyTool.Console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            host.Services.GetService<ICopyOperation>().FileCopy(@"C:\Users\marku\OneDrive\Desktop\roll20.png", @"C:\Users\marku\OneDrive\Desktop\roll21.png");
            System.Console.WriteLine("Copy finished");
            System.Console.Read();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            var hostBuilder = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.SetBasePath(Directory.GetCurrentDirectory());
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
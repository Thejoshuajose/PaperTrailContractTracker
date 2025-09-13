using System.IO;
using PaperTrail.App.Services;
using PaperTrail.App.ViewModels;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PaperTrail.Core.Data;
using PaperTrail.Core.Repositories;
using PaperTrail.Core.Services;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using MongoDB.Driver;

namespace PaperTrail.App;

public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var appDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PaperTrailContractTracker");
        Directory.CreateDirectory(Path.Combine(appDir, "files"));

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<IMongoClient>(_ =>
                    new MongoClient(Environment.GetEnvironmentVariable("MONGODB_URI") ?? "mongodb://localhost:27017"));
                services.AddSingleton<MongoContext>();
                services.AddScoped<IContractRepository, ContractRepository>();
                services.AddScoped<IPartyRepository, PartyRepository>();
                services.AddSingleton<INotificationService, NotificationService>();
                services.AddSingleton<ImportService>();
                services.AddSingleton<ExportService>();
                services.AddSingleton<DialogService>();
                services.AddSingleton<ILicenseService, LicenseService>();
                services.AddSingleton<CsvExporter>();
                services.AddSingleton<HashService>();
                services.AddTransient<ReminderEngine>();
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<ContractListViewModel>();
                services.AddSingleton<ContractEditViewModel>();
                services.AddSingleton<PartyEditViewModel>();
                services.AddSingleton<SettingsService>();
                services.AddSingleton<IJobFactory, QuartzJobFactory>();
                services.AddSingleton(provider =>
                {
                    var scheduler = StdSchedulerFactory.GetDefaultScheduler().Result;
                    scheduler.JobFactory = provider.GetRequiredService<IJobFactory>();
                    return scheduler;
                });
            })
            .Build();

        using var scope = _host.Services.CreateScope();
        var scheduler = scope.ServiceProvider.GetRequiredService<IScheduler>();
        var job = JobBuilder.Create<ReminderEngine>().WithIdentity("reminderJob").Build();
        var trigger = TriggerBuilder.Create().StartNow().WithSimpleSchedule(s => s.WithInterval(TimeSpan.FromMinutes(15)).RepeatForever()).Build();
        await scheduler.ScheduleJob(job, trigger);
        await scheduler.Start();

        var settings = scope.ServiceProvider.GetRequiredService<SettingsService>();
        var mainVm = scope.ServiceProvider.GetRequiredService<MainViewModel>();
        await mainVm.Contracts.LoadAsync();
        var mainWindow = new MainWindow { DataContext = mainVm };
        mainWindow.Show();

        if (string.IsNullOrWhiteSpace(settings.CompanyName))
        {
            var settingsVm = new SettingsViewModel(settings);
            var settingsWindow = new SettingsWindow { DataContext = settingsVm, Owner = mainWindow };
            settingsWindow.ShowDialog();
        }
    }
}

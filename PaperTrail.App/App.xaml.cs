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
                    new MongoClient(Environment.GetEnvironmentVariable("MONGODB_URI") ?? "mongodb+srv://fiwbsolutions:lDQlujC1r9yV0uc4@fiwb-cluster.icshfk2.mongodb.net/"));
                services.AddSingleton<MongoContext>();
                // Use singleton lifetime for repositories so they can be injected into
                // singleton view models without lifetime conflicts
                services.AddSingleton<IContractRepository, ContractRepository>();
                services.AddSingleton<PreviousContractRepository>();
                services.AddSingleton<IPartyRepository, PartyRepository>();
                services.AddSingleton<INotificationService, NotificationService>();
                services.AddSingleton<ImportService>();
                services.AddSingleton<ExportService>();
                services.AddSingleton<DialogService>();
                services.AddSingleton<ILicenseService, LicenseService>();
                services.AddSingleton<CsvExporter>();
                services.AddSingleton<HashService>();
                services.AddTransient<ReminderEngine>();
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<LandingViewModel>();
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
        var landingVm = scope.ServiceProvider.GetRequiredService<LandingViewModel>();
        var landingWindow = new LandingWindow { DataContext = landingVm };
        landingWindow.Show();

        if (string.IsNullOrWhiteSpace(settings.CompanyName))
        {
            var settingsVm = new SettingsViewModel(settings);
            var settingsWindow = new SettingsWindow { DataContext = settingsVm, Owner = landingWindow };
            settingsWindow.ShowDialog();
        }
    }
}

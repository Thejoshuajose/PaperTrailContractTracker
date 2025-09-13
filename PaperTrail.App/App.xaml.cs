using System.IO;
using PaperTrail.App.Services;
using PaperTrail.App.ViewModels;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PaperTrail.Core.Data;
using PaperTrail.Core.Repositories;
using PaperTrail.Core.Services;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;

namespace PaperTrail.App;

public partial class App : Application
{
    private IHost? _host;
    public IServiceProvider Services => _host?.Services ?? throw new InvalidOperationException("Host not initialized");

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var appDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PaperTrailContractTracker");
        Directory.CreateDirectory(Path.Combine(appDir, "files"));

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("PaperTrail"));
                services.AddDbContextFactory<AppDbContext>(options => options.UseInMemoryDatabase("PaperTrail"));
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
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await DataSeeder.SeedAsync(db);

        var scheduler = scope.ServiceProvider.GetRequiredService<IScheduler>();
        var job = JobBuilder.Create<ReminderEngine>().WithIdentity("reminderJob").Build();
        var trigger = TriggerBuilder.Create().StartNow().WithSimpleSchedule(s => s.WithInterval(TimeSpan.FromMinutes(15)).RepeatForever()).Build();
        await scheduler.ScheduleJob(job, trigger);
        await scheduler.Start();

        var landingWindow = new LandingWindow();
        landingWindow.Show();
    }
}

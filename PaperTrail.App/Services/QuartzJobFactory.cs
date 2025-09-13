using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;

namespace PaperTrail.App.Services;

public class QuartzJobFactory : IJobFactory
{
    private readonly IServiceProvider _provider;
    public QuartzJobFactory(IServiceProvider provider) => _provider = provider;

    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        => (IJob)_provider.GetRequiredService(bundle.JobDetail.JobType);

    public void ReturnJob(IJob job) { }
}

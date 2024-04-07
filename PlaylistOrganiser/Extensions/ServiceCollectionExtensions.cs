using Quartz;

namespace PlaylistOrganiser.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddJobAndTrigger<T>(
        this IServiceCollectionQuartzConfigurator quartz,
        string jobName)
        where T : IJob
    {
        var jobKey = new JobKey(jobName);
        quartz.AddJob<T>(opts => opts.WithIdentity(jobKey));

        quartz.AddTrigger(opts => opts
            .ForJob(jobKey)
            .WithIdentity(jobName + "-trigger")
            .WithSchedule(SimpleScheduleBuilder.Create().WithInterval(TimeSpan.FromSeconds(15)).RepeatForever())); // todo: try get cron schedules in, from config??
    }
}
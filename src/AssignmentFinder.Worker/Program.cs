using AssignmentFinder.Infrastructure;
using AssignmentFinder.Scraping;
using AssignmentFinder.Worker.Jobs;
using AssignmentFinder.Worker.Notifications;
using Quartz;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddAssignmentFinderInfrastructure(builder.Configuration);
builder.Services.AddAssignmentConnectors();

builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection(EmailOptions.SectionName));
builder.Services.AddScoped<EmailSender>();
builder.Services.AddScoped<MatchNotifier>();

var cronSchedule = builder.Configuration["Scan:CronSchedule"] ?? "0 0 */3 * * ?"; // every 3 hours by default

builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("ScanJob");
    q.AddJob<ScanJob>(opts => opts.WithIdentity(jobKey));
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("ScanJob-trigger")
        .WithCronSchedule(cronSchedule));
});
builder.Services.AddQuartzHostedService(opts => opts.WaitForJobsToComplete = true);

var host = builder.Build();
host.Run();

using test_task;
using test_task.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        services.AddSingleton<ICampaignSender, CampaignSender>();
        services.AddSingleton<ICampaignScheduler, CampaignScheduler>();
        services.AddTransient<ICustomerService, CustomerService>();
    })
    .Build();

host.Run();
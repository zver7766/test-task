using test_task.Models;

namespace test_task;

public class Worker : BackgroundService
{
    private readonly ICampaignScheduler _campaignScheduler;

    public Worker(ICampaignScheduler campaignScheduler)
    {
        _campaignScheduler = campaignScheduler;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Create a CancellationTokenSource with a 30-minute timeout
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(30));
        var cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, cts.Token).Token;

        await _campaignScheduler.ScheduleBulkCampaignsAsync(Campaign.GetExampleCampaigns(), cancellationToken);
    }
}
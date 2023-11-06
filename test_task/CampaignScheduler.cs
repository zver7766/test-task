using test_task.Extensions;
using test_task.Models;
using test_task.Services;

namespace test_task;

public interface ICampaignScheduler
{
    Task ScheduleBulkCampaignsAsync(IEnumerable<Campaign> campaigns, CancellationToken cancellationToken);
    Task ScheduleCampaignAsync(Campaign campaign, CancellationToken cancellationToken);
}

public class CampaignScheduler : ICampaignScheduler
{
    private readonly ICampaignSender _campaignSender;
    private readonly ICustomerService _customerService;
    private Dictionary<int, List<Campaign>> _campaignsSentPerDay;
    private Timer _campaignSchedulerTimer;
    private Timer _dailyUpdateTimer;

    public CampaignScheduler(ICampaignSender campaignSender, ICustomerService customerService)
    {
        _campaignSender = campaignSender ?? throw new ArgumentNullException(nameof(campaignSender));
        _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));

        _campaignsSentPerDay = new Dictionary<int, List<Campaign>>();

        // Schedule daily update timer (every 24 hours)
        _dailyUpdateTimer = new Timer(_ =>
        {
            // Reset the campaignsSentPerDay dictionary every 24 hours
            _campaignsSentPerDay.Clear();
        }, null, TimeSpan.FromHours(24), Timeout.InfiniteTimeSpan);
    }

    public async Task ScheduleBulkCampaignsAsync(IEnumerable<Campaign> campaigns, CancellationToken cancellationToken)
    {
        var sortedCampaigns = campaigns
            .OrderByDescending(c => c.Priority)
            .ThenBy(c => c.SendTime);

        foreach (var campaign in sortedCampaigns)
        {
            await ScheduleCampaignAsync(campaign, cancellationToken);
        }
    }

    public async Task ScheduleCampaignAsync(Campaign campaign, CancellationToken cancellationToken)
    {
        // Calculate the initial delay until the scheduled time
        var initialDelay = CalculateDelay(campaign.SendTime);

        // Get customers who meet the condition
        var eligibleCustomers = await GetAndFilterCustomersAsync(
            campaign.Condition,
            campaign,
            cancellationToken);

        // Mark the campaign as sent for the customer today
        MarkCampaignForCustomers(eligibleCustomers, campaign);

        // Schedule the campaign with a timer
        _campaignSchedulerTimer = new Timer(async _ =>
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var customerIds = _campaignsSentPerDay.GetCustomerIdsWithSimilarCampaigns(campaign);

                foreach (var customerId in customerIds)
                {
                    // Send the campaign
                    _campaignSender.SendCampaign(campaign.Id, campaign.Template, customerId);
                }

                // Write accumulated details to the "sends" HTML file with the template name
                await _campaignSender.WriteToSendsFileAsync(campaign, cancellationToken);
            }
            catch (Exception)
            {
                UnmarkCampaignForCustomers(campaign);
                throw;
            }

        }, null, initialDelay, Timeout.InfiniteTimeSpan);
    }

    private async Task<IEnumerable<Customer>> GetAndFilterCustomersAsync(
        Func<Customer, bool> condition,
        Campaign campaign,
        CancellationToken cancellationToken)
    {
        // Get all customers who meet the condition
        var allEligibleCustomers = await GetEligibleCustomersAsync(condition, cancellationToken);

        // Filter out customers who have received a campaign today with the same or higher priority
        var filteredCustomers = allEligibleCustomers.Where(customer => !HasCustomerReceivedCampaignToday(customer, campaign));

        return filteredCustomers;
    }

    private bool HasCustomerReceivedCampaignToday(Customer customer, Campaign campaign)
    {
        if (_campaignsSentPerDay.TryGetValue(customer.Id, out var sentCampaigns))
        {
            // Check if any campaign with equal or higher priority has been sent today
            return sentCampaigns.Any(sentCampaign => sentCampaign.Priority >= campaign.Priority);
        }

        return false;
    }

    private void MarkCampaignForCustomers(IEnumerable<Customer> customers, Campaign campaign)
    {
        foreach (var customer in customers)
        {
            if (!_campaignsSentPerDay.TryGetValue(customer.Id, out var sentCampaigns))
            {
                sentCampaigns = new List<Campaign>();
                _campaignsSentPerDay[customer.Id] = sentCampaigns;
            }

            sentCampaigns.Add(campaign);
        }
    }

    private void UnmarkCampaignForCustomers(Campaign campaign)
    {
        foreach (var sentCampaigns in _campaignsSentPerDay.Values)
        {
            sentCampaigns.RemoveAll(c => c.Id == campaign.Id);
        }
    }


    private async Task<IEnumerable<Customer>> GetEligibleCustomersAsync(Func<Customer, bool> condition, CancellationToken cancellationToken)
    {
        return await _customerService.GetCustomersByConditionAsync(condition, cancellationToken);
    }

    private TimeSpan CalculateDelay(DateTime sendTime)
    {
        // Calculate the delay until the scheduled time
        // Better to use UTC time zone, chosen standard for simplicity of testing
        var now = DateTime.Now;
        var delay = sendTime - now;

        // If the scheduled time has already passed for today, calculate the delay for the next occurrence
        if (delay.TotalMilliseconds < 0)
        {
            sendTime = sendTime.AddDays(1);
            delay = sendTime - now;
        }

        return delay;
    }
}
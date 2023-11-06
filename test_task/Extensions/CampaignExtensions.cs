using test_task.Models;

namespace test_task.Extensions;

public static class CampaignExtensions
{
    public static IEnumerable<int> GetCustomerIdsWithSimilarCampaigns(this Dictionary<int, List<Campaign>> campaignsSentPerDay, Campaign campaign)
    {
        return campaignsSentPerDay
            .Where(entry => entry.Value.Contains(campaign))
            .Select(entry => entry.Key)
            .ToList();
    }
}
using test_task.Models.Enums;

namespace test_task.Models;

public class Campaign
{
    public int Id { get; set; }
    public string Template { get; set; }
    public Func<Customer, bool> Condition { get; set; }
    public DateTime SendTime { get; set; }
    public int Priority { get; set; }

    public Campaign(
        int id,
        string template,
        Func<Customer, bool> condition,
        DateTime sendTime,
        int priority
    )
    {
        Id = id;
        Template = template;
        Condition = condition;
        SendTime = sendTime;
        Priority = priority;
    }

    // Create Method here just to not populate classes
    public static List<Campaign> GetExampleCampaigns()
    {
        return new List<Campaign>
        {
            new(
                1,
                "TemplateA",
                customer => customer.Gender == Gender.Male,
                new DateTime(2023, 11, 6, 11, 20, 00),
                1
            ),
            new(
                2,
                "TemplateB",
                customer => customer.Age > 45,
                new DateTime(2023, 11, 6, 11, 25, 00),
                2
            ),
            new(
                3,
                "TemplateC",
                customer => customer.City == "London",
                new DateTime(2023, 11, 6, 11, 25, 00),
                5
            ),
            new(
                4,
                "TemplateA",
                customer => customer.Deposit > 100,
                new DateTime(2023, 11, 6, 11, 15, 00),
                3
            ),
            new(
                5,
                "TemplateC",
                customer => customer.NewCustomer,
                new DateTime(2023, 11, 6, 11, 15, 00),
                4
            )
        };

    }
}
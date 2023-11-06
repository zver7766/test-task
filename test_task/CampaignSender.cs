using System.Globalization;
using System.Text;
using test_task.Models;

namespace test_task;

public interface ICampaignSender
{
    void SendCampaign(int id, string template, int customerId);
    Task WriteToSendsFileAsync(Campaign campaign, CancellationToken cancellationToken);
}
public class CampaignSender : ICampaignSender
{
    private const string PathToGenerated = "Data/Generated/";

    public void SendCampaign(int id, string template, int customerId)
    {
        // Implement logic to send the campaign
        // Accumulate details for each eligible customer
        Console.WriteLine($"Campaign ID: {id}, Template: {template}, Customer ID: {customerId}");
    }

    public async Task WriteToSendsFileAsync(Campaign campaign, CancellationToken cancellationToken)
    {
        var htmlContent = await GetHtmlContentAsync(campaign.Template, cancellationToken);
        var formattedSendTime = campaign.SendTime.ToString("yyyy-MM-dd HH-mm-ss", CultureInfo.InvariantCulture);

        // Changed name of file from sends and dateTime to sends dateTime and campaignId to exclude collisions
        await WriteToFileAsync(PathToGenerated + $"sends {formattedSendTime} - {campaign.Id}.html", htmlContent);
    }

    private async Task<string> GetHtmlContentAsync(string templateName, CancellationToken cancellationToken)
    {
        var filePath = $"./Templates/{templateName}.html";

        if (File.Exists(filePath))
        {
            using var reader = new StreamReader(filePath);
            {
                return await reader.ReadToEndAsync(cancellationToken);
            }
        }

        // Handle the case where the HTML file doesn't exist
        Console.WriteLine($"HTML file for template {templateName} not found.");
        return string.Empty;
    }

    private async Task WriteToFileAsync(string fileName, string content)
    {
        try
        {
            // Write to the file asynchronously
            await using (var writer = new StreamWriter(fileName, true, Encoding.UTF8))
            {
                await writer.WriteAsync(content);
            }

            Console.WriteLine($"Content written to file: {fileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing to file: {ex.Message}");
        }
    }

}
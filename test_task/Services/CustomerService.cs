using test_task.Models;
using test_task.Models.Enums;

namespace test_task.Services;

public interface ICustomerService
{
    Task<IEnumerable<Customer>> GetCustomersByConditionAsync(Func<Customer, bool> condition, CancellationToken cancellationToken);
}
public class CustomerService : ICustomerService
{
    private List<Customer> Customers;
    private const string DataPath = "./Data/customers.csv";

    public async Task<IEnumerable<Customer>> GetCustomersByConditionAsync(Func<Customer, bool> condition, CancellationToken cancellationToken)
    {
        // Pretending of getting customer from Db, getting every request for simplicity
        await ReadAndParseCsvAsync(DataPath, cancellationToken);

        return Customers.Where(condition);
    }

    private async Task ReadAndParseCsvAsync(string filePath, CancellationToken cancellationToken)
    {
        Customers = new List<Customer>();

        using var reader = new StreamReader(filePath);
        // Read the header line to skip it
        await reader.ReadLineAsync(cancellationToken);

        // Read and parse the rest of the lines asynchronously
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            string[] fields = line.Split(',');

            var customer = new Customer
            {
                Id = int.Parse(fields[0]),
                Age = int.Parse(fields[1]),
                Gender = Enum.Parse<Gender>(fields[2]),
                City = fields[3],
                Deposit = int.Parse(fields[4]),
                NewCustomer = fields[5].Equals("1", StringComparison.OrdinalIgnoreCase) || fields[5].Equals("true", StringComparison.OrdinalIgnoreCase)
            };

            Customers.Add(customer);
        }
    }
}
using test_task.Models.Enums;

namespace test_task.Models;

public class Customer
{
    public int Id { get; set; }
    public int Age { get; set; }
    public Gender Gender { get; set; }
    public string City { get; set; }
    public int Deposit { get; set; }
    public bool NewCustomer { get; set; }
}
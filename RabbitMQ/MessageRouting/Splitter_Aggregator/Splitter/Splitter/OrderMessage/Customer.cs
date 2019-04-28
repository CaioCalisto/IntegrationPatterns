
namespace Splitter.OrderMessage
{
    public class Customer
    {
        public int Id { get; set; }
        public string Email { get; set; }

        public Customer(int id, string email)
        {
            Id = id;
            Email = email;
        }
    }
}

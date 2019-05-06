
namespace Order.Messages.In
{
    public class Response
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public string Message { get; set; }
    }
}

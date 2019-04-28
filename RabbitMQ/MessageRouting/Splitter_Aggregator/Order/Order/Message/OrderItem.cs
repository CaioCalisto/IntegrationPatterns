
namespace Order.Message
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public Type ItemType { get; set; }

        public OrderItem(int id, int quantity, Type itemType)
        {
            Id = id;
            Quantity = quantity;
            ItemType = itemType;
        }
    }
}

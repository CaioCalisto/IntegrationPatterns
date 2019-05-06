
namespace Validation_A.Messages
{
    public class Message<T>
    {
        public Header Header { get; set; }
        public T Body { get; set; }

        public Message(Header header, T body)
        {
            Header = header;
            Body = body;
        }
    }
}

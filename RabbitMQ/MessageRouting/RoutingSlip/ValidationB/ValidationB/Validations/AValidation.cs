using FluentValidator;

namespace ValidationB.Validations
{
    public abstract class AValidation: Notifiable
    {
        public abstract bool Process(Messages.Orders.Order order);
    }
}

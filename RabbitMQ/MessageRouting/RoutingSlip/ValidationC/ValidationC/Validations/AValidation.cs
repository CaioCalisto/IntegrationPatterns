using FluentValidator;

namespace ValidationC.Validations
{
    public abstract class AValidation: Notifiable
    {
        public abstract bool Process(Messages.Orders.Order order);
    }
}

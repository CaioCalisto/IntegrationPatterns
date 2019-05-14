using FluentValidator;

namespace Validation_A.Validations
{
    public abstract class AValidation: Notifiable
    {
        public abstract bool Process(Messages.Orders.Order order);
    }
}

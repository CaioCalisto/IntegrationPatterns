
namespace Validation_A.Validations
{
    public class Validation: AValidation
    {
        public override bool Process(Messages.Orders.Order order)
        {
            return true;
        }
    }
}

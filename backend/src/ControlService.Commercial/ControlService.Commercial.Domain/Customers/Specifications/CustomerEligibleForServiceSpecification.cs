using ControlService.SharedKernel.SeedWork;

namespace ControlService.Commercial.Domain.Customers.Specifications;

public sealed class CustomerEligibleForServiceSpecification : ISpecification<Customer>
{
    public bool IsSatisfiedBy(Customer customer)
        => customer.Document is not null && 
           !string.IsNullOrWhiteSpace(customer.LegalName) && 
           customer.Address is not null;
}

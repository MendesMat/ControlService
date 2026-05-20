namespace ControlService.Commercial.Domain.Customers.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id);
    Task AddAsync(Customer customer);
    Task UpdateAsync(Customer customer);
}

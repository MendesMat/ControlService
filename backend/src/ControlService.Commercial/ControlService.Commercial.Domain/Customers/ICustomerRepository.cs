using ControlService.SharedKernel.SeedWork;

namespace ControlService.Commercial.Domain.Customers;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Customer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
    void Update(Customer customer);
    void Remove(Customer customer);
}

using ControlService.Domain.Commercial.Customers;
using ControlService.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace ControlService.Infrastructure.Data.Repositories.Commercial;

public class CustomerRepository : ICustomerRepository
{
    private readonly ControlServiceDbContext _context;

    public IUnitOfWork UnitOfWork => _context;

    public CustomerRepository(ControlServiceDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        await _context.Customers.AddAsync(customer, cancellationToken);
    }

    public async Task<IEnumerable<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public void Remove(Customer customer) => _context.Customers.Remove(customer);

    public void Update(Customer customer) => _context.Customers.Update(customer);
}

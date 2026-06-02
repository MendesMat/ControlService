using ControlService.Commercial.Domain.Customers;
using ControlService.Commercial.Domain.Customers.ValueObjects;
using ControlService.SharedKernel.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace ControlService.Commercial.Infrastructure.Data.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly CommercialDbContext _context;

    public IUnitOfWork UnitOfWork => _context;

    public CustomerRepository(CommercialDbContext context)
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

    public async Task<bool> ExistsByDocumentAsync(
        Document document,
        Guid? excludedCustomerId = null,
        CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .AnyAsync(c =>
                c.Document != null &&
                c.Document.Value == document.Value &&
                c.Id != excludedCustomerId,
                cancellationToken);
    }

    public void Remove(Customer customer) => _context.Customers.Remove(customer);

    public void Update(Customer customer) => _context.Customers.Update(customer);
}


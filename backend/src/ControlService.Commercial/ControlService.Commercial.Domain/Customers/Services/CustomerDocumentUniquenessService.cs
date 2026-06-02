using ControlService.Commercial.Domain.Customers.ValueObjects;
using ControlService.SharedKernel.SeedWork;

namespace ControlService.Commercial.Domain.Customers.Services;

public sealed class CustomerDocumentUniquenessService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerDocumentUniquenessService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task EnforceUniquenessAsync(
        Document document,
        Guid? excludedCustomerId = null,
        CancellationToken cancellationToken = default)
    {
        var documentAlreadyInUse = await _customerRepository.ExistsByDocumentAsync(
            document,
            excludedCustomerId,
            cancellationToken);

        if (documentAlreadyInUse)
            throw new DomainException(
                $"Já existe um cliente cadastrado com o documento {document.GetFormattedValue()}.");
    }
}

using MediatR;
using ControlService.Domain.Commercial.Customers;
using ControlService.Domain.SeedWork;

namespace ControlService.Application.Commercial.Customers.Commands;

public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, bool>
{
    private readonly ICustomerRepository _repository;

    public DeleteCustomerCommandHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (customer == null)
            throw new EntityNotFoundException(nameof(Customer), request.Id);

        _repository.Remove(customer);
        return await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}

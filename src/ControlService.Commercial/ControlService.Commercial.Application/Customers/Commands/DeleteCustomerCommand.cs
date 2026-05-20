using MediatR;

namespace ControlService.Commercial.Application.Customers.Commands;

public class DeleteCustomerCommand : IRequest<bool>
{
    public Guid Id { get; set; }

    public DeleteCustomerCommand(Guid id) => Id = id;
}

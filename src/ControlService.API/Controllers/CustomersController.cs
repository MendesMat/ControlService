using MediatR;
using Microsoft.AspNetCore.Mvc;
using ControlService.Application.Commercial.Customers.Commands;
using ControlService.Application.Commercial.Customers.Queries;
using ControlService.Application.Commercial.Customers.DTOs;

namespace ControlService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Cria um novo cliente no sistema.
    /// </summary>
    /// <param name="command">Dados para criação do cliente.</param>
    /// <returns>O cliente criado.</returns>
    /// <response code="201">Cliente criado com sucesso.</response>
    /// <response code="400">Dados da requisição inválidos.</response>
    /// <response code="422">Falha na validação de negócio.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CustomerResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Obtém os detalhes de um cliente pelo seu ID único.
    /// </summary>
    /// <param name="id">O identificador único (GUID) do cliente.</param>
    /// <returns>Os dados do cliente encontrado.</returns>
    /// <response code="200">Cliente encontrado.</response>
    /// <response code="404">Cliente não localizado.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetCustomerByIdQuery(id));
        return Ok(result);
    }

    /// <summary>
    /// Lista todos os clientes cadastrados.
    /// </summary>
    /// <returns>Uma coleção de clientes.</returns>
    /// <response code="200">Lista de clientes recuperada com sucesso.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CustomerResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllCustomersQuery());
        return Ok(result);
    }

    /// <summary>
    /// Atualiza os dados de um cliente existente.
    /// </summary>
    /// <param name="id">O identificador do cliente a ser atualizado.</param>
    /// <param name="command">Novos dados do cliente.</param>
    /// <returns>O cliente atualizado.</returns>
    /// <response code="200">Cliente atualizado com sucesso.</response>
    /// <response code="400">ID da rota não coincide com o ID do comando.</response>
    /// <response code="404">Cliente não localizado.</response>
    /// <response code="422">Erro de validação de negócio.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CustomerResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerCommand command)
    {
        if (id != command.Id)
            return BadRequest("O ID da rota não corresponde ao objeto.");

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Remove um cliente do sistema.
    /// </summary>
    /// <param name="id">O identificador único do cliente.</param>
    /// <returns>Sem conteúdo em caso de sucesso.</returns>
    /// <response code="204">Cliente removido com sucesso.</response>
    /// <response code="404">Cliente não localizado.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteCustomerCommand(id));
        return NoContent();
    }
}

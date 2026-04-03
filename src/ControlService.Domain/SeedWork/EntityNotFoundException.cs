namespace ControlService.Domain.SeedWork;

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string entityName, object key)
        : base($"{entityName} com identificador '{key}' não foi encontrado.") { }
}

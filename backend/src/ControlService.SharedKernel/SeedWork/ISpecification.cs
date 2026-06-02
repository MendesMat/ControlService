namespace ControlService.SharedKernel.SeedWork;

public interface ISpecification<T>
{
    bool IsSatisfiedBy(T entity);
}

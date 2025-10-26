namespace PTM.Application.Interfaces.Policies;

public interface IUserPlanPolicy<T>
{
    Task Validate(T entity);
}

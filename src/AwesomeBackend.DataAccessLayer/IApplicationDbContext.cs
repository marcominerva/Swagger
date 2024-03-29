namespace AwesomeBackend.DataAccessLayer;

public interface IApplicationDbContext
{
    IQueryable<T> GetData<T>(bool trackingChanges = false) where T : class;

    void Insert<T>(T entity) where T : class;

    void Delete<T>(T entity) where T : class;

    Task SaveAsync();
}

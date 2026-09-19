namespace BlogWriter;

public interface ISessionOwnerProvider
{
    Task<string> GetOwnerIdAsync(CancellationToken cancellationToken = default);
}

namespace OtHoneypot.Core.Interfaces;
public interface IDataService
{
    Task GenerateDataAsync(CancellationToken cancellationToken);

    List<IData> GetGeneratedDataAsync();
}
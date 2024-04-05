namespace Booking.Presentation.Options;

public record ArchiverOptions
{
    public TimeSpan StorageTime { get; set; }
    public TimeSpan StartingFrequency { get; set; }
    public string Strategy { get; set; }
    public string SourceDirectory { get; set; }
    public string DestinationDirectory { get; set; }
}

// TODO: сделать ручку для изменения настроек
public class ArchiverOptionsBuilder
{
    private readonly ArchiverOptions _settings = new ArchiverOptions();
    
    public ArchiverOptionsBuilder SetStorageTime(TimeSpan storageTime)
    {
        _settings.StorageTime = storageTime;
        return this;
    }

    public ArchiverOptionsBuilder SetSourceDirectory(string sourceDirectory)
    {
        _settings.SourceDirectory = sourceDirectory;
        return this;
    }

    public ArchiverOptionsBuilder SetDestinationDirectory(string destinationDirectory)
    {
        _settings.DestinationDirectory = destinationDirectory;
        return this;
    }

    public ArchiverOptions Build()
    {
        return _settings;
    }
}
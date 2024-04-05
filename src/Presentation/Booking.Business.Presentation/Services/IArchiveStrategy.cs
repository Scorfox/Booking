namespace Booking.Presentation.Services;

public interface IArchiveStrategy
{
    void ArchiveLogs(string sourceDirectory, string destinationPath);
}
namespace Booking.Presentation.Services;

public interface IArchiverService
{
    void ArchiveLogs();
    void SetStrategy(IArchiveStrategy strategy);
}
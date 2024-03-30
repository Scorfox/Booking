namespace Booking.Business.Application.Services;

public interface IBookingReservationLogger
{
    void AddLog(string logMessage);
    public void ProcessLogs();
}
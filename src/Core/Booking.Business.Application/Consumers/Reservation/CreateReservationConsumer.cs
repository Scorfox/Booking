using AutoMapper;
using Booking.Business.Application.Repositories;
using Booking.Business.Application.Services;
using MassTransit;
using Otus.Booking.Common.Booking.Contracts.Filial.Requests;
using Otus.Booking.Common.Booking.Contracts.Filial.Responses;
using Otus.Booking.Common.Booking.Contracts.Reservation.Requests;
using Otus.Booking.Common.Booking.Contracts.Reservation.Responses;
using Otus.Booking.Common.Booking.Contracts.User.Requests;
using Otus.Booking.Common.Booking.Contracts.User.Responses;
using Otus.Booking.Common.Booking.Exceptions;
using Otus.Booking.Common.Booking.Notifications.Models;

namespace Booking.Business.Application.Consumers.Reservation;

public class CreateReservationConsumer : IConsumer<CreateReservation>
{
    private readonly IBookingReservationLogger _bookingReservationLogger;
    private readonly IMapper _mapper;
    private readonly IRequestClient<GetUserById> _userRequestClient;
    private readonly IRequestClient<GetFilialById> _filialByIdRequestClient;
    private readonly ITableRepository _tableRepository;
    private readonly IReservationRepository _reservationRepository;

    public CreateReservationConsumer(
        IBookingReservationLogger bookingReservationLogger,
        IMapper mapper,
        ITableRepository tableRepository,
        IRequestClient<GetUserById> userRequestClient,
        IRequestClient<GetFilialById> filialByIdRequestClient,
        IReservationRepository reservationRepository
    )
    {
        _bookingReservationLogger = bookingReservationLogger;
        _mapper = mapper;
        _userRequestClient = userRequestClient;
        _filialByIdRequestClient = filialByIdRequestClient;
        _tableRepository = tableRepository;
        _reservationRepository = reservationRepository;
    }
    
    public async Task Consume(ConsumeContext<CreateReservation> context)
    {
        var request = context.Message;
        
        var table = await _tableRepository.FindByIdAsync(request.TableId);

        if (table == null)
            throw new BadRequestException($"Table with ID {request.TableId} doesn't exist");
        
        _bookingReservationLogger.AddLog($"Getting user {request.WhoBookedId} from Booking.Auth...");
        var user = await _userRequestClient
            .GetResponse<GetUserResult>(new GetUserById { Id = request.WhoBookedId });
        _bookingReservationLogger.AddLog($"Getting filial {request.FilialId} from Booking.Auth...");
        var filial = await _filialByIdRequestClient
            .GetResponse<GetFilialResult>(new GetFilialById {Id = table.FilialId});

        var reservation = _mapper.Map<Domain.Entities.Reservation>(request);
        
        await _reservationRepository.CreateAsync(reservation);
        
        await context.RespondAsync(_mapper.Map<CreateReservationResult>(reservation));
        
        _bookingReservationLogger.AddLog($"Table {request.TableId} is pre-booked from {request.From} to {request.To} by user {request.WhoBookedId}");
        
        _bookingReservationLogger.AddLog($"Sending message to Booking.Notification for user {request.WhoBookedId} ...");
        
        var reservationCreatedNotification = new ReservationCreatedNotification
        {
            Email = user.Message.Email,
            Address = filial.Message.Address,
            FilialName = filial.Message.Name,
            FirstName = user.Message.FirstName,
            LastName = user.Message.LastName,
            PersonsCount = table.SeatsNumber,
            TableName = table.Name,
            From = request.From.DateTime.ToShortDateString() + " - " + request.From.DateTime.ToShortTimeString(),
            To = request.To.DateTime.ToShortTimeString()
        };

        await context.Publish(reservationCreatedNotification);
        
        _bookingReservationLogger.ProcessLogs();
    }
}
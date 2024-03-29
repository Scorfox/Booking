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
    private readonly BookingLog _bookingLog;
    private readonly IMapper _mapper;
    private readonly IRequestClient<GetUserById> _userRequestClient;
    private readonly IRequestClient<GetFilialById> _filialByIdRequestClient;
    private readonly ITableRepository _tableRepository;
    private readonly IReservationRepository _reservationRepository;

    public CreateReservationConsumer(
        BookingLog bookingLog,
        IMapper mapper,
        ITableRepository tableRepository,
        IRequestClient<GetUserById> userRequestClient,
        IRequestClient<GetFilialById> filialByIdRequestClient,
        IReservationRepository reservationRepository
    )
    {
        _bookingLog = bookingLog;
        _mapper = mapper;
        _userRequestClient = userRequestClient;
        _filialByIdRequestClient = filialByIdRequestClient;
        _tableRepository = tableRepository;
        _reservationRepository = reservationRepository;
    }
    
    public async Task Consume(ConsumeContext<CreateReservation> context)
    {
        var request = context.Message;

        var user = await _userRequestClient.GetResponse<GetUserResult>(new GetUserById { Id = request.WhoBookedId });
        var table = await _tableRepository.FindByIdAsync(request.TableId);

        if (table == null)
            throw new BadRequestException($"Table with ID {request.TableId} doesn't exist");

        var filial = await _filialByIdRequestClient
            .GetResponse<GetFilialResult>(new GetFilialById {Id = table.FilialId});

        var reservation = _mapper.Map<Domain.Entities.Reservation>(request);
        
        await _reservationRepository.CreateAsync(reservation);
        
        await context.RespondAsync(_mapper.Map<CreateReservationResult>(reservation));
        
        _bookingLog.AddLog($"{request.TableId} is pre-booked at {request.From} to {request.To} by userId {request.WhoBookedId}");

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
    }
}
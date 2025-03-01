﻿using AutoMapper;
using Booking.Business.Application.Repositories;
using MassTransit;
using Otus.Booking.Common.Booking.Contracts.Filial.Requests;
using Otus.Booking.Common.Booking.Contracts.Filial.Responses;
using Otus.Booking.Common.Booking.Contracts.Reservation.Requests;
using Otus.Booking.Common.Booking.Contracts.Reservation.Responses;
using Otus.Booking.Common.Booking.Contracts.User.Requests;
using Otus.Booking.Common.Booking.Contracts.User.Responses;
using Otus.Booking.Common.Booking.Exceptions;
using Otus.Booking.Common.Booking.Notifications.Enums;
using Otus.Booking.Common.Booking.Notifications.Models;

namespace Booking.Business.Application.Consumers.Reservation;

public class ConfirmReservationConsumer : IConsumer<ConfirmReservation>
{
    private readonly IMapper _mapper;
    private readonly IReservationRepository _reservationRepository;
    private readonly ITableRepository _tableRepository;
    private readonly IRequestClient<GetUserById> _userRequestClient;
    private readonly IRequestClient<GetFilialById> _filialByIdRequestClient;

    public ConfirmReservationConsumer(
        IMapper mapper,
        IReservationRepository reservationRepository,
        IRequestClient<GetUserById> userRequestClient,
        IRequestClient<GetFilialById> filialRequestClient,
        ITableRepository tableRepository
        )
    {
        _mapper = mapper;
        _userRequestClient = userRequestClient;
        _filialByIdRequestClient = filialRequestClient;
        _reservationRepository = reservationRepository;
        _tableRepository = tableRepository;
    }
    
    public async Task Consume(ConsumeContext<ConfirmReservation> context)
    {
        var request = context.Message;

        var table = await _tableRepository.FindByIdAsync(request.TableId);

        if (table == null)
            throw new NotFoundException($"Table with ID {request.TableId} doesn't exist");
        
        var reservation = await _reservationRepository.FindByIdAsync(request.Id);
        
        if (reservation == null)
            throw new NotFoundException($"Reservation with ID {request.Id} doesn't exist");
        
        if (request.CompanyId != reservation.Table.CompanyId)
            throw new ForbiddenException($"RequestCompanyId {request.CompanyId} is not equal TableCompanyId {reservation.Table.CompanyId}");
        
        var user = await _userRequestClient
            .GetResponse<GetUserResult>(new GetUserById { Id = reservation.WhoBookedId });
        var filial = await _filialByIdRequestClient
            .GetResponse<GetFilialResult>(new GetFilialById {Id = request.FilialId, CompanyId = request.CompanyId});

        reservation.WhoConfirmedId = request.WhoConfirmedId;
        
        await _reservationRepository.UpdateAsync(reservation);
        
        await context.RespondAsync(_mapper.Map<ConfirmReservationResult>(reservation));

        var reservationStatusNotification = new ReservationStatusChangedNotification
        {
            Email = user.Message.Email,
            Address = filial.Message.Address,
            FilialName = filial.Message.Name,
            FirstName = user.Message.FirstName,
            LastName = user.Message.LastName,
            PersonsCount = table.SeatsNumber,
            TableName = table.Name,
            Status = ReservationStatus.Confirmed,
            From = reservation.From,
            To = reservation.To
        };

        await context.Publish(reservationStatusNotification);
    }
}
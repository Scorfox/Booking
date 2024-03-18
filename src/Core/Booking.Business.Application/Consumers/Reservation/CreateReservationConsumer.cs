﻿using AutoMapper;
using Booking.Business.Application.Repositories;
using MassTransit;
using Otus.Booking.Common.Booking.Contracts.Reservation.Requests;
using Otus.Booking.Common.Booking.Contracts.Reservation.Responses;
using Otus.Booking.Common.Booking.Exceptions;

namespace Booking.Business.Application.Consumers.Reservation;

public class CreateReservationConsumer : IConsumer<CreateReservation>
{
    private readonly IMapper _mapper;
    private readonly IReservationRepository _reservationRepository;
    private readonly ITableRepository _tableRepository;

    public CreateReservationConsumer(
        IMapper mapper,
        IReservationRepository reservationRepository,
        ITableRepository tableRepository
        )
    {
        _mapper = mapper;
        _reservationRepository = reservationRepository;
        _tableRepository = tableRepository;
    }
    
    public async Task Consume(ConsumeContext<CreateReservation> context)
    {
        var request = context.Message;
        
        if (!await _tableRepository.HasAnyByIdAsync(request.TableId))
            throw new BadRequestException($"Table with ID {request.TableId} doesn't exists");
            
        var reservation = _mapper.Map<Domain.Entities.Reservation>(request);
        
        await _reservationRepository.CreateAsync(reservation);

        await context.RespondAsync(_mapper.Map<CreateReservationResult>(reservation));
    }
}
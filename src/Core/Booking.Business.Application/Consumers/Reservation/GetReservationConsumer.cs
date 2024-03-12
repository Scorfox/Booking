﻿using AutoMapper;
using Booking.Business.Application.Repositories;
using MassTransit;
using Otus.Booking.Common.Booking.Contracts.Reservation.Requests;
using Otus.Booking.Common.Booking.Contracts.Reservation.Responses;
using Otus.Booking.Common.Booking.Exceptions;

namespace Booking.Business.Application.Consumers.Reservation;

public sealed class GetReservationConsumer : IConsumer<GetReservationById>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IMapper _mapper;

    public GetReservationConsumer(IReservationRepository reservationRepository, IMapper mapper)
    {
        _reservationRepository = reservationRepository;
        _mapper = mapper;
    }

    public async Task Consume(ConsumeContext<GetReservationById> context)
    {
        var request = context.Message;

        var reservation = _reservationRepository.FindByIdAsync(request.Id);

        if (reservation == null)
            throw new NotFoundException($"Reservation with ID {request.Id} doesn't exists");

        await context.RespondAsync(_mapper.Map<GetReservationResult>(reservation));
    }
}

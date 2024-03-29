using AutoMapper;
using Booking.Business.Application.Repositories;
using MassTransit;
using Otus.Booking.Common.Booking.Contracts.Reservation.Requests;
using Otus.Booking.Common.Booking.Contracts.Reservation.Responses;
using Otus.Booking.Common.Booking.Exceptions;

namespace Booking.Business.Application.Consumers.Reservation;

public class UpdateReservationConsumer : IConsumer<UpdateReservation>
{
    private readonly IMapper _mapper;
    private readonly IReservationRepository _reservationRepository;
    private readonly ITableRepository _tableRepository;

    public UpdateReservationConsumer(
        IMapper mapper,
        IReservationRepository reservationRepository, 
        ITableRepository tableRepository
        )
    {
        _mapper = mapper;
        _reservationRepository = reservationRepository;
        _tableRepository = tableRepository;
    }
    
    public async Task Consume(ConsumeContext<UpdateReservation> context)
    {
        var request = context.Message;
        
        if (!await _tableRepository.HasAnyByIdAsync(request.TableId))
            throw new NotFoundException($"Table with ID {request.TableId} doesn't exist");
        
        var reservation = await _reservationRepository.FindByIdAsync(request.Id);
        
        if (reservation == null)
            throw new NotFoundException($"Reservation with ID {request.Id} doesn't exist");
        
        if (request.CompanyId != reservation.Table.CompanyId)
            throw new ForbiddenException($"RequestCompanyId {request.CompanyId} is not equal TableCompanyId {reservation.Table.CompanyId}");
        
        _mapper.Map(request, reservation);
        
        await _reservationRepository.UpdateAsync(reservation!);

        await context.RespondAsync(_mapper.Map<UpdateReservationResult>(reservation));
    }
}
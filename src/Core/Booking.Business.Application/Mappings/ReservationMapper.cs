using AutoMapper;
using Booking.Business.Domain.Entities;
using Otus.Booking.Common.Booking.Contracts.Reservation.Models;
using Otus.Booking.Common.Booking.Contracts.Reservation.Requests;
using Otus.Booking.Common.Booking.Contracts.Reservation.Responses;

namespace Booking.Business.Application.Mappings;

public sealed class ReservationMapper : Profile
{
    public ReservationMapper()
    {
        // Create
        CreateMap<CreateReservation, Reservation>();
        CreateMap<Reservation, CreateReservationResult>();

        // Read
        CreateMap<Reservation, GetReservationResult>();
        CreateMap<Reservation, ReservationGettingDto>();

        // Update
        CreateMap<UpdateReservation, Reservation>();
        CreateMap<Reservation, UpdateReservationResult>();
        
        CreateMap<Reservation, ConfirmReservationResult>();
        CreateMap<Reservation, CancelReservationResult>();
    }
}
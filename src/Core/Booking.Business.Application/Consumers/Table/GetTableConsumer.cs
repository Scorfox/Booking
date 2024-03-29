using AutoMapper;
using Booking.Business.Application.Repositories;
using MassTransit;
using Otus.Booking.Common.Booking.Contracts.Table.Requests;
using Otus.Booking.Common.Booking.Contracts.Table.Responses;
using Otus.Booking.Common.Booking.Exceptions;

namespace Booking.Business.Application.Consumers.Table;

public sealed class GetTableConsumer : IConsumer<GetTableById>
{
    private readonly ITableRepository _tableRepository;
    private readonly IMapper _mapper;

    public GetTableConsumer(ITableRepository tableRepository, IMapper mapper)
    {
        _tableRepository = tableRepository;
        _mapper = mapper;
    }

    public async Task Consume(ConsumeContext<GetTableById> context)
    {
        var request = context.Message;

        var table = await _tableRepository.FindByIdAsync(request.Id);

        if (table == null)
            throw new NotFoundException($"Table with ID {request.Id} doesn't exist");

        await context.RespondAsync(_mapper.Map<GetTableResult>(table));
    }
}
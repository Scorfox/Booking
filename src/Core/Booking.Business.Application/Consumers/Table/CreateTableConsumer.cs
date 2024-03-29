using AutoMapper;
using Booking.Business.Application.Repositories;
using MassTransit;
using Otus.Booking.Common.Booking.Contracts.Table.Requests;
using Otus.Booking.Common.Booking.Contracts.Table.Responses;
using Otus.Booking.Common.Booking.Exceptions;

namespace Booking.Business.Application.Consumers.Table;

public class CreateTableConsumer : IConsumer<CreateTable>
{
    private readonly IMapper _mapper;
    private readonly ITableRepository _tableRepository;

    public CreateTableConsumer(IMapper mapper, ITableRepository tableRepository)
    {
        _mapper = mapper;
        _tableRepository = tableRepository;
    }
    
    public async Task Consume(ConsumeContext<CreateTable> context)
    {
        var request = context.Message;
        
        if (await _tableRepository.HasAnyWithFilialIdAndNameAsync(request.FilialId, request.Name))
            throw new ConflictException($"Table with NAME {request.Name} and FILIAL {request.FilialId} already exists");
        
        var table = _mapper.Map<Domain.Entities.Table>(request);
        
        await _tableRepository.CreateAsync(table);

        await context.RespondAsync(_mapper.Map<CreateTableResult>(table));
    }
}
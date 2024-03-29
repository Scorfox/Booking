using AutoMapper;
using Booking.Business.Application.Repositories;
using MassTransit;
using Otus.Booking.Common.Booking.Contracts.Table.Requests;
using Otus.Booking.Common.Booking.Contracts.Table.Responses;
using Otus.Booking.Common.Booking.Exceptions;

namespace Booking.Business.Application.Consumers.Table;

public class UpdateTableConsumer : IConsumer<UpdateTable>
{
    private readonly ITableRepository _tableRepository;
    private readonly IMapper _mapper;

    public UpdateTableConsumer(ITableRepository filialRepository, IMapper mapper)
    {
        _tableRepository = filialRepository;
        _mapper = mapper;
    }
    
    public async Task Consume(ConsumeContext<UpdateTable> context)
    {
        var request = context.Message;
        var table = await _tableRepository.FindByIdAsync(request.Id);
        
        if (table == null)
            throw new NotFoundException($"Table with ID {request.Id} doesn't exist");

        if (await _tableRepository.HasAnyWithFilialIdAndNameExceptIdAsync(request.Id, request.FilialId, request.Name))
            throw new ConflictException($"Table with NAME {request.Name} and FILIAL {request.FilialId} already exists");
        
        if (request.CompanyId != table.CompanyId)
            throw new ForbiddenException($"RequestCompanyId {request.CompanyId} is not equal TableCompanyId {table.CompanyId}");
                
        _mapper.Map(request, table);
        
        await _tableRepository.UpdateAsync(table);

        await context.RespondAsync(_mapper.Map<UpdateTableResult>(table));
    }
}
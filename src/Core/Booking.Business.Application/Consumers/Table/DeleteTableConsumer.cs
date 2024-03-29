using Booking.Business.Application.Repositories;
using MassTransit;
using Otus.Booking.Common.Booking.Contracts.Table.Requests;
using Otus.Booking.Common.Booking.Contracts.Table.Responses;
using Otus.Booking.Common.Booking.Exceptions;

namespace Booking.Business.Application.Consumers.Table;

public class DeleteTableConsumer:IConsumer<DeleteTable>
{
    private readonly ITableRepository _tableRepository;

    public DeleteTableConsumer(ITableRepository tableRepository)
    {
        _tableRepository = tableRepository;
    }

    public async Task Consume(ConsumeContext<DeleteTable> context)
    {
        var request = context.Message;

        var table = await _tableRepository.FindByIdAsync(request.Id);
        
        if (table == null)
            throw new NotFoundException($"Table with ID {request.Id} doesn't exist");
        
        if (request.CompanyId != table.CompanyId)
            throw new ForbiddenException($"RequestCompanyId {request.CompanyId} is not equal TableCompanyId {table.CompanyId}");
        
        await _tableRepository.Delete(table);
        
        await context.RespondAsync(new DeleteTableResult());
    }
}
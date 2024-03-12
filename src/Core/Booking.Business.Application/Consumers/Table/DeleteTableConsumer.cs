﻿using Booking.Business.Application.Repositories;
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
            throw new NotFoundException($"Table with ID {request.Id} doesn't exists");
        
        await _tableRepository.Delete(table);
        
        await context.RespondAsync(new DeleteTableResult());
    }
}
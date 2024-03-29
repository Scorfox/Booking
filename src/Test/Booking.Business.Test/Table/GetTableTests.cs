using AutoFixture;
using AutoMapper;
using Booking.Business.Application.Consumers.Table;
using Booking.Business.Application.Mappings;
using Booking.Business.Application.Repositories;
using Booking.Business.Persistence.Repositories;
using MassTransit.Testing;
using Otus.Booking.Common.Booking.Contracts.Table.Requests;
using Otus.Booking.Common.Booking.Contracts.Table.Responses;

namespace Booking.Business.Test.Table;

public class GetTableTests : BaseTest
{
    private GetTableConsumer Consumer { get; }
    private ITableRepository TableRepository { get; }

    public GetTableTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<TableMapper>());

        TableRepository = new TableRepository(DataContext);
        Consumer = new GetTableConsumer(TableRepository, new Mapper(config));
    }

    [Test]
    public async Task GetTableByIdTest_ReturnsSuccess()
    {
        // Arrange

        var table = Fixture.Build<Domain.Entities.Table>().Without(e => e.Reservations).Create();
        await TableRepository.CreateAsync(table);
            
        var request = Fixture.Build<GetTableById>().With(e => e.Id, table.Id).Create();

        var testHarness = new InMemoryTestHarness();
        testHarness.Consumer(() => Consumer);
        await testHarness.Start();

        // Act
        await testHarness.InputQueueSendEndpoint.Send(request);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(testHarness.Consumed.Select<GetTableById>().Any(), Is.True);
            Assert.That(testHarness.Published.Select<GetTableResult>().Any(), Is.True);
        });

        await testHarness.Stop();
    }
}
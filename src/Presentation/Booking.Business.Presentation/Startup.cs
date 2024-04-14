using Booking.Business.Application.Consumers.Reservation;
using Booking.Business.Application.Consumers.Table;
using Booking.Business.Application;
using Booking.Business.Persistence;
using Booking.Presentation.Jobs;
using Booking.Presentation.Options;
using Booking.Presentation.Services;
using MassTransit;
using Quartz;
using Quartz.AspNetCore;

namespace Booking.Presentation
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ArchiverOptions>(Configuration.GetSection("Archiver"));
            services.ConfigurePersistence(Configuration);
            services.ConfigureApplication(Configuration);

            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSingleton<IArchiverService, ArchiverService>();

            services.AddMassTransit(x =>
            {
                // Добавляем шину сообщений
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(Configuration["RabbitMQ:Host"], h =>
                    {
                        h.Username(Configuration["RabbitMQ:Username"]);
                        h.Password(Configuration["RabbitMQ:Password"]);
                    });

                    cfg.ConfigureEndpoints(context);

                });

                // Table
                x.AddConsumer<CreateTableConsumer>();
                x.AddConsumer<GetTableConsumer>();
                x.AddConsumer<GetTablesListConsumer>();
                x.AddConsumer<UpdateTableConsumer>();
                x.AddConsumer<DeleteTableConsumer>();

                // Reservation
                x.AddConsumer<CreateReservationConsumer>();
                x.AddConsumer<GetReservationConsumer>();
                x.AddConsumer<GetReservationsListConsumer>();
                x.AddConsumer<UpdateReservationConsumer>();

                x.AddConsumer<CancelReservationConsumer>();
                x.AddConsumer<ConfirmReservationConsumer>();
            });

            if (Boolean.Parse(Configuration["Archiver:Enabled"]))
            {            
                services.AddQuartz(q =>
                {
                    // Just use the name of your job that you created in the Jobs folder.
                    var jobKey = new JobKey("ArchiveLogs");
                    q.AddJob<ArchiveLogsJob>(opts => opts.WithIdentity(jobKey));

                    q.AddTrigger(opts => opts
                        .ForJob(jobKey)
                        .WithIdentity("ArchiveLogsJob-trigger")
                        .StartNow()
                        .WithSimpleSchedule(x => x
                            .WithInterval(TimeSpan.Parse(Configuration["Archiver:StartingFrequency"]))
                            .RepeatForever()));
                });
                // ASP.NET Core hosting
                services.AddQuartzServer(options =>
                {
                    // when shutting down we want jobs to complete gracefully
                    options.WaitForJobsToComplete = true;
                });
            }
        }

        public void Configure(WebApplication app, IWebHostEnvironment env)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            app.UseCors();
            app.MapControllers();
        }
    }
}

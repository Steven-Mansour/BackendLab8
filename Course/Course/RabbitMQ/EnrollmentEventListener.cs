using RabbitMQ.Client;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client.Events;
using System.Text;
using Course.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Course.RabbitMQ
{
    public class EnrollmentEventListener : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private IConnection _connection;
        private IChannel _channel;
        private string _queueName; 

        public EnrollmentEventListener(IServiceScopeFactory scopeFactory)
        {
            Console.WriteLine("Created EnrollmentEventListener");
            _scopeFactory = scopeFactory;
        }

        private async Task InitializeRabbitMqAsync()
        {
            Console.WriteLine("Initializing RabbitMQ...");
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.ExchangeDeclareAsync(exchange: "enrollment_exchange", type: ExchangeType.Fanout);

            var queueDeclareOk = await _channel.QueueDeclareAsync();
            _queueName = queueDeclareOk.QueueName;  // queue name
            await _channel.QueueBindAsync(queue: _queueName,
                                          exchange: "enrollment_exchange",
                                          routingKey: "");

            await _channel.BasicQosAsync(0, 1, false);
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await InitializeRabbitMqAsync();
            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Execution Started");
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (ch, ea) =>
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                var enrollmentEvent = JsonConvert.DeserializeObject<EnrollmentEvent>(content);
                await ProcessEnrollment(enrollmentEvent);
                // Just for debugging
                Console.WriteLine($"{enrollmentEvent.ClassId} {enrollmentEvent.StudentId}");

                await _channel.BasicAckAsync(ea.DeliveryTag, false);
            };

            Console.WriteLine("Starting consumer on queue: " + _queueName);
            await _channel.BasicConsumeAsync(queue: _queueName,
                                             autoAck: false,
                                             consumer: consumer);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        private async Task ProcessEnrollment(EnrollmentEvent enrollmentEvent)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ClassenrollmentsContext>();
                bool isEnrolled = await dbContext.Enrollments
                    .AnyAsync(e => e.Studentid == enrollmentEvent.StudentId && e.Classid == enrollmentEvent.ClassId);

                if (!isEnrolled)
                {
                    var enrollment = new Enrollment
                    {
                        Studentid = enrollmentEvent.StudentId,
                        Classid = enrollmentEvent.ClassId
                    };
                    bool isClass = await dbContext.Classes.AnyAsync(c => c.Classid == enrollmentEvent.ClassId);
                    if (isClass)
                    {
                        dbContext.Enrollments.Add(enrollment); 
                        await dbContext.SaveChangesAsync();
                        
                        Console.WriteLine($"Enrollment saved: Class {enrollmentEvent.ClassId}, Student {enrollmentEvent.StudentId}");
                    }
                    else
                    {
                        Console.WriteLine("Class doesn't exist");
                    }
                }
                else
                {
                    Console.WriteLine($"Student {enrollmentEvent.StudentId} is already enrolled in Class {enrollmentEvent.ClassId}");
                }
            }
        }

        public override void Dispose()
        {
            _channel?.CloseAsync();
            _connection?.CloseAsync();
            base.Dispose();
        }
    }

    public class EnrollmentEvent
    {
        public int StudentId { get; set; }
        public int ClassId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

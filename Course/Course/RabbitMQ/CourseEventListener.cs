
using RabbitMQ.Client;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client.Events;
using System.Text;
using Course.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
namespace Course.RabbitMQ
{
    public class CourseEventListener : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private IConnection _connection;
        private IChannel _channel;
        private string _queueName; 

        public CourseEventListener(IServiceScopeFactory scopeFactory)
        {
            Console.WriteLine("Created CourseEventListener");
            _scopeFactory = scopeFactory;
        }
        private async Task InitializeRabbitMqAsync()
        {
            Console.WriteLine("Initializing RabbitMQ...");
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.ExchangeDeclareAsync(exchange: "course_exchange", type: ExchangeType.Fanout);

            var queueDeclareOk = await _channel.QueueDeclareAsync();
            _queueName = queueDeclareOk.QueueName;  // queue name
            await _channel.QueueBindAsync(queue: _queueName,
                                          exchange: "course_exchange",
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
                var enrollmentEvent = JsonConvert.DeserializeObject<CourseEvent>(content);
                await ProcessEnrollment(enrollmentEvent);
                await _channel.BasicAckAsync(ea.DeliveryTag, false);
            };

            Console.WriteLine("Starting consumer on queue: " + _queueName);
            await _channel.BasicConsumeAsync(queue: _queueName,
                                             autoAck: false,
                                             consumer: consumer);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        private async Task ProcessEnrollment(CourseEvent courseEvent)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ClassenrollmentsContext>();
                    var course = new Models.Course
                    {
                       CourseName = courseEvent.CourseName,
                    };
                    
                dbContext.Courses.Add(course); 
                await dbContext.SaveChangesAsync();
        
                Console.WriteLine($"Course saved: {course.CourseName}");
               
            }
        }

        public override void Dispose()
        {
            _channel?.CloseAsync();
            _connection?.CloseAsync();
            base.Dispose();
        }
    }

    public class CourseEvent
    {
        public String CourseName { get; set; }
    }
}

using RabbitMQ.Client;
namespace DemoLab7.RabbitMQ;

using System.Text;
using Newtonsoft.Json;

public class EnrollmentEventPublisher
{
    public async void PublishEnrollmentEvent(int studentId, int classId)
    {
        try
        {
            var enrollmentEvent = new
            {
                StudentId = studentId,
                ClassId = classId,
                Timestamp = DateTime.UtcNow
            };

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();
            
            await channel.ExchangeDeclareAsync(exchange: "enrollment_exchange", type: ExchangeType.Fanout);

            var message = JsonConvert.SerializeObject(enrollmentEvent);
            var body = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(
                exchange: "enrollment_exchange",
                routingKey: "",
                body: body
            );
            Console.WriteLine("Enrollment event published.");
        }
        catch (Exception e)
        {
            throw new Exception(); 
        }
    }
}

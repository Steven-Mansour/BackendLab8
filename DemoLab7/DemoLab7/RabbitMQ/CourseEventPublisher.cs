using System.Text;
using DemoLab7.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace DemoLab7.RabbitMQ;

public class CourseEventPublisher
{
    public async void PublishCourseEvent(Course course)
    {
        try
        {
            var courseEvent = new
            {
                courseName = course.CourseName,
            };
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();
            await channel.ExchangeDeclareAsync(exchange: "course_exchange", type: ExchangeType.Fanout);

            var message = JsonConvert.SerializeObject(courseEvent);
            var body = Encoding.UTF8.GetBytes(message);
            await channel.BasicPublishAsync(
                exchange: "course_exchange",
                routingKey: "",
                body: body
            );
            Console.WriteLine("Course event published.");
        }catch (Exception e)
        {
            throw new Exception(); 
        }
    }
}
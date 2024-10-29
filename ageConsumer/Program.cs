using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

class AgeConsumer
{
    static void Main(string[] args)
    {
        var factory = new ConnectionFactory() { HostName = "localhost", Port = 5672 };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        //Declare the ageQueue to recieve an age from the splitter
        channel.QueueDeclare(queue: "ageQueue",
                            durable: false,
                            exclusive: false,
                            autoDelete: false,
                            arguments: null);

        //Declare updatedAgeQueue to send Enriched age (+5 for simplicity)
        channel.QueueDeclare(queue: "updatedAgeQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);

        //Consumer til at modtage og håndtere beskeder fra ageQueue
        EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine(" [x] Received Age: {0}", message);

            string newMessage = (Convert.ToInt32(message)+5).ToString();

            var updatedAgeProps = channel.CreateBasicProperties();
            updatedAgeProps.CorrelationId = ea.BasicProperties.CorrelationId; // Preserve CorrelationId

            channel.BasicPublish(exchange: "", routingKey: "updatedAgeQueue", basicProperties: updatedAgeProps, body: Encoding.UTF8.GetBytes(newMessage));
            Console.WriteLine("Sent age:{0} to updatedAgeQueue", newMessage);
        };

        //Opret basicconsume på ageQueue
        channel.BasicConsume(queue: "ageQueue", autoAck: true, consumer: consumer);

        Console.WriteLine(" Press [enter] to exit.");
        Console.WriteLine("Waiting for messages.");


        Console.ReadLine();
    }
}
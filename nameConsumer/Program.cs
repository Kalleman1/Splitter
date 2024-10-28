using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

class NameConsumer
{
    static void Main(string[] args)
    {
        var factory = new ConnectionFactory() { HostName = "localhost", Port = 5672 };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        //Opret nameQueue
        channel.QueueDeclare("nameQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);

        //Declare updatedNameQueue to send Enriched name (+Clausen for simplicity)
        channel.QueueDeclare(queue: "updatedNameQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);

        //Consumer til at modtage og håndtere beskeder fra ageQueue
        EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine(" [x] Received {0}", message);

            string updatedMessage = message + " Clausen";

            channel.BasicPublish(exchange: "", routingKey: "updatedNameQueue", basicProperties: null, body: Encoding.UTF8.GetBytes(updatedMessage));
            Console.WriteLine("Sent {0} to updatedNameQueue", updatedMessage);

        };

        //Opret basicconsume på nameQueue
        channel.BasicConsume(queue: "nameQueue",
                             autoAck: true,
                             consumer: consumer);

        Console.WriteLine(" Press [enter] to exit.");
        Console.WriteLine("Waiting for messages.");
        Console.ReadLine();
    }
}
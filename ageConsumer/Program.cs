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
        channel.QueueDeclare(queue: "ageQueue",
                            durable: false,
                            exclusive: false,
                            autoDelete: false,
                            arguments: null);

        //Consumer til at modtage og håndtere beskeder fra ageQueue
        EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine(" [x] Received {0}", message);
        };

        //Opret basicconsume på ageQueue
        channel.BasicConsume(queue: "ageQueue", autoAck: true, consumer: consumer);

        Console.WriteLine(" Press [enter] to exit.");
        Console.WriteLine("Waiting for messages.");


        Console.ReadLine();
    }
}
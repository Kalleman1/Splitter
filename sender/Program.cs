using System;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

class Sender
{
    static void Main(string[] args)
    {
        var factory = new ConnectionFactory() { HostName = "localhost", Port = 5672 };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        //Declare composite Queue - Dette er vores 'fulde' besked (Fulde person objekt)
        channel.QueueDeclare("compositeQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);

        var person = new Person
        {
            FirstName = "John",
            LastName = "Doe",
            Age = 30
        };

        var message = JsonConvert.SerializeObject(person);
        var body = Encoding.UTF8.GetBytes(message);

        //Send fulde person objekt til compositeQueue
        channel.BasicPublish(exchange: "",
                             routingKey: "compositeQueue",
                             basicProperties: null,
                             body: body);

        Console.WriteLine(" [x] Sent {0}", message);
        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }
}

public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
}
using System;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

class Splitter
{
    static void Main(string[] args)
    {
        var person = new Person();
        var factory = new ConnectionFactory() { HostName = "localhost", Port = 5672 };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        //Declare composite Queue - Dette er vores 'fulde' besked (Fulde person objekt)
        channel.QueueDeclare(queue: "compositeQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);

        //Split queue 1 - Her sendes der udelukkende navnet på personen
        channel.QueueDeclare(queue: "nameQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);

        //Split queue 2 - Her sendes der udelukkende alderen på personen
        channel.QueueDeclare(queue: "ageQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);


        //Consumer til at modtage og håndtere beskeder fra vores compositeQueue
        EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            //Modtager body og deserializer til et Person objekt
            var body = ea.Body;
            var arrayBody = body.ToArray();
            var deserializedBody = JsonSerializer.Deserialize<Person>(arrayBody);

            //Split personen i name og age
            var name = person.SplitName(deserializedBody);
            var age = person.SplitAge(deserializedBody);

            //Send name til nameQueue
            channel.BasicPublish(exchange: "",
                             routingKey: "nameQueue",
                             basicProperties: null,
                             body: Encoding.UTF8.GetBytes(name));
            Console.WriteLine(" [x] Sent {0}", name);

            //Send age til ageQueue
            channel.BasicPublish(exchange: "",
                             routingKey: "ageQueue",
                             basicProperties: null,
                             body: Encoding.UTF8.GetBytes(age.ToString()));
            Console.WriteLine(" [x] Sent {0}", age);

        };

        //Opret basicconsume på compositeQueue
        channel.BasicConsume(queue: "compositeQueue", autoAck: true, consumer: consumer);


        Console.WriteLine(" Press [enter] to exit.");
        Console.WriteLine("Waiting for messages.");


        Console.ReadLine();
    }

    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }

        public int SplitAge(Person person)
        {
            return person.Age;
        }

        public string SplitName(Person person)
        {
            return person.FirstName + " " + person.LastName;
        }

    }
}
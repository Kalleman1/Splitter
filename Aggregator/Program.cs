using System;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

class Aggregator
{
    static void Main(string[] args)
    {
        Dictionary<string, Person> persons = new Dictionary<string, Person>();
        var factory = new ConnectionFactory() { HostName = "localhost", Port = 5672 };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "updatedNameQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
        channel.QueueDeclare(queue: "updatedAgeQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);

        // Name Consumer
        EventingBasicConsumer nameConsumer = new EventingBasicConsumer(channel);
        nameConsumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var correlationId = ea.BasicProperties.CorrelationId;

            //Checks if correlationID is already present, if not create a new person obejct and add to dict
            if (!persons.ContainsKey(correlationId))
                persons[correlationId] = new Person();

            var person = persons[correlationId];
            var nameParts = message.Split(' ');
            person.FirstName = nameParts[0];
            person.LastName = string.Join(" ", nameParts.Skip(1));

            Console.WriteLine("Name for CorrelationId {0} has been added: {1}", correlationId, person.getFullName());
        };

        // Age Consumer
        EventingBasicConsumer ageConsumer = new EventingBasicConsumer(channel);
        ageConsumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var correlationId = ea.BasicProperties.CorrelationId;

            //Checks if correlationID is already present, if not create a new person obejct and add to dict
            if (!persons.ContainsKey(correlationId))
                persons[correlationId] = new Person();

            var person = persons[correlationId];
            person.Age = int.Parse(message);

            Console.WriteLine("Age for CorrelationId {0} has been added: {1}", correlationId, person.Age);
        };

        channel.BasicConsume(queue: "updatedNameQueue", autoAck: true, consumer: nameConsumer);
        channel.BasicConsume(queue: "updatedAgeQueue", autoAck: true, consumer: ageConsumer);

        Console.WriteLine(" Press [enter] to exit.");
        Console.WriteLine("Waiting for messages.");
        Console.ReadLine();
    }

    public static List<string> SplitName(string fullName)
    {
        List<string> result = new List<string>();
        string[] serviceArr = fullName.Split(' ');
        result.Add(serviceArr[0]);
        string lastName = string.Join(" ",serviceArr.Skip(1));
        result.Add(lastName);

        return result;
    }

    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }


        public bool hasName()
        { 
            return FirstName != null && LastName != null;
        }

        public bool hasAge()
        {
            return Age < 0;
        }

        public string getFullName()
        { 
            return FirstName + " " + LastName;
        }
    }
}
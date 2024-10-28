using System;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

class Aggregator
{
    static void Main(string[] args)
    {
        List<Person> persons = new List<Person>();
        var person = new Person();
        var factory = new ConnectionFactory() { HostName = "localhost", Port = 5672 };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        //Her modtages det opdaterede navn fra nameConsumer
        channel.QueueDeclare(queue: "updatedNameQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);

        //Her modtages den opdaterede alder fra ageConsumer
        channel.QueueDeclare(queue: "updatedAgeQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);


        //Consumer til at modtage og håndtere beskeder fra vores compositeQueue
        EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            //Modtager body og decoder bytes til string
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine(" [x] Received {0}", message);

            try
            {
                int age = Convert.ToInt32(message);
                bool isAgePopulated = false;
                foreach  (Person person in persons)
                {
                    if (!person.hasAge())
                    {
                        person.Age = age;
                        break;
                    }
                }

                if (!isAgePopulated)
                {
                    persons.Add(new Person { Age=age });
                }
            }
            catch (Exception)
            {
                List<string> firstAndLastName = SplitName(message);
                bool isNamePopulated = false;
                foreach (Person person in persons)
                {
                    if (!person.hasName())
                    {
                        person.FirstName = firstAndLastName[0];
                        person.LastName = firstAndLastName[1];
                        isNamePopulated = true;
                        break;
                    }
                }

                if (!isNamePopulated)
                {
                    persons.Add(new Person { FirstName = firstAndLastName[0], LastName = firstAndLastName[1] });
                }
            }

            Console.WriteLine(persons.Count.ToString());
        };

        //Opret basicconsume på compositeQueue
        channel.BasicConsume(queue: "updatedAgeQueue", autoAck: true, consumer: consumer);
        channel.BasicConsume(queue: "updatedNameQueue", autoAck: true, consumer: consumer);


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
    }
}
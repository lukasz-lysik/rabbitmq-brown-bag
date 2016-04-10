using System;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace DirectRouting.EmailClient
{
    class Program
    {
        private string exchangeName = "4dev_04_x_emails";

        static void Main(string[] args)
        {
            Console.Write("Your address: ");
            var address = Console.ReadLine();

            var factory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            var connection = factory.CreateConnection();
            
            Task.Factory.StartNew(() => StartConsumer(connection, address));
            Task.Factory.StartNew(() => StartPublisher(connection));
            Task.WaitAll();
        }

        private static void StartConsumer(IConnection connection, string address)
        {
            using (var channel = connection.CreateModel())
            {
                var exchangeName = "4dev_04_x_emails";
                var queueName = "4dev_04_consumer_" + address;

                channel.ExchangeDeclare(exchangeName, "direct");
                channel.QueueDeclare(queueName, true, false, false, null);
                channel.QueueBind(queueName, exchangeName, address);

                var consumer = new QueueingBasicConsumer(channel);
                channel.BasicConsume(queueName, true, consumer);

                while (true)
                {
                    var ea = consumer.Queue.Dequeue();

                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);

                    Console.WriteLine("Received: {0}", message);
                }
            }
        }
        
        private static void StartPublisher(IConnection connection)
        {
            using (var channel = connection.CreateModel())
            {
                // 1. Declare queue

                var exchangeName = "4dev_04_x_emails";
                channel.ExchangeDeclare(exchangeName, "direct");
                
                channel.BasicReturn += (sender, args) =>
                    Console.WriteLine("User {0} doesn't exist.", args.RoutingKey);

                while (true)
                {   
                    var input = Console.ReadLine().Split(new[] {':'}, 2);

                    var recipient = input[0];
                    var message = input[1];

                    var body = Encoding.UTF8.GetBytes(message);
                    var routingKey = recipient;

                    //channel.BasicPublish(exchangeName, routingKey, null, body);
                    channel.BasicPublish(exchangeName, routingKey, true, null, body);
                }
            }
        }
    }
}

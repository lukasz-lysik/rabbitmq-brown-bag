// ReSharper Disable All

using System;
using System.Text;
using System.Timers;
using RabbitMQ.Client;

namespace PubSub.Producer
{
    class Program
    {
        static void Main(string[] args)
        {
            var random = new Random();

            var factory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            using(var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    // 1. Declare queue

                    var exchangeName = "4dev_03_x_pub_sub";
                    channel.ExchangeDeclare(exchangeName, "fanout");

                    var timer = new Timer(1000);
                    timer.Elapsed += (sender, eventArgs) =>
                    {
                        var message = random.Next(100).ToString();
                        
                        var body = Encoding.UTF8.GetBytes(message);
                        Console.WriteLine("Publishing: {0}", message);
                        channel.BasicPublish(exchangeName, string.Empty, null, body);
                    };
                    timer.Start();

                    Console.WriteLine("Producer started");
                    Console.ReadLine();
                }
            }
        }
    }
}

// ReSharper Restore All
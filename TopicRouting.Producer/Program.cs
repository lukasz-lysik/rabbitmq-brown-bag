using System;
using System.Text;
using System.Timers;
using RabbitMQ.Client;

namespace TopicRouting.Producer
{
    class Program
    {
        private static readonly string[] Countries = { "USA", "UK", "Canada"};
        private static readonly string[] Types = {"Politics", "Sports", "Weather"};
        private static readonly string[] Formats = {"Article", "Video", "Audio"};

        static void Main(string[] args)
        {
            var random = new Random();

            var factory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    // 1. Declare queue

                    var exchangeName = "4dev_05_x_news";
                    channel.ExchangeDeclare(exchangeName, "topic");

                    var timer = new Timer(1000);
                    timer.Elapsed += (sender, eventArgs) =>
                    {
                        var country = GetRandom(random, Countries);
                        var type = GetRandom(random, Types);
                        var format = GetRandom(random, Formats);

                        var routingKey = string.Format("{0}.{1}.{2}", country, type, format);
                        var message = "News for " + routingKey;

                        Console.WriteLine("Publishing: {0}", message);

                        var body = Encoding.UTF8.GetBytes(message);
                        channel.BasicPublish(exchangeName, routingKey, null, body);
                    };
                    timer.Start();

                    Console.WriteLine("Producer started");
                    Console.ReadLine();
                }
            }
        }

        private static string GetRandom(Random random, string[] countries)
        {
            return countries[random.Next(countries.Length)];
        }
    }
}

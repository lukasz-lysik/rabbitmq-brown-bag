using System;
using System.Text;
using RabbitMQ.Client;

namespace AckReject.SpamProcessor
{
    // dead-letter-exchange:	bb_08_x_spam

    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    var exchangeName = "bb_08_x_spam";
                    var queueName = "bb_08_spam";

                    channel.ExchangeDeclare(exchangeName, "fanout");
                    channel.QueueDeclare(queueName, true, false, false, null);
                    channel.QueueBind(queueName, exchangeName, "");

                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume(queueName, true, consumer);

                    while (true)
                    {
                        var ea = consumer.Queue.Dequeue();

                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);

                        Console.WriteLine("SPAM received: {0}", message);
                    }
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// ReSharper Disable All

namespace HelloWorld.Consumer
{
    class Program
    {
        private static void Main(string[] args)
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    // 1. Declare queue
                    
                    var queueName = "bb_01_hello_world";
                    var durable = true;
                    var exclusive = false;
                    var autoDelete = false;
                    var arguments = new Dictionary<string, object>();

                    // Note: declare on producer and consumer have to match
                    channel.QueueDeclare(queueName, durable, exclusive, autoDelete, arguments);

                    // 2. Start consuming
                    
                    var consumer = new QueueingBasicConsumer(channel);
                    var noAck = true;
                    channel.BasicConsume(queueName, noAck, consumer);
                    
                    while (true)
                    {
                        var ea = (BasicDeliverEventArgs) consumer.Queue.Dequeue();

                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);

                        Console.WriteLine("Received: {0}", message);
                        //Console.WriteLine("ConsumerTag: {0}", consumer.ConsumerTag);
                    }
                }
            }
        }
    }
}

// ReSharper Restore All
﻿using System;
using System.Text;
using RabbitMQ.Client;

namespace PubSub.DurableConsumer
{
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
                    // 1. Setup queue

                    var exchangeName = "bb_03_x_pub_sub";
                    var queueName = "bb_03_pub_sub_consumer";

                    channel.QueueDeclare(queueName, true, false, false, null);
                    channel.QueueBind(queueName, exchangeName, "");

                    // 2. Start consuming

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
        }
    }
}

using System;
// ReSharper Disable All 
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;

namespace HelloWorld.Producer
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
                    // Note: exclusive:true + autoDelete:true is used for RPC (later)
                    var arguments = new Dictionary<string, object>();
                    channel.QueueDeclare(queueName, durable, exclusive, autoDelete, arguments);

                    // 2. Prepare message

                    

                    // 3. Publish message

                    var mandatory = false;
                    var properties = channel.CreateBasicProperties();
                    
                    #region Setup optional properties

                    properties.SetPersistent(true);
                    properties.ContentType = "text/plain";
                    properties.Headers = new Dictionary<string, object>();
                    properties.Headers.Add("anything", "here");
                    #endregion

                    var i = 1;
                    while (true)
                    {
                        Console.WriteLine("Press ENTER to publish...");
                        Console.ReadLine();

                        var message = string.Format("Hello World! {0}", i++);
                        var body = Encoding.UTF8.GetBytes(message);

                        channel.BasicPublish(
                            exchange: "", 
                            routingKey: queueName, 
                            mandatory: mandatory, 
                            basicProperties: properties, 
                            body:body);

                        Console.WriteLine("Published: '{0}'", message);
                    }
                }
            }
        }
    }
}

// ReSharper Restore All
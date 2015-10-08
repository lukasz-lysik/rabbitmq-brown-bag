using System;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace ManualAck.EmailClient
{
    class Program
    {
        private string exchangeName = "bb_07_x_emails";

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
                var exchangeName = "bb_07_x_emails";
                var queueName = "bb_07_consumer_" + address;

                channel.ExchangeDeclare(exchangeName, "direct");
                channel.QueueDeclare(queueName, true, false, false, null);
                channel.QueueBind(queueName, exchangeName, address);

                var consumer = new QueueingBasicConsumer(channel);
                var noAck = false;
                channel.BasicConsume(queueName, noAck, consumer);

                while (true)
                {
                    var ea = consumer.Queue.Dequeue();

                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);

                    AckOrReject(channel, ea.DeliveryTag, message);
                }
            }
        }

        #region AckOrReject

        private static void AckOrReject(IModel channel, ulong deliveryTag, string message)
        {
            if (message.Contains("WTF"))
            {
                Console.WriteLine("REJECTED: {0}", message);
                channel.BasicReject(deliveryTag, false);
            }
            else
            {
                Console.WriteLine("Received: {0}", message);
                channel.BasicAck(deliveryTag, false);
            }
        }

        #endregion

        private static void StartPublisher(IConnection connection)
        {
            using (var channel = connection.CreateModel())
            {
                var exchangeName = "bb_07_x_emails";
                channel.ExchangeDeclare(exchangeName, "direct");
                
                while (true)
                {
                    var input = Console.ReadLine().Split(new[] { ':' }, 2);

                    var recipient = input[0];
                    var message = input[1];

                    var body = Encoding.UTF8.GetBytes(message);
                    var routingKey = recipient;

                    channel.BasicPublish(exchangeName, routingKey, null, body);

                }
            }
        }
    }
}

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AmqpReceiver
{
    class Program
    {
        static Stopwatch amqpTimer;
        static int amqpMessageCounter;

        static void Main(string[] args)
        {
            Console.Write("Enter message count to expect: ");
            string input = Console.ReadLine();
            int messageCount = int.Parse(input);
            Console.Write("Enter concurrent receivers: ");
            input = Console.ReadLine();
            int concurrent = int.Parse(input);
            List<Task> tasks = new List<Task>();
            amqpTimer = System.Diagnostics.Stopwatch.StartNew();
            amqpMessageCounter = messageCount;
            for (int idx = 0; idx < concurrent; ++idx)
            {
                tasks.Add(Task.Factory.StartNew(() => AmqpReceiver()));
            }

            Task.WaitAll(tasks.ToArray());
            Console.WriteLine($"AmqpMessageCount left: {amqpMessageCounter}, Time elapsed: {amqpTimer.Elapsed.ToString("mm':'ss':'fff")}");
            Console.Write("All done. Press enter: ");
            Console.ReadLine();
        }
        private static void AmqpReceiver()
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.UserName = "guest";
            factory.Password = "guest";
            factory.VirtualHost = "/";
            factory.HostName = "localhost";
            IConnection conn = factory.CreateConnection();
            IModel channel = conn.CreateModel();
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (ch, ea) =>
            {
                var body = ea.Body.ToArray();
                string message = $"Reiceved {body.Length} bytes"; 
                channel.BasicAck(ea.DeliveryTag, false);
                Console.WriteLine(message);
                --amqpMessageCounter;
                if (amqpMessageCounter == 0)
                    amqpTimer.Stop();
            };
            String consumerTag = channel.BasicConsume("perftest", false, consumer);
            int lastMessageCount = amqpMessageCounter;
            int counter = 0;
            while (true)
            {
                Thread.Sleep(500);
                if (amqpMessageCounter <= 0)
                    break;
                else if (lastMessageCount == amqpMessageCounter)
                {
                    ++counter;
                    if (counter > 10)
                        break;
                }
                else
                    counter = 0;
            }
            channel.BasicCancel(consumerTag);
            amqpTimer.Stop();
        }

    }
}

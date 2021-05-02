using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AmqpSender
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Enter message count: ");
            string input = Console.ReadLine();
            int messageCount = int.Parse(input);
            Console.Write("Persistent queues: ");
            input = Console.ReadLine();
            bool durable = input.ToUpper().StartsWith("Y");
            Console.Write("Bytes: ");
            input = Console.ReadLine();
            int size = int.Parse(input);
            Console.WriteLine($"OK. Running with {messageCount} messages and {(durable ? "persistent" : "non-persistent")} queues.");
            Stopwatch amqpTimer = Stopwatch.StartNew();
            List<Task> tasks = new List<Task>();
            tasks.Add(Task.Factory.StartNew(() => sendToAmqp(messageCount, durable, size)));
            Task.WaitAll(tasks.ToArray());
            amqpTimer.Stop();
            Console.WriteLine($"Time elapsed: {amqpTimer.Elapsed.ToString("mm':'ss':'fff")}");
            Console.Write("All done. Press enter: ");
            Console.ReadLine();
        }

        private static void sendToAmqp(int messageCount, bool durable, int size)
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.UserName = "guest";
            factory.Password = "guest";
            factory.VirtualHost = "/";
            factory.HostName = "localhost";
            IConnection conn = factory.CreateConnection();
            IModel channel = conn.CreateModel();
            channel.QueueDelete("perftest");
            channel.ExchangeDelete("perftest-exchange");
            channel.ExchangeDeclare("perftest-exchange", ExchangeType.Direct);
            channel.QueueDeclare("perftest", durable, false, false, null);
            channel.QueueBind("perftest", "perftest-exchange", "perform", null);
            channel.ConfirmSelect();
            Console.WriteLine("Sending to AMQP");
            for (int idx = 0; idx < messageCount; ++idx)
            {
                byte[] messageBodyBytes = GetByteArray(size);
                IBasicProperties props = channel.CreateBasicProperties();
                props.ContentType = "text/plain";
                props.DeliveryMode = (byte)(durable ? 2 : 1);
                channel.BasicPublish("perftest-exchange", "perform", props, messageBodyBytes);
                channel.WaitForConfirmsOrDie();
            }
            Console.WriteLine("Done");
        }
        static private byte[] GetByteArray(int sizeInBytes)
        {
            Random rnd = new Random();
            byte[] b = new byte[sizeInBytes]; // convert kb to byte
            rnd.NextBytes(b);
            return b;
        }

    }
}

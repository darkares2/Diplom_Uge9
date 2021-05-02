using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace MsMQSender
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
            Stopwatch timer = Stopwatch.StartNew();
            List<Task> tasks = new List<Task>();
            tasks.Add(Task.Factory.StartNew(() => sendToMSMQ(messageCount, durable, size)));
            Task.WaitAll(tasks.ToArray());
            timer.Stop();
            Console.WriteLine($"Time elapsed: {timer.Elapsed.ToString("mm':'ss':'fff")}");
            Console.Write("All done. Press enter: ");
            Console.ReadLine();
        }
        private static void sendToMSMQ(int messageCount, bool durable, int size)
        {
            if (MessageQueue.Exists(@".\Private$\PerfTest"))
                MessageQueue.Delete(@".\Private$\PerfTest");
            MessageQueue.Create(@".\Private$\PerfTest");
            MessageQueue messageQueue = new MessageQueue(@".\Private$\PerfTest");
            messageQueue.Label = "Queue Performance test";
            DefaultPropertiesToSend myDefaultProperties = new DefaultPropertiesToSend();
            myDefaultProperties.Label = "PerformanceTest";
            myDefaultProperties.Recoverable = durable;
            messageQueue.DefaultPropertiesToSend = myDefaultProperties;
            Console.WriteLine("Sending to MSMQ");
            for (int idx = 0; idx < messageCount; ++idx)
            {
                byte[] messageBodyBytes = GetByteArray(size);
                messageQueue.Send(messageBodyBytes);
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

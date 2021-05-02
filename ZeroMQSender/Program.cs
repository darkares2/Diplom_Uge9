using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroMQSender
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Enter message count: ");
            string input = Console.ReadLine();
            int messageCount = int.Parse(input);
            Console.WriteLine($"OK. Running with {messageCount} messages.");
            List<Task> tasks = new List<Task>();
            tasks.Add(Task.Factory.StartNew(() => sendToZeroMQ(messageCount)));
            Task.WaitAll(tasks.ToArray());
            Console.Write("All done. Press enter: ");
            Console.ReadLine();
        }

        private static void sendToZeroMQ(int messageCount)
        {
            using (var client = new PublisherSocket())
            {
                client.Connect("tcp://127.0.0.1:5555");
                Console.WriteLine("Sending to ZeroMQ");
                for (int idx = 0; idx < messageCount; ++idx)
                {
                    var message = new NetMQMessage();
                    message.Append("AAAA");
                    byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes($"{idx} - Messagebody.");
                    message.Append(messageBodyBytes);
                    client.SendMultipartMessage(message);
                    Console.Write(".");
                }
                Console.WriteLine("Done");
            }
        }

    }
}

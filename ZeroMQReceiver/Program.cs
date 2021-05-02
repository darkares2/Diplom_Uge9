using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroMQReceiver
{
    class Program
    {
        static Stopwatch zmqTimer;
        static int zmqMessageCounter;

        static void Main(string[] args)
        {
            Console.Write("Enter message count to expect: ");
            string input = Console.ReadLine();
            int messageCount = int.Parse(input);
            Console.Write("Enter concurrent receivers: ");
            input = Console.ReadLine();
            int concurrent = int.Parse(input);
            List<Task> tasks = new List<Task>();
            zmqTimer = Stopwatch.StartNew();
            zmqMessageCounter = messageCount;
            for (int idx = 0; idx < concurrent; ++idx)
            {
                //tasks.Add(Task.Factory.StartNew(() => AmqpReceiver()));
                tasks.Add(Task.Factory.StartNew(() => ZeroMQReceiver()));
                //tasks.Add(Task.Factory.StartNew(() => MSMQReceiver()));
            }

            Task.WaitAll(tasks.ToArray());
            Console.WriteLine($"ZmqMessageCount left: {zmqMessageCounter}, Time elapsed: {zmqTimer.Elapsed.ToString("mm':'ss':'fff")}");
            Console.Write("All done. Press enter: ");
            Console.ReadLine();
        }
        private static void ZeroMQReceiver()
        {
            using (var server = new SubscriberSocket())
            {
                server.Options.ReceiveHighWatermark = 1000;
                server.Bind("tcp://127.0.0.1:5555");
                server.Subscribe("AAAA");
                Console.WriteLine("Receiving ZeroMQ");
                int counter = 0;
                while (zmqMessageCounter > 0)
                {
                    NetMQMessage message2 = server.ReceiveMultipartMessage(2);
                    Console.WriteLine(message2[1].ConvertToString(Encoding.UTF8));
                    --zmqMessageCounter;
                    continue;
                    NetMQMessage message = new NetMQMessage(2);
                    if (server.TryReceiveMultipartMessage(ref message, 2))
                    {
                        Console.WriteLine(message[1].ConvertToString(Encoding.UTF8));
                        --zmqMessageCounter;
                        counter = 0;
                    }
                    else
                    {
                        ++counter;
                        Thread.Sleep(1000);
                        if (counter > 10)
                        {
                            Console.WriteLine("Giving up waiting...");
                            break;
                        }
                    }
                }
                Console.WriteLine("Done");
                zmqTimer.Stop();
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace MsMQReceiver
{
    class Program
    {
        static Stopwatch msmqTimer;
        static int msmqMessageCounter;

        static void Main(string[] args)
        {
            Console.Write("Enter message count to expect: ");
            string input = Console.ReadLine();
            int messageCount = int.Parse(input);
            Console.Write("Enter concurrent receivers: ");
            input = Console.ReadLine();
            int concurrent = int.Parse(input);
            List<Task> tasks = new List<Task>();
            msmqTimer = Stopwatch.StartNew();
            msmqMessageCounter = messageCount;
            for (int idx = 0; idx < concurrent; ++idx)
            {
                tasks.Add(Task.Factory.StartNew(() => MSMQReceiver()));
            }

            Task.WaitAll(tasks.ToArray());
            Console.WriteLine($"MsMQMessageCount left: {msmqMessageCounter}, Time elapsed: {msmqTimer.Elapsed.ToString("mm':'ss':'fff")}");
            Console.Write("All done. Press enter: ");
            Console.ReadLine();
        }

        private static void MSMQReceiver()
        {
            MessageQueue messageQueue = new MessageQueue(@".\Private$\PerfTest");
            messageQueue.Formatter = new BinaryMessageFormatter();
            Console.WriteLine("Receiving MSMQ");
            while (msmqMessageCounter > 0)
            {
                try
                {
                    Message message = messageQueue.Receive(new TimeSpan(0, 0, 5));
                    BinaryReader reader = new BinaryReader(message.BodyStream);
                    int count = (int)message.BodyStream.Length;
                    byte[] bytes = reader.ReadBytes(count);
                    --msmqMessageCounter;
                    Console.WriteLine($"Received {bytes.Length} bytes");
                }
                catch (Exception ex)
                {
                    break;
                }
            }
            Console.WriteLine("Done");
            msmqTimer.Stop();
        }

    }
}

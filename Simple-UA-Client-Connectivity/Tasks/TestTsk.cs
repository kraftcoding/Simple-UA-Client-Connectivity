using Opc.Ua;
using Simple_UA_Client_Connectivity.Helpers;
using SimpleUAClientLibrary.Controllers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Simple_UA_Client_Connectivity.Tasks
{
    internal static class TestTsk
    {
        static readonly object _locker = new object();
        static bool _go;
        
        public static void Main(int result, ProgramCtrl Prg, string nodeId)
        {
            //Console.WriteLine("Insert num of threads");
            //int.TryParse(Console.ReadLine(), out int result);

            Stopwatch sw = new Stopwatch();
            sw.Start();            

            new Thread(Work).Start();

            lock (_locker)
            {
                _go = true;
                Monitor.Pulse(_locker);
            }
            
            var q = new PCQueue(result);            

            for (int i = 0; i < 10000; i++)
            {                
                q.EnqueueItem(() =>
                {
                    Thread.Sleep(1000);
                    NodeId dataNodeId = new NodeId(nodeId);
                    DataValue simulatedDataValue = Prg.m_session.ReadValue(dataNodeId);
                    Console.WriteLine(simulatedDataValue.ToString());
                    Thread.Sleep(2000);
                });
            }

            q.Shutdown(true);
            Console.WriteLine();
            Console.WriteLine("Workers complete!");

            sw.Stop();
            Console.WriteLine(sw.Elapsed);

            //delay
            Console.ReadLine();

        }

        internal static void Work()
        {
            Console.WriteLine("work thread started");
            lock (_locker)
            {
                while (!_go)
                {
                    Monitor.Wait(_locker);    // Lock is released while we’re waiting
                    // lock is regained
                }
            }
            Console.WriteLine("Woken!!!");
        }
    }
}

using Opc.Ua;
using Simple_UA_Client_Connectivity.Tasks;
using SimpleUAClientLibrary.Controllers;
using System;
using System.Collections.Generic;

namespace Simple_UA_Client_Connectivity
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            //Params
            bool p_useSecurity = false;
            int p_baseAddressId = 0; //default 0

            //Test subscription
            Dictionary<string, string> subsDictionary = new Dictionary<string, string>();
            subsDictionary.Add("DisplayName", "Alarm");
            subsDictionary.Add("PublishingInterval", "1000");
            subsDictionary.Add("KeepAliveCount", "10");
            subsDictionary.Add("LifetimeCount", "100");
            subsDictionary.Add("MaxNotificationsPerPublish", "1000");
            subsDictionary.Add("PublishingEnabled", "true");

            // Test nodes
            string NodeIdAlarm = "i=13";
            //string[] nodeIdsServer = { "i=2256", "i=2268", "i=2258" };
            string[] nodeIdsServer = { NodeIdAlarm };

            // create program controller
            ProgramCtrl Prg = new ProgramCtrl(p_baseAddressId);

            // manage connection
            try
            {
                Prg.ConnectEndPoint(p_useSecurity);

                /* EXAMPLE 1 : Print all nodes */
                Prg.PrintNodesToLog();

                /* EXAMPLE 2 : Create subscriptions */
                Prg.CreateSubscription(null, null, null, 0); // default
                Prg.CreateSubscription(NodeIdAlarm, "Alarm", subsDictionary, MonitoringMode.Reporting);

                /* EXAMPLE 3 : Read nodes task with cancellation token */               
                TestTaskCancellationTkn.Launch(Prg, nodeIdsServer, 2000);
            }
            catch (Exception exception)
            {
                Utils.Trace("Error: " + exception.ToString());
            }
            finally
            {                
                Prg.DisconnectEndPoint();
            }
        }
    }
}

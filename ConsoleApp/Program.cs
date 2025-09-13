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
            #region Global variables

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
            //string nodeId = "i=2256"; // ServerStatus
            //string nodeId = "i=2266"; // Simple Alarm Server
            //string[] nodeIds = { "i=2256", "i=2268", "i=2258" };
            //string[] nodeIds = { "ns=2;s=0:/Green" };
            string[] nodeIds = { "i=2266" };

            #endregion          

            // create program controller
            ProgramManager Prg = new ProgramManager(p_baseAddressId);

            // manage connection
            try
            {
                // Stablish comunication with server
                Prg.ConnectEndPoint(p_useSecurity);

                /* EXAMPLE 1 : Print all nodes */
                Prg.PrintNodesToLog();

                /* EXAMPLE 2 : Create subscriptions */
                //Prg.CreateSubscription(null, null, null, 0); // Default
                //Prg.CreateSubscription(nodeId, "Tets suscription", subsDictionary, MonitoringMode.Reporting);
                //TemplateTsk.Launch(Prg, 2000); // Template task to manage the monitorized suscription events

                /* EXAMPLE 3 : Read nodes task with cancellation token */
                //ReadNodesTsk.Launch(Prg, nodeIds, 2000);

                /* EXAMPLE 4 : Write nodes task with cancellation token */
                WriteNodesTsk.Launch(Prg, nodeIds, 2000);
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

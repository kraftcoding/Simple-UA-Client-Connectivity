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
            bool p_useSecurity = true;
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
            string nodeId = "i=2256"; // ServerStatus
            //string nodeId = "ns=2;i=138"; // Boiler #1 (quick start app server)
            string[] nodeIds = { "i=2256", "i=2268", "i=2258" };

            #endregion          

            // create program controller
            ProgramCtrl Prg = new ProgramCtrl(p_baseAddressId);

            // manage connection
            try
            {
                // Stablish comunication with server
                // TODO:
                // - Implement the keep alive procedure to manage reconnections
                Prg.ConnectEndPoint(p_useSecurity);

                /* EXAMPLE 1 : Print all nodes */
                //Prg.PrintNodesToLog();

                /* EXAMPLE 2 : Create subscriptions */
                //Prg.CreateSubscription(null, null, null, 0); // Default
                Prg.CreateSubscription(nodeId, "Tets suscription", subsDictionary, MonitoringMode.Reporting);
                TemplateTsk.Launch(Prg, 2000); // Template task to manage the monitorized suscription events and manage a controled shut down

                /* EXAMPLE 3 : Read nodes task with cancellation token */
                //ReadNodesTsk.Launch(Prg, nodeIds, 2000);
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

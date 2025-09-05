using Opc.Ua;
using Simple_UA_Client_Connectivity.Tasks;
using SimpleUAClientLibrary.Controllers;
using System;
using System.Collections.Generic;
using System.Threading;

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

            // create program controller
            ProgramCtrl Prg = new ProgramCtrl(p_baseAddressId);

            // manage connection
            try
            {
                Prg.ConnectEndPoint(p_useSecurity);

                /* EXAMPLE 1 : Print all nodes */
                //Prg.PrintNodesToLog();

                /* EXAMPLE 2 : Create pubications */
                //Prg.CreateSubscription("ns=4;s=5:?ServerStatus/ServerState", "ServerState");

                /* EXAMPLE 3 : Read nodes task */
                string[] nodeIds = { "i=2256", "i=2268", "i=2258" };
                TestTaskCancellationTkn.Launch(Prg, nodeIds, 2000);
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

using Opc.Ua;
using Simple_UA_Client_Connectivity.Tasks;
using SimpleUAClientLibrary.Controllers;
using System;

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

                /* Example 1 : Print all nodes */
                Prg.PrintNodes();

                /* Example 2 : Create subscriptions */
                //Prg.CreateSubscription(null); // create default subscription
                //Prg.CreateSubscription("ns=4;s=5:?ServerStatus", "ServerStatus");

                /* Example 3 : Read node (server status) task */               
                TestTsk.Main(1, Prg, "i=2256");

            }
            catch (Exception exception)
            {
                Utils.Trace("Session opening error: " + exception.ToString());
            }
            finally
            {
                Prg.DisconnectEndPoint();
            }
        }
    }
}

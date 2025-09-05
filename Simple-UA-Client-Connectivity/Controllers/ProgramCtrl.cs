using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using Simple_UA_Client_Connectivity.Controllers;

namespace SimpleUAClientLibrary.Controllers
{
    public class ProgramCtrl
    {
        private ApplicationInstance application;
        private ApplicationConfiguration m_configuration;
        private string m_endPoint;
        public Session m_session;
        private ConnectivityCtrl ConnCtrl;

        public ProgramCtrl(int baseAddressId)
        {
            // application config. section
            application = new ApplicationInstance();
            application.ApplicationName = "Simple UA Client Connectivity";
            application.ConfigSectionName = "Opc.Ua.Client_Suacc_1";
            application.ApplicationType = ApplicationType.Client;

            // load the application configuration.
            application.LoadApplicationConfiguration(false);

            // check the application certificate.
            application.CheckApplicationInstanceCertificate(false, 0);

            // initialize connection
            m_configuration = application.ApplicationConfiguration;
                        
            m_endPoint = m_configuration.ServerConfiguration.BaseAddresses[baseAddressId];

            // create connectivity controler
            ConnCtrl = new ConnectivityCtrl(m_configuration);
        }

        public void ConnectEndPoint(bool useSecurity)
        {
            m_session = ConnCtrl.Connect(m_endPoint, useSecurity);
            Utils.Trace("Session opened");
        }

        public void CreateSubscription(string nodeId, string name)
        {
            SubscriptionsCtrl Subs = new SubscriptionsCtrl();
            Subs.Create(m_session, nodeId, name);
            Utils.Trace(nodeId + " suscription created");
        }

        public void PrintNodesToLog()
        {
            NodeCtrl Nods = new NodeCtrl();
            Nods.PrintRootFolderToLog(m_session);           
        }

        public void DisconnectEndPoint()
        {
            ConnCtrl.Disconnect();
            Utils.Trace("Session closed");
        }
    }
}

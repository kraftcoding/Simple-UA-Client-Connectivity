using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using Simple_UA_Client_Connectivity.Controllers;
using System.Collections.Generic;

namespace SimpleUAClientLibrary.Controllers
{
    public class ProgramCtrl
    {
        #region Public objects
        public Session m_session;
        #endregion

        #region Private objects
        private ApplicationInstance application;
        private ApplicationConfiguration m_configuration;
        private string m_endPoint;        
        private ConnectivityCtrl ConnCtrl;
        #endregion

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

        #region Public objects
        public void ConnectEndPoint(bool useSecurity)
        {
            m_session = ConnCtrl.Connect(m_endPoint, useSecurity);
            Utils.Trace("Session opened");
        }

        public void CreateSubscription(string nodeId, string name, Dictionary<string, string> subsDictionary, MonitoringMode monitoringMode)
        {
            SubscriptionsCtrl Subs = new SubscriptionsCtrl();
            Subscription m_subscription = null;

            if (subsDictionary == null)
            {
                m_subscription = Subs.CreateDefault(m_session);
            }
            else
            {
                m_subscription = Subs.CreateByDictionary(m_session, subsDictionary);
            }

            m_subscription.Create();
            Utils.Trace("Suscription created");

            // default
            MonitoredItem m_monitoredItem = Subs.GetMonitoredItem(nodeId, name, monitoringMode, null);

            // filtered
            //MonitoredItem m_monitoredItem = Subs.GetMonitoredItem(nodeId, name, monitoringMode, m_session);   

            m_subscription.AddItem(m_monitoredItem);
            Utils.Trace("Adding monitored item");

            m_subscription.ApplyChanges();

            //m_subscription.ConditionRefresh();

            m_session.AddSubscription(m_subscription);
            Utils.Trace("Apply changes");

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
        #endregion
    }
}

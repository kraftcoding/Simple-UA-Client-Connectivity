using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Client.Controls;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SimpleUAClientLibrary.Controllers
{
    internal class ConnectivityCtrl
    {
        #region constructor
        public ConnectivityCtrl(ApplicationConfiguration configuration) {
            m_configuration = configuration;
            m_CertificateValidation = new CertificateValidationEventHandler(CertificateValidator_CertificateValidation);
        }
        #endregion

        #region Private properties
        private CertificateValidationEventHandler m_CertificateValidation;
        private int m_reconnectPeriod = 10;
        private EventHandler m_ReconnectComplete;
        private EventHandler m_ReconnectStarting;
        private EventHandler m_KeepAliveComplete;
        private EventHandler m_ConnectComplete;        
        private SessionReconnectHandler m_reconnectHandler;
        private ApplicationConfiguration m_configuration;
        private Session m_session;
        #endregion

        #region Public properties
        public string[] PreferredLocales { get; set; }
        public IUserIdentity UserIdentity { get; set; }
        #endregion

        #region Internal methods

        /// <summary>
        /// Creates a new session.
        /// </summary>
        /// <returns>The new session object.</returns>
        internal Session Connect(string endPoint, bool useSecurity)
        {
            // disconnect from existing session.
            Disconnect();

            EndpointDescription endpointDescription = ClientUtils.SelectEndpoint(endPoint, useSecurity);
            EndpointConfiguration endpointConfiguration = EndpointConfiguration.Create(m_configuration);
            ConfiguredEndpoint endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);

            Session m_session = Session.Create(
                m_configuration,
                endpoint,
                false,
                true,
                m_configuration.ApplicationName,
                (uint) m_configuration.ServerConfiguration.MaxSessionTimeout,
                UserIdentity, //new UserIdentity();
                PreferredLocales);

            // set up keep alive callback.
            m_session.KeepAlive += new KeepAliveEventHandler(Session_KeepAlive);            

            // raise an event.
            DoConnectComplete(null);

            // return the new session.
            return m_session;
        }

        /// <summary>
        /// Disconnects from the server.
        /// </summary>
        internal void Disconnect()
        {
            //UpdateStatus(false, DateTime.UtcNow, "Disconnected");

            // stop any reconnect operation.
            if (m_reconnectHandler != null)
            {
                m_reconnectHandler.Dispose();
                m_reconnectHandler = null;
            }

            // disconnect any existing session.
            if (m_session != null)
            {
                m_session.Close(10000);
                m_session = null;
            }

            // raise an event.
            DoConnectComplete(null);
        }

        #endregion

        #region Private methods

        //private void Disconnect(Session m_session)
        //{
        //    //UpdateStatus(false, DateTime.UtcNow, "Disconnected");

        //    // stop any reconnect operation.
        //    if (m_reconnectHandler != null)
        //    {
        //        m_reconnectHandler.Dispose();
        //        m_reconnectHandler = null;
        //    }

        //    // disconnect any existing session.
        //    if (m_session != null)
        //    {
        //        m_session.Close(10000);
        //        m_session = null;
        //    }

        //    // raise an event.
        //    DoConnectComplete(null);
        //}

        /// <summary>
        /// Handles a keep alive event from a session.
        /// </summary>
        private void Session_KeepAlive(Session session, KeepAliveEventArgs e)
        {           
            try
            {
                // check for events from discarded sessions.
                if (!Object.ReferenceEquals(session, m_session))
                {
                    return;
                }

                // start reconnect sequence on communication error.
                if (ServiceResult.IsBad(e.Status))
                {
                    if (m_reconnectPeriod <= 0)
                    {
                        UpdateStatus(true, e.CurrentTime, "Communication Error ({0})", e.Status);
                        return;
                    }

                    UpdateStatus(true, e.CurrentTime, "Reconnecting in {0}s", m_reconnectPeriod);

                    if (m_reconnectHandler == null)
                    {
                        if (m_ReconnectStarting != null)
                        {
                            m_ReconnectStarting(this, e);
                        }

                        m_reconnectHandler = new SessionReconnectHandler();
                        m_reconnectHandler.BeginReconnect(m_session, m_reconnectPeriod * 1000, Server_ReconnectComplete);
                    }

                    return;
                }

                // update status.
                UpdateStatus(false, e.CurrentTime, "Connected [{0}]", session.Endpoint.EndpointUrl);

                // raise any additional notifications.
                if (m_KeepAliveComplete != null)
                {
                    m_KeepAliveComplete(this, e);
                }
            }
            catch (Exception exception)
            {
                ClientUtils.HandleException("Session_KeepAlive", exception);
            }
        }

        /// <summary>
        /// Updates the status control.
        /// </summary>
        /// <param name="error">Whether the status represents an error.</param>
        /// <param name="time">The time associated with the status.</param>
        /// <param name="status">The status message.</param>
        /// <param name="args">Arguments used to format the status message.</param>
        private void UpdateStatus(bool error, DateTime time, string status, params object[] args)
        {
            Utils.Trace(String.Format(status, args));
            Utils.Trace(time.ToLocalTime().ToString("hh:mm:ss"));
        }

        /// <summary>
        /// Raises the connect complete event on the main GUI thread.
        /// </summary>
        private void DoConnectComplete(object state)
        {
            if (m_ConnectComplete != null)
            {
                m_ConnectComplete(this, null);
            }
        }

        /// <summary>
        /// Handles a certificate validation error.
        /// </summary>
        private void CertificateValidator_CertificateValidation(CertificateValidator sender, CertificateValidationEventArgs e)
        {         

            try
            {
                e.Accept = m_configuration.SecurityConfiguration.AutoAcceptUntrustedCertificates;

                if (!m_configuration.SecurityConfiguration.AutoAcceptUntrustedCertificates)
                {
                    DialogResult result = MessageBox.Show(
                        e.Certificate.Subject,
                        "Untrusted Certificate",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    e.Accept = (result == DialogResult.Yes);
                }
            }
            catch (Exception exception)
            {
                ClientUtils.HandleException("CertificateValidator_CertificateValidation", exception);
            }
        }

        /// <summary>
        /// Handles a reconnect event complete from the reconnect handler.
        /// </summary>
        private void Server_ReconnectComplete(object sender, EventArgs e)
        {          

            try
            {
                // ignore callbacks from discarded objects.
                if (!Object.ReferenceEquals(sender, m_reconnectHandler))
                {
                    return;
                }

                m_session = m_reconnectHandler.Session;
                m_reconnectHandler.Dispose();
                m_reconnectHandler = null;

                // raise any additional notifications.
                if (m_ReconnectComplete != null)
                {
                    m_ReconnectComplete(this, e);
                }
            }
            catch (Exception exception)
            {
                ClientUtils.HandleException("Server_ReconnectComplete", exception);
            }
        }

        #endregion
    }
}

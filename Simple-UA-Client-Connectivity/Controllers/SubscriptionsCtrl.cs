using Opc.Ua;
using Opc.Ua.Client;
using SimpleUAClientLibrary.Filters;
using System.Collections.Generic;


namespace SimpleUAClientLibrary.Controllers
{
    internal class SubscriptionsCtrl
    {
        #region Private Fields        
        bool m_connectedOnce = false;
        Subscription m_subscription = null;
        private FilterDefinition m_filter;
        private Dictionary<NodeId, NodeId> m_eventTypeMappings;
        private MonitoredItem m_monitoredItem;
        private MonitoredItemNotificationEventHandler m_MonitoredItem_Notification;
        #endregion

        public SubscriptionsCtrl()
        {
            // the filter to use.
            m_filter = new FilterDefinition();

            m_filter.AreaId = ObjectIds.Server;
            m_filter.Severity = EventSeverity.Min;
            m_filter.IgnoreSuppressedOrShelved = true;
            m_filter.EventTypes = new NodeId[] { ObjectTypeIds.ConditionType };
        }

        /// <summary>
        /// Updates the application after connecting to or disconnecting from the server.
        /// </summary>
        internal void Create(Session m_session, string nodeId, string name)
        {
            // check for disconnect.
            if (m_session == null)
            {
                return;
            }

            // set a suitable initial state.
            if (m_session != null && !m_connectedOnce)
            {
                m_connectedOnce = true;
            }

            // create the default subscription.
            m_subscription = new Subscription();

            m_subscription.DisplayName = name;
            m_subscription.PublishingInterval = 1000;
            m_subscription.KeepAliveCount = 10;
            m_subscription.LifetimeCount = 100;
            m_subscription.MaxNotificationsPerPublish = 1000;
            m_subscription.PublishingEnabled = true;
            m_subscription.TimestampsToReturn = TimestampsToReturn.Both;

            m_session.AddSubscription(m_subscription);
            m_subscription.Create();
            
            // standard
            m_monitoredItem = GetMonitoredItem(nodeId, name, MonitoringMode.Reporting);

            // filtered
            //m_monitoredItem = GetMonitoredItemWithFilter(nodeId, m_session)

            //set up callback for notifications.
            m_monitoredItem.Notification += OnMonitoredItemNotificationEvent;

            m_subscription.AddItem(m_monitoredItem);

            m_subscription.ApplyChanges();

            //m_subscription.ConditionRefresh();

        }

        #region Private methods

        private void OnMonitoredItemNotificationEvent(object sender, MonitoredItemNotificationEventArgs e)
        {
            var notification = e.NotificationValue as MonitoredItemNotification;

            if (notification == null)
            {
                return;
            }

            var monitoredItem = sender as MonitoredItem;

            if (monitoredItem != null)
            {
                var message = System.String.Format("Event called for Variable \"{0}\" with Value = {1}.",
                                            monitoredItem.DisplayName, notification.Value);                
                Utils.Trace(message);
            }
        }

        private MonitoredItem GetMonitoredItem(string nodeId, string displayName, MonitoringMode monitoringMode)
        {
            // Create a MonitoredItem 
            MonitoredItem monitoredItem = new MonitoredItem
            {
                StartNodeId = new NodeId(nodeId),
                AttributeId = Attributes.Value,
                DisplayName = displayName,
                MonitoringMode = monitoringMode,
                SamplingInterval = 1000,
                QueueSize = 0,
                DiscardOldest = true
            };

            return monitoredItem;
        }

        //private MonitoredItem GetMonitoredItemWithFilter(string nodeId, Session m_session)
        //{
        //    // must specify the fields that the form is interested in.
        //    m_filter.SelectClauses = m_filter.ConstructSelectClauses(
        //        m_session,
        //        NodeId.Parse(nodeId), // accepts multiple NodeIds
        //        ObjectTypeIds.DialogConditionType,
        //        ObjectTypeIds.ExclusiveLimitAlarmType,
        //        ObjectTypeIds.NonExclusiveLimitAlarmType);

        //    // create a monitored item based on the current filter settings.
        //    return m_filter.CreateMonitoredItem(m_session);
        //}

        #endregion

    }
}

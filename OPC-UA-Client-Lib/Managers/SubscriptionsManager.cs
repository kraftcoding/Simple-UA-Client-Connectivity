using Opc.Ua;
using Opc.Ua.Client;
using SimpleUAClientLibrary.Filters;
using System.Collections.Generic;

namespace SimpleUAClientLibrary.Controllers
{
    internal class SubscriptionsManager
    {
        #region Private Fields        
        bool m_connectedOnce = false;
        private FilterDefinition m_filter;
        private Dictionary<NodeId, NodeId> m_eventTypeMappings;
        private MonitoredItem m_monitoredItem;
        private MonitoredItemNotificationEventHandler m_MonitoredItem_Notification;
        #endregion

        public SubscriptionsManager()
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

        #region Internal methods

        internal Subscription CreateDefault(Session m_session)
        {
            Subscription m_subscription = new Subscription(m_session.DefaultSubscription) { PublishingInterval = 10000 };
            m_session.AddSubscription(m_subscription);

            return m_subscription;
        }

        internal Subscription CreateByDictionary(Session m_session, Dictionary<string, string> subsDictionary)
        {
            // check for disconnect.
            if (m_session == null)
            {
                return null;
            }

            // set a suitable initial state.
            if (m_session != null && !m_connectedOnce)
            {
                m_connectedOnce = true;
            }

            Subscription m_subscription = GetSubscription(m_session, subsDictionary, TimestampsToReturn.Both);
            m_session.AddSubscription(m_subscription);

            return m_subscription;
        }

        internal MonitoredItem GetMonitoredItem(string nodeId, string displayName, MonitoringMode monitoringMode, Session m_session)
        {
            MonitoredItem m_monitoredItem = null;

            if (m_session == null)
            {
                m_monitoredItem = GetMonitoredItem(nodeId, displayName, monitoringMode);
            }
            else
            {
                m_monitoredItem = GetMonitoredItemWithFilter(nodeId, m_session);
            }

            //set up callback for notifications.
            m_monitoredItem.Notification += OnMonitoredItemNotificationEvent;

            return m_monitoredItem;
        }

        #endregion

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

        private Subscription GetSubscription(Session m_session, Dictionary<string, string> subsDictionary, TimestampsToReturn timestampsToReturn)
        {
            // create the default subscription.
            Subscription m_subscription = new Subscription();

            m_subscription.DisplayName = subsDictionary["DisplayName"];
            m_subscription.PublishingInterval = int.Parse(subsDictionary["PublishingInterval"]);
            m_subscription.KeepAliveCount = uint.Parse(subsDictionary["KeepAliveCount"]);
            m_subscription.LifetimeCount = uint.Parse(subsDictionary["LifetimeCount"]);
            m_subscription.MaxNotificationsPerPublish = uint.Parse(subsDictionary["MaxNotificationsPerPublish"]);
            m_subscription.PublishingEnabled = bool.Parse(subsDictionary["PublishingEnabled"]);
            m_subscription.TimestampsToReturn = timestampsToReturn;

            // Adds m_session instance for m_subscription
            m_session.AddSubscription(m_subscription);

            return m_subscription;
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

        private MonitoredItem GetMonitoredItemWithFilter(string nodeId, Session m_session)
        {
            // must specify the fields that the form is interested in.
            m_filter.SelectClauses = m_filter.ConstructSelectClauses(
                m_session,
                NodeId.Parse(nodeId), // accepts multiple NodeIds
                ObjectTypeIds.DialogConditionType,
                ObjectTypeIds.ExclusiveLimitAlarmType,
                ObjectTypeIds.NonExclusiveLimitAlarmType);

            // create a monitored item based on the current filter settings.
            return m_filter.CreateMonitoredItem(m_session);
        }


        #endregion

    }
}

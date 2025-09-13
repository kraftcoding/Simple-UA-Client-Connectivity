using Opc.Ua;
using Opc.Ua.Client;
using System.Linq;

namespace Simple_UA_Client_Connectivity.Controllers
{
    internal class NodeManager
    {
        private NodeId m_rootId;

        public NodeManager() {
            m_rootId = Opc.Ua.ObjectIds.ObjectsFolder;
        }

        #region Internal methods
        internal void PrintRootFolderToLog(Session m_session)
        {
            ReferenceDescriptionCollection refs;
            byte[] bts;
            m_session.Browse(null, null, ObjectIds.ObjectsFolder, 0u, BrowseDirection.Forward, ReferenceTypeIds.HierarchicalReferences, true, (uint)NodeClass.Variable | (uint)NodeClass.Object | (uint)NodeClass.Method, out bts, out refs);
            //Console.WriteLine("\nDisplayName: BrowseName, NodeClass\n");
            Utils.Trace("\nDisplayName: BrowseName, NodeClass\n");
            foreach (var rd in refs)
            {
                //Console.WriteLine("*Object: {0} | NodeId: {1}\n", rd.DisplayName, rd.NodeId);
                Utils.Trace("*Object: {0} | NodeId: {1}\n", rd.DisplayName, rd.NodeId);
                ReferenceDescriptionCollection refs2;
                byte[] bts2;
                m_session.Browse(null, null, ExpandedNodeId.ToNodeId(rd.NodeId, m_session.NamespaceUris), 0u, BrowseDirection.Forward, ReferenceTypeIds.HierarchicalReferences, true, (uint)NodeClass.Variable | (uint)NodeClass.Object | (uint)NodeClass.Method, out bts2, out refs2);
                foreach (var ref2 in refs2)
                {
                    ReferenceDescriptionCollection refs3;
                    byte[] bts3;
                    //Console.WriteLine("\t #Interface/Object: {0} | NodeId: {1}", ref2.DisplayName, ref2.NodeId);
                    //Console.WriteLine("\t ------------------------------");
                    Utils.Trace("\t #Interface/Object: {0} | NodeId: {1}", ref2.DisplayName, ref2.NodeId);
                    Utils.Trace("\t ------------------------------");
                    m_session.Browse(null, null, ExpandedNodeId.ToNodeId(ref2.NodeId, m_session.NamespaceUris), 0u, BrowseDirection.Forward, ReferenceTypeIds.HierarchicalReferences, true, (uint)NodeClass.Variable | (uint)NodeClass.Object | (uint)NodeClass.Method, out bts3, out refs3);
                    foreach (var ref3 in refs3.Where(n => n.DisplayName != "Icon"))
                    {
                        NodeId nodeId = new NodeId(ref3.NodeId.ToString());
                        Utils.Trace("\n\t\t +Node: {0} | NodeId: {1} ", ref3.DisplayName, ref3.NodeId);
                        //Console.WriteLine("\n\t\t +Node: {0} | NodeId: {1} ", ref3.DisplayName, ref3.NodeId);
                        var references = m_session.FetchReferences(nodeId);
                        foreach (var reference in references.Where(r => r.NodeClass.ToString() == "Variable"))
                        {
                            //Console.WriteLine("\t\t\t -Node Array Item: {0} | NodeId: {1}", reference.DisplayName, reference.NodeId);
                            Utils.Trace("\t\t\t -Node Array Item: {0} | NodeId: {1}", reference.DisplayName, reference.NodeId);
                        }
                    }
                    //Console.WriteLine("\n\t ------------------------------ \n");
                    Utils.Trace("\n\t ------------------------------ \n");
                }
            }
        }

        #endregion
    }
}

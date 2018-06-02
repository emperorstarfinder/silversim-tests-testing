// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3 with
// the following clarification and special exception.

// Linking this library statically or dynamically with other modules is
// making a combined work based on this library. Thus, the terms and
// conditions of the GNU Affero General Public License cover the whole
// combination.

// As a special exception, the copyright holders of this library give you
// permission to link this library with independent modules to produce an
// executable, regardless of the license terms of these independent
// modules, and to copy and distribute the resulting executable under
// terms of your choice, provided that you also meet, for each linked
// independent module, the terms and conditions of the license of that
// module. An independent module is a module which is not derived from
// or based on this library. If you modify this library, you may extend
// this exception to your version of the library, but you are not
// obligated to do so. If you do not wish to do so, delete this
// exception statement from your version.

using SilverSim.Scene.Types.Script;
using SilverSim.Scripting.Lsl;
using SilverSim.Tests.Viewer.UDP;
using SilverSim.Viewer.Messages.Object;

namespace SilverSim.Tests.Viewer
{
    public partial class ViewerControlApi
    {
        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendBuyObjectInventory")]
        public void SendBuyObjectInventory(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey objectID,
            LSLKey itemID,
            LSLKey folderID)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit))
                {
                    viewerCircuit.SendMessage(new BuyObjectInventory
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        ObjectID = objectID.AsUUID,
                        ItemID = itemID.AsUUID,
                        FolderID = folderID.AsUUID
                    });
                }
            }
        }

        [APIExtension("ViewerControl")]
        public const int DEREZ_ACTION_SAVE_INTO_AGENT_INVENTORY = 0;
        [APIExtension("ViewerControl")]
        public const int DEREZ_ACTION_TAKE_COPY = 1;
        [APIExtension("ViewerControl")]
        public const int DEREZ_ACTION_SAVE_INTO_TASK_INVENTORY = 2;
        [APIExtension("ViewerControl")]
        public const int DEREZ_ACTION_ATTACHMENT = 3;
        [APIExtension("ViewerControl")]
        public const int DEREZ_ACTION_TAKE = 4;
        [APIExtension("ViewerControl")]
        public const int DEREZ_ACTION_GOD_TAKE_COPY = 5;
        [APIExtension("ViewerControl")]
        public const int DEREZ_ACTION_DELETE_TO_TRASH = 6;
        [APIExtension("ViewerControl")]
        public const int DEREZ_ACTION_ATTACHMENT_TO_INV = 7;
        [APIExtension("ViewerControl")]
        public const int DEREZ_ACTION_ATTACHMENT_EXISTS = 8;
        [APIExtension("ViewerControl")]
        public const int DEREZ_ACTION_RETURN_TO_OWNER = 9;
        [APIExtension("ViewerControl")]
        public const int DEREZ_ACTION_RETURN_TO_LAST_OWNER = 10;

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendDeRezObject")]
        public void SendDeRezObject(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey groupID,
            int derezAction,
            LSLKey destinationID,
            LSLKey transactionID,
            int packetCount,
            int packetNumber)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit))
                {
                    viewerCircuit.SendMessage(new DeRezObject
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        GroupID = groupID.AsUUID,
                        Destination = (DeRezObject.DeRezAction)derezAction,
                        DestinationID = destinationID.AsUUID,
                        TransactionID = transactionID.AsUUID,
                        PacketCount = (byte)packetCount,
                        PacketNumber = (byte)packetNumber
                    });
                }
            }
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendDetachAttachmentIntoInv")]
        public void SendDetachAttachmentIntoInv(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey itemID)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit))
                {
                    viewerCircuit.SendMessage(new DetachAttachmentIntoInv
                    {
                        AgentID = agent.AgentID,
                        ItemID = itemID.AsUUID
                    });
                }
            }
        }
    }
}

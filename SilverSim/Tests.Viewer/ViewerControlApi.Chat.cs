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
using SilverSim.Scripting.Lsl.Api.ByteString;
using SilverSim.Tests.Viewer.UDP;
using SilverSim.Types;
using SilverSim.Types.IM;
using SilverSim.Viewer.Messages;
using SilverSim.Viewer.Messages.Chat;
using SilverSim.Viewer.Messages.IM;

namespace SilverSim.Tests.Viewer
{
    public partial class ViewerControlApi
    {
        [Reliable]
        [Zerocoded]
        private sealed class ChatFromViewer : Message
        {
            public override MessageType Number => MessageType.ChatFromViewer;

            public UUID AgentID;
            public UUID SessionID;
            public string Message;
            public ChatType ChatType;
            public int Channel;

            public override void Serialize(UDPPacket p)
            {
                p.WriteUUID(AgentID);
                p.WriteUUID(SessionID);
                p.WriteStringLen16(Message);
                p.WriteUInt8((byte)ChatType);
                p.WriteInt32(Channel);
            }
        }

        [APIExtension("ViewerControl")]
        public const int CHAT_TYPE_WHISPER = 0;
        [APIExtension("ViewerControl")]
        public const int CHAT_TYPE_SAY = 1;
        [APIExtension("ViewerControl")]
        public const int CHAT_TYPE_SHOUT = 2;
        [APIExtension("ViewerControl")]
        public const int CHAT_TYPE_START_TYPING = 4;
        [APIExtension("ViewerControl")]
        public const int CHAT_TYPE_STOP_TYPING = 5;
        [APIExtension("ViewerControl")]
        public const int CHAT_TYPE_DEBUG_CHANNEL = 6;
        [APIExtension("ViewerControl")]
        public const int CHAT_TYPE_REGION = 7;
        [APIExtension("ViewerControl")]
        public const int CHAT_TYPE_OWNER = 8;
        [APIExtension("ViewerControl")]
        public const int CHAT_TYPE_BROADCAST = 0xFF;

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendChatFromViewer")]
        public void SendChatFromViewer(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            string message,
            int chatType,
            int channel)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    viewerCircuit.SendMessage(new ChatFromViewer
                    {
                        AgentID = viewerCircuit.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        Message = message,
                        ChatType = (ChatType)chatType,
                        Channel = channel
                    });
                }
            }
        }

        [APIExtension("ViewerControl")]
        public const int DIALOG_MESSAGE_FROM_AGENT = 0;
        [APIExtension("ViewerControl")]
        public const int DIALOG_MESSAGE_BOX = 1;
        [APIExtension("ViewerControl")]
        public const int DIALOG_GROUP_INVITATION = 3;
        [APIExtension("ViewerControl")]
        public const int DIALOG_INVENTORY_OFFERED = 4;
        [APIExtension("ViewerControl")]
        public const int DIALOG_INVENTORY_ACCEPTED = 5;
        [APIExtension("ViewerControl")]
        public const int DIALOG_INVENTORY_DECLINED = 6;
        [APIExtension("ViewerControl")]
        public const int DIALOG_GROUP_VOTE = 7;
        [APIExtension("ViewerControl")]
        public const int DIALOG_TASK_INVENTORY_OFFERED = 9;
        [APIExtension("ViewerControl")]
        public const int DIALOG_TASK_INVENTORY_ACCEPTED = 10;
        [APIExtension("ViewerControl")]
        public const int DIALOG_TASK_INVENTORY_DECLINED = 11;
        [APIExtension("ViewerControl")]
        public const int DIALOG_NEW_USER_DEFAULT = 12;
        [APIExtension("ViewerControl")]
        public const int DIALOG_SESION_ADD = 13;
        [APIExtension("ViewerControl")]
        public const int DIALOG_SESSION_OFFLINE_ADD = 14;
        [APIExtension("ViewerControl")]
        public const int DIALOG_SESSION_GROUP_START = 15;
        [APIExtension("ViewerControl")]
        public const int DIALOG_SESSION_CARDLESS_START = 16;
        [APIExtension("ViewerControl")]
        public const int DIALOG_SESSION_SEND = 17;
        [APIExtension("ViewerControl")]
        public const int DIALOG_SESSION_DROP = 18;
        [APIExtension("ViewerControl")]
        public const int DIALOG_MESSAGE_FROM_OBJECT = 19;
        [APIExtension("ViewerControl")]
        public const int DIALOG_BUSY_AUTO_RESPONSE = 20;
        [APIExtension("ViewerControl")]
        public const int DIALOG_CONSOLE_AND_CHAT_HISTORY = 21;
        [APIExtension("ViewerControl")]
        public const int DIALOG_REQUEST_TELEPORT = 22;
        [APIExtension("ViewerControl")]
        public const int DIALOG_ACCEPT_TELEPORT = 23;
        [APIExtension("ViewerControl")]
        public const int DIALOG_DENY_TELEPORT = 24;
        [APIExtension("ViewerControl")]
        public const int DIALOG_GODLIKE_REQUEST_TELEPORT = 25;
        [APIExtension("ViewerControl")]
        public const int DIALOG_REQUEST_LURE = 26;
        [APIExtension("ViewerControl")]
        public const int DIALOG_GOTO_URL = 28;
        [APIExtension("ViewerControl")]
        public const int DIALOG_SESION_911_START = 29;
        [APIExtension("ViewerControl")]
        public const int DIALOG_LURE_911 = 30;
        [APIExtension("ViewerControl")]
        public const int DIALOG_FROM_TASK_AS_ALERT = 31;
        [APIExtension("ViewerControl")]
        public const int DIALOG_GROUP_NOTICE = 32;
        [APIExtension("ViewerControl")]
        public const int DIALOG_GROUP_NOTICE_INVENTORY_ACCEPTED = 33;
        [APIExtension("ViewerControl")]
        public const int DIALOG_GROUP_NOTICE_INVENTORY_DECLINED = 34;
        [APIExtension("ViewerControl")]
        public const int DIALOG_GROUP_INVITATION_ACCEPT = 35;
        [APIExtension("ViewerControl")]
        public const int DIALOG_GROUP_INVITATION_DECLINE = 36;
        [APIExtension("ViewerControl")]
        public const int DIALOG_GROUP_NOTICE_REQUESTED = 37;
        [APIExtension("ViewerControl")]
        public const int DIALOG_FRIENDSHIP_OFFERED = 38;
        [APIExtension("ViewerControl")]
        public const int DIALOG_FRIENDSHIP_ACCEPTED = 39;
        [APIExtension("ViewerControl")]
        public const int DIALOG_FRIENDSHIP_DECLINED = 40;
        [APIExtension("ViewerControl")]
        public const int DIALOG_START_TYPING = 41;
        [APIExtension("ViewerControl")]
        public const int DIALOG_STOP_TYPING = 42;

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendInstantMessage")]
        public void SendInstantMessage(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            int isFromGroup,
            LSLKey toAgent,
            int parentEstateID,
            LSLKey regionID,
            Vector3 position,
            int isOffline,
            int dialog,
            LSLKey id,
            long timestamp,
            string fromAgentName,
            string message,
            ByteArrayApi.ByteArray binaryBucket)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    viewerCircuit.SendMessage(new ImprovedInstantMessage
                    {
                        AgentID = viewerCircuit.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        FromGroup = isFromGroup != 0,
                        ToAgentID = toAgent.AsUUID,
                        ParentEstateID = (uint)parentEstateID,
                        RegionID = regionID.AsUUID,
                        Position = position,
                        IsOffline = isOffline != 0,
                        Dialog = (GridInstantMessageDialog)dialog,
                        ID = id.AsUUID,
                        Timestamp = Date.UnixTimeToDateTime((ulong)timestamp),
                        FromAgentName = fromAgentName,
                        Message = message,
                        BinaryBucket = binaryBucket.Data
                    });
                }
            }
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendRetrieveInstantMessages")]
        public void RetrieveInstantMessages(
            ScriptInstance instance,
            ViewerAgentAccessor agent)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    viewerCircuit.SendMessage(new RetrieveInstantMessages
                    {
                        AgentID = viewerCircuit.AgentID,
                        SessionID = viewerCircuit.SessionID
                    });
                }
            }
        }
    }
}

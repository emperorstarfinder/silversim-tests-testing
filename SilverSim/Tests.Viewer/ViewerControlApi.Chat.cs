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
using SilverSim.Types;
using SilverSim.Viewer.Messages;
using SilverSim.Viewer.Messages.Chat;

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
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit))
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
    }
}

// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using log4net;
using SilverSim.Types;
using SilverSim.Viewer.Core;
using SilverSim.Viewer.Messages;
using SilverSim.Viewer.Messages.IM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SilverSim.Tests.UDP.Common
{
    public class ViewerCircuit : Circuit
    {
        public UUID SessionID { get; protected set; }
        public UUID AgentID { get; protected set; }

        private static readonly ILog m_Log = LogManager.GetLogger("VIEWER CIRCUIT");
        private static readonly UDPPacketDecoder m_PacketDecoder = new UDPPacketDecoder(true);

        public event Action<Message> OnReceivedMessage;

        public ViewerCircuit(
            UDPCircuitsManager server,
            UInt32 circuitcode,
            UUID sessionID,
            UUID agentID)
            : base(server, circuitcode)
        {
            SessionID = sessionID;
            AgentID = agentID;
        }

        protected override void CheckForNewDataToSend()
        {
        }

        public override void Dispose()
        {
        }

        protected override void LogMsgLogoutReply()
        {
        }

        protected override void LogMsgOnLogoutCompletion()
        {
        }

        protected override void LogMsgOnTimeout()
        {
        }

        protected override void OnCircuitSpecificPacketReceived(MessageType mType, UDPPacket pck)
        {
            UDPPacketDecoder.PacketDecoderDelegate del;
            if (m_PacketDecoder.PacketTypes.TryGetValue(mType, out del))
            {
                Message m = del(pck);
                /* we got a decoder, so we can make use of it */
                m.ReceivedOnCircuitCode = CircuitCode;
                m.CircuitAgentID = AgentID;
                try
                {
                    m.CircuitAgentOwner = UUI.Unknown;
                    m.CircuitSessionID = SessionID;
                    m.CircuitSceneID = UUID.Zero;
                }
                catch
                {
                    /* this is a specific error that happens only during logout */
                    return;
                }

                /* we keep the circuit relatively dumb so that we have no other logic than how to send and receive messages to the remote sim.
                    * It merely collects delegates to other objects as well to call specific functions.
                    */
                var ev = OnReceivedMessage;
                if (null != ev)
                {
                    foreach (Action<Message> mdel in ev.GetInvocationList())
                    {
                        mdel.Invoke(m);
                    }
                }
            }
            else
            {
                /* Ignore we have no decoder for that */
            }
        }

        protected override void SendSimStats(int dt)
        {
        }

        protected override void SendViaEventQueueGet(Message m)
        {
        }

        protected override void StartSpecificThreads()
        {
        }

        protected override void StopSpecificThreads()
        {
        }
    }
}

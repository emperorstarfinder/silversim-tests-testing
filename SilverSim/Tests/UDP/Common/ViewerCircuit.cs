// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using log4net;
using SilverSim.Threading;
using SilverSim.Types;
using SilverSim.Viewer.Core;
using SilverSim.Viewer.Messages;
using SilverSim.Viewer.Messages.IM;
using System;
using System.Collections.Generic;

namespace SilverSim.Tests.UDP.Common
{
    public class ViewerCircuit : Circuit
    {
        public UUID SessionID { get; protected set; }
        public UUID AgentID { get; protected set; }

        private static readonly ILog m_Log = LogManager.GetLogger("VIEWER CIRCUIT");
        private static readonly UDPPacketDecoder m_PacketDecoder = new UDPPacketDecoder(true);
        public readonly BlockingQueue<Message> ReceiveQueue = new BlockingQueue<Message>();
        public bool EnableReceiveQueue = false;

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

        protected override void LogMsgLogoutReply()
        {
        }

        protected override void LogMsgOnLogoutCompletion()
        {
        }

        protected override void LogMsgOnTimeout()
        {
        }

        public Dictionary<string, Action<Message>> GenericMessageRouting
        {
            get
            {
                return m_GenericMessageRouting;
            }
        }

        public Dictionary<SilverSim.Types.IM.GridInstantMessageDialog, Action<Message>> IMMessageRouting
        {
            get
            {
                return m_IMMessageRouting;
            }
        }

        public Dictionary<MessageType, Action<Message>> MessageRouting
        {
            get
            {
                return m_MessageRouting;
            }
        }

        public Message Receive(int timeout)
        {
            if (!EnableReceiveQueue)
            {
                throw new Exception("Receive queue not enabled");
            }
            return ReceiveQueue.Dequeue(timeout);
        }

        public Message Receive()
        {
            if(!EnableReceiveQueue)
            {
                throw new Exception("Receive queue not enabled");
            }
            return ReceiveQueue.Dequeue();
        }

        protected override void OnCircuitSpecificPacketReceived(MessageType mType, UDPPacket pck)
        {

            Func<UDPPacket, Message> del;
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
                Action<Message> mdel;
                if (m_MessageRouting.TryGetValue(m.Number, out mdel))
                {
                    mdel(m);
                }
                else if (m.Number == MessageType.ImprovedInstantMessage)
                {
                    ImprovedInstantMessage im = (ImprovedInstantMessage)m;
                    if (im.CircuitAgentID != im.AgentID ||
                        im.CircuitSessionID != im.SessionID)
                    {
                        return;
                    }
                    if (m_IMMessageRouting.TryGetValue(im.Dialog, out mdel))
                    {
                        mdel(m);
                    }
                    else if (EnableReceiveQueue)
                    {
                        ReceiveQueue.Enqueue(m);
                    }
                }
                else if (m.Number == MessageType.GenericMessage)
                {
                    SilverSim.Viewer.Messages.Generic.GenericMessage genMsg = (SilverSim.Viewer.Messages.Generic.GenericMessage)m;
                    if (m_GenericMessageRouting.TryGetValue(genMsg.Method, out mdel))
                    {
                        mdel(m);
                    }
                    else if (EnableReceiveQueue)
                    {
                        ReceiveQueue.Enqueue(m);
                    }
                }
                else if (EnableReceiveQueue)
                {
                    ReceiveQueue.Enqueue(m);
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

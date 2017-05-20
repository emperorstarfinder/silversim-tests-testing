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

using log4net;
using SilverSim.Threading;
using SilverSim.Types;
using SilverSim.Types.Estate;
using SilverSim.Types.Grid;
using SilverSim.Types.IM;
using SilverSim.Viewer.Core;
using SilverSim.Viewer.Messages;
using SilverSim.Viewer.Messages.IM;
using SilverSim.Viewer.Messages.Region;
using System;
using System.Collections.Generic;
using System.Net;
using SilverSim.Viewer.Messages.Generic;

namespace SilverSim.Tests.Viewer.UDP
{
    public class ViewerCircuit : Circuit
    {
        public UUID SessionID { get; protected set; }
        public UUID AgentID { get; protected set; }

        private static readonly ILog m_Log = LogManager.GetLogger("VIEWER CIRCUIT");
        private static readonly UDPPacketDecoder m_PacketDecoder = new UDPPacketDecoder(true);
        public readonly BlockingQueue<Message> ReceiveQueue = new BlockingQueue<Message>();
        public bool EnableReceiveQueue;

        readonly Dictionary<MessageType, Action<Message>> m_MessageRouting = new Dictionary<MessageType, Action<Message>>();
        readonly Dictionary<string, Action<Message>> m_GenericMessageRouting = new Dictionary<string, Action<Message>>();
        readonly Dictionary<string, Action<Message>> m_GodlikeMessageRouting = new Dictionary<string, Action<Message>>();
        readonly Dictionary<GridInstantMessageDialog, Action<Message>> m_IMMessageRouting = new Dictionary<GridInstantMessageDialog, Action<Message>>();

        public class RegionHandshakeData
        {
            public RegionOptionFlags RegionFlags;
            public RegionAccess SimAccess;
            public string SimName = string.Empty;
            public UUID SimOwner = UUID.Zero;
            public bool IsEstateManager;
            public double WaterHeight;
            public double BillableFactor;
            public UUID CacheID = UUID.Zero;
            public UUID TerrainBase0 = UUID.Zero;
            public UUID TerrainBase1 = UUID.Zero;
            public UUID TerrainBase2 = UUID.Zero;
            public UUID TerrainBase3 = UUID.Zero;
            public UUID TerrainDetail0 = UUID.Zero;
            public UUID TerrainDetail1 = UUID.Zero;
            public UUID TerrainDetail2 = UUID.Zero;
            public UUID TerrainDetail3 = UUID.Zero;
            public double TerrainStartHeight00;
            public double TerrainStartHeight01;
            public double TerrainStartHeight10;
            public double TerrainStartHeight11;
            public double TerrainHeightRange00;
            public double TerrainHeightRange01;
            public double TerrainHeightRange10;
            public double TerrainHeightRange11;

            public UUID RegionID = UUID.Zero;

            public Int32 CPUClassID;
            public Int32 CPURatio;
            public string ColoName = string.Empty;
            public string ProductSKU = string.Empty;
            public string ProductName = string.Empty;

            public List<RegionHandshake.RegionExtDataEntry> RegionExtData = new List<RegionHandshake.RegionExtDataEntry>();

            public RegionHandshakeData()
            {

            }

            public RegionHandshakeData(RegionHandshake msg)
            {
                RegionFlags = msg.RegionFlags;
                SimAccess = msg.SimAccess;
                SimName = msg.SimName;
                IsEstateManager = msg.IsEstateManager;
                WaterHeight = msg.WaterHeight;
                BillableFactor = msg.BillableFactor;
                CacheID = msg.CacheID;
                TerrainBase0 = msg.TerrainBase0;
                TerrainBase1 = msg.TerrainBase1;
                TerrainBase2 = msg.TerrainBase2;
                TerrainBase3 = msg.TerrainBase3;
                TerrainDetail0 = msg.TerrainDetail0;
                TerrainDetail1 = msg.TerrainDetail1;
                TerrainDetail2 = msg.TerrainDetail2;
                TerrainDetail3 = msg.TerrainDetail3;
                TerrainHeightRange00 = msg.TerrainHeightRange00;
                TerrainHeightRange01 = msg.TerrainHeightRange01;
                TerrainHeightRange10 = msg.TerrainHeightRange10;
                TerrainHeightRange11 = msg.TerrainHeightRange11;
                TerrainStartHeight00 = msg.TerrainStartHeight00;
                TerrainStartHeight01 = msg.TerrainStartHeight01;
                TerrainStartHeight10 = msg.TerrainStartHeight10;
                TerrainStartHeight11 = msg.TerrainStartHeight11;
                RegionID = msg.RegionID;
                CPUClassID = msg.CPUClassID;
                CPURatio = msg.CPURatio;
                ColoName = msg.ColoName;
                ProductSKU = msg.ProductSKU;
                ProductName = msg.ProductName;
                RegionExtData = msg.RegionExtData;
            }
        }

        public RegionHandshakeData RegionData { get; private set; }

        public ViewerCircuit(
            UDPCircuitsManager server,
            UInt32 circuitcode,
            UUID sessionID,
            UUID agentID,
            EndPoint remoteEndPoint)
            : base(server, circuitcode)
        {
            RegionData = new RegionHandshakeData();
            RemoteEndPoint = remoteEndPoint;
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

        public Dictionary<string, Action<Message>> GenericMessageRouting => m_GenericMessageRouting;

        public Dictionary<GridInstantMessageDialog, Action<Message>> IMMessageRouting => m_IMMessageRouting;

        public Dictionary<MessageType, Action<Message>> MessageRouting => m_MessageRouting;

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

                if(m.Number == MessageType.RegionHandshake)
                {
                    RegionData = new RegionHandshakeData((RegionHandshake)m);
                    var reply = new RegionHandshakeReply()
                    {
                        SessionID = SessionID,
                        AgentID = AgentID,
                        Flags = 0
                    };
                    SendMessage(reply);
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
                    var im = (ImprovedInstantMessage)m;
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
                    var genMsg = (GenericMessage)m;
                    if (m_GenericMessageRouting.TryGetValue(genMsg.Method, out mdel))
                    {
                        mdel(m);
                    }
                    else if (EnableReceiveQueue)
                    {
                        ReceiveQueue.Enqueue(m);
                    }
                }
                else if (m.Number == MessageType.GodlikeMessage)
                {
                    var genMsg = (GodlikeMessage)m;
                    if (m_GodlikeMessageRouting.TryGetValue(genMsg.Method, out mdel))
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

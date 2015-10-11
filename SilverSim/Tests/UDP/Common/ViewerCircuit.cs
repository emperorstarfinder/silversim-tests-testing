// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using SilverSim.Types;
using SilverSim.Viewer.Core;
using SilverSim.Viewer.Messages;
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

        protected override void OnCircuitSpecificPacketReceived(MessageType mType, UDPPacket p)
        {
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

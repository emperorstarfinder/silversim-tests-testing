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
using SilverSim.Scene.Types.Script.Events;
using SilverSim.Scripting.Lsl;
using SilverSim.Tests.Viewer.UDP;
using SilverSim.Types;
using SilverSim.Viewer.Messages.Estate;
using SilverSim.Viewer.Messages.Generic;
using SilverSim.Viewer.Messages.God;
using System;

namespace SilverSim.Tests.Viewer
{
    public partial class ViewerControlApi
    {
        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction, "SendEstateCovenantRequest")]
        public void SendEstateCovenantRequest(
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
                    viewerCircuit.SendMessage(new EstateCovenantRequest
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID
                    });
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction, "SendSimWideDeletes")]
        public void SendSimWideDeletes(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey targetID,
            int flags)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    viewerCircuit.SendMessage(new SimWideDeletes
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        TargetID = targetID.AsUUID,
                        Flags = (SimWideDeletes.DeleteFlags)flags
                    });
                }
            }
        }

        private void HandleEstateOwnerMessage(EstateOwnerMessage m, ViewerConnection vc, ViewerAgentAccessor agent)
        {
            try
            {
                switch (m.Method)
                {
                    case "setaccess":
                        break;

                    case "estateupdateinfo":
                        TranslateEstateOwnerMessageEstateUpdateInfo(m, vc, agent);
                        break;

                    case "setexperience":
                        TranslateEstateOwnerMessageSetExperience(m, vc, agent);
                        break;
                }
            }
            catch(Exception e)
            {
                m_Log.Error("EstateOwnerMessage decode failed", e);
            }
        }

        #region estateupdateinfo
        private void TranslateEstateOwnerMessageEstateUpdateInfo(EstateOwnerMessage m, ViewerConnection vc, ViewerAgentAccessor agent)
        {
            if (m.ParamList.Count < 10)
            {
                return;
            }

            vc.PostEvent(new EstateOwnerMessageEstateUpdateInfoReceivedEvent
            {
                Agent = agent,
                EstateName = m.ParamList[0].FromUTF8Bytes(),
                EstateOwner = UUID.Parse(m.ParamList[1].FromUTF8Bytes()),
                EstateID = int.Parse(m.ParamList[2].FromUTF8Bytes()),
                Flags = (int)uint.Parse(m.ParamList[3].FromUTF8Bytes()),
                SunPosition = int.Parse(m.ParamList[4].FromUTF8Bytes()) / 1024.0 - 6,
                ParentEstateID = int.Parse(m.ParamList[5].FromUTF8Bytes()),
                CovenantID = UUID.Parse(m.ParamList[6].FromUTF8Bytes()),
                CovenantTimestamp = (long)ulong.Parse(m.ParamList[7].FromUTF8Bytes()),
                SendToAgentOnly = int.Parse(m.ParamList[8].FromUTF8Bytes()),
                AbuseEmail = m.ParamList[9].FromUTF8Bytes()
            });
        }

        [TranslatedScriptEvent("estateownermessage_estateupdateinfo_received")]
        public class EstateOwnerMessageEstateUpdateInfoReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public string EstateName;
            [TranslatedScriptEventParameter(2)]
            public LSLKey EstateOwner;
            [TranslatedScriptEventParameter(3)]
            public int EstateID;
            [TranslatedScriptEventParameter(4)]
            public int Flags;
            [TranslatedScriptEventParameter(5)]
            public double SunPosition;
            [TranslatedScriptEventParameter(6)]
            public int ParentEstateID;
            [TranslatedScriptEventParameter(7)]
            public LSLKey CovenantID;
            [TranslatedScriptEventParameter(8)]
            public long CovenantTimestamp;
            [TranslatedScriptEventParameter(9)]
            public int SendToAgentOnly;
            [TranslatedScriptEventParameter(10)]
            public string AbuseEmail;
        }

        [APIExtension(ExtensionName, "estateownermessage_estateupdateinfo_received")]
        [StateEventDelegate]
        public delegate void EstateOwnerMessageEstateUpdateInfoReceived(
            ViewerAgentAccessor agent,
            string estateName,
            LSLKey estateOwner,
            int estateID,
            int Flags,
            double SunPosition,
            int ParentEstateID,
            LSLKey covenantID,
            long covenantTimestamp,
            int sendToAgentOnly,
            string abuseEmail);
        #endregion

        #region setexperience
        private void TranslateEstateOwnerMessageSetExperience(EstateOwnerMessage m, ViewerConnection vc, ViewerAgentAccessor agent)
        {
            if (m.ParamList.Count < 5)
            {
                return;
            }
            var ev = new EstateOwnerMessageSetExperienceReceivedEvent
            {
                Agent = agent,
                TransactionID = m.TransactionID,
                InvoiceID = m.Invoice,
            };

            int blockedCount;
            int trustedCount;
            int allowedCount;
            if(!int.TryParse(m.ParamList[0].FromUTF8Bytes(), out ev.EstateID) ||
                !int.TryParse(m.ParamList[2].FromUTF8Bytes(), out blockedCount) ||
                !int.TryParse(m.ParamList[3].FromUTF8Bytes(), out trustedCount) ||
                !int.TryParse(m.ParamList[4].FromUTF8Bytes(), out allowedCount))
            {
                return;
            }
            if(m.ParamList.Count < 5 + blockedCount + trustedCount + allowedCount)
            {
                return;
            }

            int offset = 5;
            while(blockedCount-- > 0)
            {
                ev.Blocked.Add(new LSLKey(new UUID(m.ParamList[offset++], 0)));
            }
            while (trustedCount-- > 0)
            {
                ev.Trusted.Add(new LSLKey(new UUID(m.ParamList[offset++], 0)));
            }
            while (allowedCount-- > 0)
            {
                ev.Allowed.Add(new LSLKey(new UUID(m.ParamList[offset++], 0)));
            }

            vc.PostEvent(ev);
        }

        [TranslatedScriptEvent("estateownermessage_setexperience_received")]
        public class EstateOwnerMessageSetExperienceReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey TransactionID;
            [TranslatedScriptEventParameter(2)]
            public LSLKey InvoiceID;
            [TranslatedScriptEventParameter(3)]
            public int EstateID;
            [TranslatedScriptEventParameter(4)]
            public AnArray Allowed;
            [TranslatedScriptEventParameter(5)]
            public AnArray Blocked;
            [TranslatedScriptEventParameter(6)]
            public AnArray Trusted;
        }

        [APIExtension(ExtensionName, "estateownermessage_setexperience_received")]
        [StateEventDelegate]
        public delegate void EstateOwnerMessageSetExperienceReceived(
            ViewerAgentAccessor agent,
            int estateID,
            AnArray allowed,
            AnArray blocked,
            AnArray trusted);
        #endregion
    }
}

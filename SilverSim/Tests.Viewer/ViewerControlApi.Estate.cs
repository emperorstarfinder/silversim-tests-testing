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
using System.Globalization;

namespace SilverSim.Tests.Viewer
{
    public partial class ViewerControlApi
    {
        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
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

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
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

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendEstateChangeCovenantID(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey id)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    var msg = new EstateOwnerMessage
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        Method = "estatechangecovenantid"
                    };
                    msg.ParamList.Add(id.AsUUID.GetBytes());
                    viewerCircuit.SendMessage(msg);
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendEstateGetInfo(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey transactionID,
            LSLKey invoice)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    var msg = new EstateOwnerMessage
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        Method = "getinfo",
                        TransactionID = transactionID,
                        Invoice = invoice
                    };
                    viewerCircuit.SendMessage(msg);
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendEstateSetRegionTerrain(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey transactionID,
            LSLKey invoice,
            double waterHeight,
            double terrainRaiseLimit,
            double terrainLowerLimit,
            int isSunFixed,
            double sunPosition,
            int useEstateSun)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    var msg = new EstateOwnerMessage
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        Method = "setregionterrain",
                        TransactionID = transactionID,
                        Invoice = invoice
                    };
                    msg.ParamList.Add(waterHeight.ToString(CultureInfo.InvariantCulture).ToUTF8Bytes());
                    msg.ParamList.Add(terrainRaiseLimit.ToString(CultureInfo.InvariantCulture).ToUTF8Bytes());
                    msg.ParamList.Add(terrainLowerLimit.ToString(CultureInfo.InvariantCulture).ToUTF8Bytes());
                    msg.ParamList.Add(useEstateSun.Clamp(0, 1).ToString().ToUTF8Bytes());
                    msg.ParamList.Add(isSunFixed.Clamp(0, 1).ToString().ToUTF8Bytes());
                    msg.ParamList.Add(sunPosition.ToString(CultureInfo.InvariantCulture).ToUTF8Bytes());
                    msg.ParamList.Add("0".ToUTF8Bytes());
                    msg.ParamList.Add("0".ToUTF8Bytes());
                    msg.ParamList.Add("6".ToUTF8Bytes());
                    viewerCircuit.SendMessage(msg);
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendRegionRestart(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            double timeToRestart)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    var msg = new EstateOwnerMessage
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        Method = "restart"
                    };
                    msg.ParamList.Add(timeToRestart.ToString(CultureInfo.InvariantCulture).ToUTF8Bytes());
                    viewerCircuit.SendMessage(msg);
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendEstateSimulatorMessage(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            string message)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    var msg = new EstateOwnerMessage
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        Method = "simulatormessage"
                    };
                    /* first four are unused for that case */
                    msg.ParamList.Add(string.Empty.ToUTF8Bytes());
                    msg.ParamList.Add(string.Empty.ToUTF8Bytes());
                    msg.ParamList.Add(string.Empty.ToUTF8Bytes());
                    msg.ParamList.Add(string.Empty.ToUTF8Bytes());
                    msg.ParamList.Add(message.ToUTF8Bytes());
                    viewerCircuit.SendMessage(msg);
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendEstateInstantMessage(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            string message)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    var msg = new EstateOwnerMessage
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        Method = "instantmessage"
                    };
                    /* first four are unused for that case */
                    msg.ParamList.Add(string.Empty.ToUTF8Bytes());
                    msg.ParamList.Add(string.Empty.ToUTF8Bytes());
                    msg.ParamList.Add(string.Empty.ToUTF8Bytes());
                    msg.ParamList.Add(string.Empty.ToUTF8Bytes());
                    msg.ParamList.Add(message.ToUTF8Bytes());
                    viewerCircuit.SendMessage(msg);
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendSetRegionDebug(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            int disableScripts,
            int disableCollision,
            int disablePhysics)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    var msg = new EstateOwnerMessage
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        Method = "setregiondebug"
                    };
                    msg.ParamList.Add(disableScripts.Clamp(0, 1).ToString().ToUTF8Bytes());
                    msg.ParamList.Add(disableCollision.Clamp(0, 1).ToString().ToUTF8Bytes());
                    msg.ParamList.Add(disablePhysics.Clamp(0, 1).ToString().ToUTF8Bytes());
                    viewerCircuit.SendMessage(msg);
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendSetRegionInfo(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            int blockTerraform,
            int blockFly,
            int allowDamage,
            int allowLandResell,
            int agentLimit,
            double objectBonus,
            int access,
            int restrictPushing,
            int allowLandJoinDivide)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    var msg = new EstateOwnerMessage
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        Method = "setregioninfo"
                    };
                    msg.ParamList.Add(blockTerraform.Clamp(0, 1).ToString().ToUTF8Bytes());
                    msg.ParamList.Add(blockFly.Clamp(0, 1).ToString().ToUTF8Bytes());
                    msg.ParamList.Add(allowDamage.Clamp(0, 1).ToString().ToUTF8Bytes());
                    msg.ParamList.Add(allowLandResell.Clamp(0, 1).ToString().ToUTF8Bytes());
                    msg.ParamList.Add(agentLimit.ToString().ToUTF8Bytes());
                    msg.ParamList.Add(string.Format(CultureInfo.InvariantCulture, "{0}", objectBonus).ToUTF8Bytes());
                    msg.ParamList.Add(access.ToString().ToUTF8Bytes());
                    msg.ParamList.Add(restrictPushing.ToString().ToUTF8Bytes());
                    msg.ParamList.Add(allowLandJoinDivide.ToString().ToUTF8Bytes());
                    viewerCircuit.SendMessage(msg);
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendEstateChangeInfo(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            string estateName,
            int estateFlags,
            double sunPos)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    var msg = new EstateOwnerMessage
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        Method = "estatechangeinfo"
                    };
                    uint actSunPos = (uint)((sunPos + 6) * 1024.0);
                    msg.ParamList.Add(estateName.ToUTF8Bytes());
                    msg.ParamList.Add(((uint)estateFlags).ToString().ToUTF8Bytes());
                    msg.ParamList.Add(actSunPos.ToString().ToUTF8Bytes());
                    viewerCircuit.SendMessage(msg);
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendEstateAccessAddUser(ScriptInstance instance, ViewerAgentAccessor agent, LSLKey transactionid, LSLKey invoice, int allEstates, LSLKey user)
        {
            EstateAccessDeltaFlags flags = EstateAccessDeltaFlags.AddUser;
            if(allEstates != 0)
            {
                flags |= EstateAccessDeltaFlags.AllEstates;
            }
            SendEstateAccess(instance, agent, transactionid, invoice, flags, user);
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendEstateAccessRemoveUser(ScriptInstance instance, ViewerAgentAccessor agent, LSLKey transactionid, LSLKey invoice, int allEstates, LSLKey user)
        {
            EstateAccessDeltaFlags flags = EstateAccessDeltaFlags.RemoveUser;
            if (allEstates != 0)
            {
                flags |= EstateAccessDeltaFlags.AllEstates;
            }
            SendEstateAccess(instance, agent, transactionid, invoice, flags, user);
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendEstateAccessAddGroup(ScriptInstance instance, ViewerAgentAccessor agent, LSLKey transactionid, LSLKey invoice, int allEstates, LSLKey user)
        {
            EstateAccessDeltaFlags flags = EstateAccessDeltaFlags.AddGroup;
            if (allEstates != 0)
            {
                flags |= EstateAccessDeltaFlags.AllEstates;
            }
            SendEstateAccess(instance, agent, transactionid, invoice, flags, user);
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendEstateAccessRemoveGroup(ScriptInstance instance, ViewerAgentAccessor agent, LSLKey transactionid, LSLKey invoice, int allEstates, LSLKey user)
        {
            EstateAccessDeltaFlags flags = EstateAccessDeltaFlags.RemoveGroup;
            if (allEstates != 0)
            {
                flags |= EstateAccessDeltaFlags.AllEstates;
            }
            SendEstateAccess(instance, agent, transactionid, invoice, flags, user);
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendEstateAccessAddManager(ScriptInstance instance, ViewerAgentAccessor agent, LSLKey transactionid, LSLKey invoice, int allEstates, LSLKey user)
        {
            EstateAccessDeltaFlags flags = EstateAccessDeltaFlags.AddManager;
            if (allEstates != 0)
            {
                flags |= EstateAccessDeltaFlags.AllEstates;
            }
            SendEstateAccess(instance, agent, transactionid, invoice, flags, user);
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendEstateAccessRemoveManager(ScriptInstance instance, ViewerAgentAccessor agent, LSLKey transactionid, LSLKey invoice, int allEstates, LSLKey user)
        {
            EstateAccessDeltaFlags flags = EstateAccessDeltaFlags.RemoveManager;
            if (allEstates != 0)
            {
                flags |= EstateAccessDeltaFlags.AllEstates;
            }
            SendEstateAccess(instance, agent, transactionid, invoice, flags, user);
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendEstateAccessAddBan(ScriptInstance instance, ViewerAgentAccessor agent, LSLKey transactionid, LSLKey invoice, int allEstates, LSLKey user)
        {
            EstateAccessDeltaFlags flags = EstateAccessDeltaFlags.AddBan;
            if (allEstates != 0)
            {
                flags |= EstateAccessDeltaFlags.AllEstates;
            }
            SendEstateAccess(instance, agent, transactionid, invoice, flags, user);
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendEstateAccessRemoveBan(ScriptInstance instance, ViewerAgentAccessor agent, LSLKey transactionid, LSLKey invoice, int allEstates, LSLKey user)
        {
            EstateAccessDeltaFlags flags = EstateAccessDeltaFlags.RemoveBan;
            if (allEstates != 0)
            {
                flags |= EstateAccessDeltaFlags.AllEstates;
            }
            SendEstateAccess(instance, agent, transactionid, invoice, flags, user);
        }

        private void SendEstateAccess(ScriptInstance instance, ViewerAgentAccessor agent, LSLKey transactionid, LSLKey invoice, EstateAccessDeltaFlags flags, LSLKey user)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    var msg = new EstateOwnerMessage
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        Method = "estateaccessdelta",
                        TransactionID = transactionid,
                        Invoice = invoice
                    };
                    msg.ParamList.Add("0".ToUTF8Bytes());
                    msg.ParamList.Add(((uint)flags).ToString().ToUTF8Bytes());
                    msg.ParamList.Add(user.AsUUID.ToString().ToUTF8Bytes());
                    viewerCircuit.SendMessage(msg);
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendEstateExperienceAccessAddAllowed(ScriptInstance instance, ViewerAgentAccessor agent, LSLKey transactionid, LSLKey invoice, int allEstates, LSLKey experience)
        {
            EstateExperienceDeltaFlags flags = EstateExperienceDeltaFlags.AddAllowed;
            if (allEstates != 0)
            {
                flags |= EstateExperienceDeltaFlags.AllEstates;
            }
            SendEstateExperienceAccess(instance, agent, transactionid, invoice, flags, experience);
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendEstateExperienceAccessRemoveAllowed(ScriptInstance instance, ViewerAgentAccessor agent, LSLKey transactionid, LSLKey invoice, int allEstates, LSLKey experience)
        {
            EstateExperienceDeltaFlags flags = EstateExperienceDeltaFlags.RemoveAllowed;
            if (allEstates != 0)
            {
                flags |= EstateExperienceDeltaFlags.AllEstates;
            }
            SendEstateExperienceAccess(instance, agent, transactionid, invoice, flags, experience);
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendEstateExperienceAccessAddBlocked(ScriptInstance instance, ViewerAgentAccessor agent, LSLKey transactionid, LSLKey invoice, int allEstates, LSLKey experience)
        {
            EstateExperienceDeltaFlags flags = EstateExperienceDeltaFlags.AddBlocked;
            if (allEstates != 0)
            {
                flags |= EstateExperienceDeltaFlags.AllEstates;
            }
            SendEstateExperienceAccess(instance, agent, transactionid, invoice, flags, experience);
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendEstateExperienceAccessRemoveBlocked(ScriptInstance instance, ViewerAgentAccessor agent, LSLKey transactionid, LSLKey invoice, int allEstates, LSLKey experience)
        {
            EstateExperienceDeltaFlags flags = EstateExperienceDeltaFlags.RemoveBlocked;
            if (allEstates != 0)
            {
                flags |= EstateExperienceDeltaFlags.AllEstates;
            }
            SendEstateExperienceAccess(instance, agent, transactionid, invoice, flags, experience);
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendEstateExperienceAccessAddTrusted(ScriptInstance instance, ViewerAgentAccessor agent, LSLKey transactionid, LSLKey invoice, int allEstates, LSLKey experience)
        {
            EstateExperienceDeltaFlags flags = EstateExperienceDeltaFlags.AddTrusted;
            if (allEstates != 0)
            {
                flags |= EstateExperienceDeltaFlags.AllEstates;
            }
            SendEstateExperienceAccess(instance, agent, transactionid, invoice, flags, experience);
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendEstateExperienceAccessRemoveTrusted(ScriptInstance instance, ViewerAgentAccessor agent, LSLKey transactionid, LSLKey invoice, int allEstates, LSLKey experience)
        {
            EstateExperienceDeltaFlags flags = EstateExperienceDeltaFlags.RemoveTrusted;
            if (allEstates != 0)
            {
                flags |= EstateExperienceDeltaFlags.AllEstates;
            }
            SendEstateExperienceAccess(instance, agent, transactionid, invoice, flags, experience);
        }

        private void SendEstateExperienceAccess(ScriptInstance instance, ViewerAgentAccessor agent, LSLKey transactionid, LSLKey invoice, EstateExperienceDeltaFlags flags, LSLKey experience)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    var msg = new EstateOwnerMessage
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        Method = "estateexperiencedelta",
                        TransactionID = transactionid,
                        Invoice = invoice
                    };
                    msg.ParamList.Add("0".ToUTF8Bytes());
                    msg.ParamList.Add(((uint)flags).ToString().ToUTF8Bytes());
                    msg.ParamList.Add(experience.AsUUID.ToString().ToUTF8Bytes());
                    viewerCircuit.SendMessage(msg);
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
                        TranslateEstateOwnerMessageSetAccess(m, vc, agent);
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

        #region setaccess
        private void TranslateEstateOwnerMessageSetAccess(EstateOwnerMessage m, ViewerConnection vc, ViewerAgentAccessor agent)
        {
            if(m.ParamList.Count < 6)
            {
                return;
            }
            var res = new EstateOwnerMessageSetAccessReceivedEvent
            {
                Agent = agent,
                TransactionID = m.TransactionID,
                Invoice = m.Invoice
            };

            int offset = 6;
            int allowedAgents = int.Parse(m.ParamList[2].FromUTF8Bytes());
            int allowedGroups = int.Parse(m.ParamList[3].FromUTF8Bytes());
            int bannedAgents = int.Parse(m.ParamList[4].FromUTF8Bytes());
            int managers = int.Parse(m.ParamList[5].FromUTF8Bytes());

            while(allowedAgents-- > 0)
            {
                res.AllowedAgents.Add(new LSLKey(new UUID(m.ParamList[offset++], 0)));
            }
            while (allowedGroups-- > 0)
            {
                res.AllowedGroups.Add(new LSLKey(new UUID(m.ParamList[offset++], 0)));
            }
            while (bannedAgents-- > 0)
            {
                res.BannedAgents.Add(new LSLKey(new UUID(m.ParamList[offset++], 0)));
            }
            while (managers-- > 0)
            {
                res.Managers.Add(new LSLKey(new UUID(m.ParamList[offset++], 0)));
            }
            vc.PostEvent(res);
        }

        [TranslatedScriptEvent("estateownermessage_setaccess_received")]
        public class EstateOwnerMessageSetAccessReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey TransactionID;
            [TranslatedScriptEventParameter(2)]
            public LSLKey Invoice;
            [TranslatedScriptEventParameter(3)]
            public int EstateID;
            [TranslatedScriptEventParameter(4)]
            public int Flags;
            [TranslatedScriptEventParameter(5)]
            public AnArray AllowedAgents = new AnArray();
            [TranslatedScriptEventParameter(6)]
            public AnArray AllowedGroups = new AnArray();
            [TranslatedScriptEventParameter(7)]
            public AnArray BannedAgents = new AnArray();
            [TranslatedScriptEventParameter(8)]
            public AnArray Managers = new AnArray();
        }

        [APIExtension(ExtensionName, "estateownermessage_setaccess_received")]
        [StateEventDelegate]
        public delegate void EstateOwnerMessageSetAccessReceived(
            ViewerAgentAccessor agent,
            LSLKey transactionID,
            LSLKey invoice,
            int estateID,
            int flags,
            int remaining,
            AnArray list);
        #endregion

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
                TransactionID = m.TransactionID,
                Invoice = m.Invoice,
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
            public LSLKey TransactionID;
            [TranslatedScriptEventParameter(2)]
            public LSLKey Invoice;
            [TranslatedScriptEventParameter(3)]
            public string EstateName;
            [TranslatedScriptEventParameter(4)]
            public LSLKey EstateOwner;
            [TranslatedScriptEventParameter(5)]
            public int EstateID;
            [TranslatedScriptEventParameter(6)]
            public int Flags;
            [TranslatedScriptEventParameter(7)]
            public double SunPosition;
            [TranslatedScriptEventParameter(8)]
            public int ParentEstateID;
            [TranslatedScriptEventParameter(9)]
            public LSLKey CovenantID;
            [TranslatedScriptEventParameter(10)]
            public long CovenantTimestamp;
            [TranslatedScriptEventParameter(11)]
            public int SendToAgentOnly;
            [TranslatedScriptEventParameter(12)]
            public string AbuseEmail;
        }

        [APIExtension(ExtensionName, "estateownermessage_estateupdateinfo_received")]
        [StateEventDelegate]
        public delegate void EstateOwnerMessageEstateUpdateInfoReceived(
            ViewerAgentAccessor agent,
            LSLKey transactionID,
            LSLKey invoice,
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
                Invoice = m.Invoice,
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
            public LSLKey Invoice;
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
            LSLKey transactionID,
            LSLKey invoice,
            int estateID,
            AnArray allowed,
            AnArray blocked,
            AnArray trusted);
        #endregion
    }
}

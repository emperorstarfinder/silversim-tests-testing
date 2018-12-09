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

using SilverSim.Scene.Types.Script.Events;
using SilverSim.Scripting.Lsl;
using SilverSim.Scripting.Lsl.Api.ByteString;
using SilverSim.Types;
using SilverSim.Viewer.Messages;
using SilverSim.Viewer.Messages.Agent;
using SilverSim.Viewer.Messages.Alert;
using SilverSim.Viewer.Messages.Avatar;
using SilverSim.Viewer.Messages.Camera;
using SilverSim.Viewer.Messages.Chat;
using SilverSim.Viewer.Messages.Common;
using SilverSim.Viewer.Messages.Economy;
using SilverSim.Viewer.Messages.Estate;
using SilverSim.Viewer.Messages.Friend;
using SilverSim.Viewer.Messages.God;
using SilverSim.Viewer.Messages.IM;
using SilverSim.Viewer.Messages.Names;
using SilverSim.Viewer.Messages.Object;
using SilverSim.Viewer.Messages.Region;
using SilverSim.Viewer.Messages.Script;
using SilverSim.Viewer.Messages.Sound;
using SilverSim.Viewer.Messages.Telehub;
using SilverSim.Viewer.Messages.Teleport;
using System;
using System.ComponentModel;

namespace SilverSim.Tests.Viewer
{
    public partial class ViewerControlApi
    {
        [APIExtension("ViewerControl", "agentinfo")]
        [APIDisplayName("agentinfo")]
        [APIAccessibleMembers]
        public sealed class AgentInfo
        {
            public LSLKey AgentID { get; }
            public int CircuitCode { get; }
            public LSLKey RegionID { get; }

            public AgentInfo(Message m, uint circuitCode)
            {
                AgentID = m.CircuitAgentID;
                CircuitCode = (int)circuitCode;
                RegionID = m.CircuitSceneID;
            }

            public AgentInfo(UUID agentID, UUID regionID, uint circuitCode)
            {
                AgentID = agentID;
                RegionID = regionID;
                CircuitCode = (int)circuitCode;
            }
        }

        #region regioninfo_received event
        [APIExtension("ViewerControl", "regioninfodata")]
        [APIDisplayName("regioninfodata")]
        [APIIsVariableType]
        [APIAccessibleMembers]
        public class RegionInfoData
        {
            private readonly RegionInfo m_Msg;

            public RegionInfoData(RegionInfo m)
            {
                m_Msg = m;
            }

            public string SimName => m_Msg.SimName;
            public int EstateID => (int)m_Msg.EstateID;
            public int ParentEstateID => (int)m_Msg.ParentEstateID;
            public int RegionFlags => (int)m_Msg.RegionFlags;
            public int SimAccess => (int)m_Msg.SimAccess;
            public int MaxAgents => (int)m_Msg.MaxAgents;
            public double BillableFactor => m_Msg.BillableFactor;
            public double ObjectBonusFactor => m_Msg.ObjectBonusFactor;
            public double WaterHeight => m_Msg.WaterHeight;
            public double TerrainRaiseLimit => m_Msg.TerrainRaiseLimit;
            public double TerrainLowerLimit => m_Msg.TerrainLowerLimit;
            public int PricePerMeter => m_Msg.PricePerMeter;
            public int RedirectGridX => m_Msg.RedirectGridX;
            public int RedirectGridY => m_Msg.RedirectGridY;
            public int UseEstateSun => m_Msg.UseEstateSun.ToLSLBoolean();
            public double SunHour => m_Msg.SunHour;
            public string ProductSKU => m_Msg.ProductSKU;
            public string ProductName => m_Msg.ProductName;
            public int HardMaxAgents => (int)m_Msg.HardMaxAgents;
            public int HardMaxObjects => (int)m_Msg.HardMaxObjects;
            public long RegionFlagsExtended => m_Msg.RegionFlagsExtended.Count != 0 ? (long)m_Msg.RegionFlagsExtended[0] : 0;
        }

        [TranslatedScriptEvent("regioninfo_received")]
        public class RegionInfoReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;

            [TranslatedScriptEventParameter(1)]
            public RegionInfoData RegionData;

            public static void HandleRegionInfo(Message m, ViewerConnection vc, uint circuitCode)
            {
                var msg = (RegionInfo)m;
                vc.PostEvent(new RegionInfoReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                    RegionData = new RegionInfoData(msg)
                });
            }
        }

        [APIExtension("ViewerControl", "regioninfo_received")]
        [StateEventDelegate]
        public delegate void RegionInfoReceived(
            [Description("Agent info")]
            AgentInfo agent,
            LSLKey regionID,
            RegionInfoData regionData);
        #endregion

        #region regionhandshake_received event
        [APIExtension("ViewerControl", "regionhandshakedata")]
        [APIDisplayName("regionhandshakedata")]
        [APIIsVariableType]
        [APIAccessibleMembers]
        public sealed class RegionHandshakeData
        {
            private readonly RegionHandshake m_Msg;

            public RegionHandshakeData(RegionHandshake m)
            {
                m_Msg = m;
            }

            public int RegionFlags => (int)m_Msg.RegionFlags;
            public int SimAccess => (int)m_Msg.SimAccess;
            public string SimName => m_Msg.SimName;
            public LSLKey SimOwner => m_Msg.SimOwner;
            public int IsEstateManager => m_Msg.IsEstateManager.ToLSLBoolean();
            public double WaterHeight => m_Msg.WaterHeight;
            public double BillableFactor => m_Msg.BillableFactor;
            public LSLKey CacheID => m_Msg.CacheID;
            public AnArray TerrainBase => new AnArray { m_Msg.TerrainBase0, m_Msg.TerrainBase1, m_Msg.TerrainBase2, m_Msg.TerrainBase3 };
            public AnArray TerrainDetail => new AnArray { m_Msg.TerrainDetail0, m_Msg.TerrainDetail1, m_Msg.TerrainDetail2, m_Msg.TerrainDetail3 };
            public AnArray TerrainStartHeight => new AnArray { m_Msg.TerrainStartHeight00, m_Msg.TerrainStartHeight01, m_Msg.TerrainStartHeight10, m_Msg.TerrainStartHeight11 };
            public AnArray TerrainHeightRange => new AnArray { m_Msg.TerrainHeightRange00, m_Msg.TerrainHeightRange01, m_Msg.TerrainHeightRange10, m_Msg.TerrainHeightRange11 };
            public LSLKey RegionID => m_Msg.RegionID;
            public int CPUClassID => m_Msg.CPUClassID;
            public int CPURatio => m_Msg.CPURatio;
            public string ColoName => m_Msg.ColoName;
            public string ProductSKU => m_Msg.ProductSKU;
            public string ProductName => m_Msg.ProductName;
            public long RegionFlagsExtended => m_Msg.RegionExtData.Count > 0 ? (long)m_Msg.RegionExtData[0].RegionFlagsExtended : 0;
            public long RegionProtocols => m_Msg.RegionExtData.Count > 0 ? (long)m_Msg.RegionExtData[0].RegionProtocols : 0;
        }

        [TranslatedScriptEvent("regionhandshake_received")]
        public class RegionHandshakeReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;

            [TranslatedScriptEventParameter(1)]
            public LSLKey RegionID;

            [TranslatedScriptEventParameter(2)]
            public RegionHandshakeData RegionData;

            public static void HandleRegionHandshake(Message m, ViewerConnection vc, uint circuitCode)
            {
                var msg = (RegionHandshake)m;
                vc.PostEvent(new RegionHandshakeReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                    RegionID = msg.RegionID,
                    RegionData = new RegionHandshakeData(msg)
                });
            }
        }

        [APIExtension("ViewerControl", "regionhandshake_received")]
        [StateEventDelegate]
        public delegate void RegionHandshakeReceived(
            [Description("Agent info")]
            AgentInfo agent,
            LSLKey regionID,
            RegionHandshakeData regionData);
        #endregion

        #region logoutreply_received
        [TranslatedScriptEvent("logoutreply_received")]
        public class LogoutReplyReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent { get; }

            public LogoutReplyReceivedEvent(Message m, uint circuitcode)
            {
                Agent = new AgentInfo(m, circuitcode);
            }
        }

        [APIExtension("ViewerControl", "logoutreply_received")]
        [StateEventDelegate]
        public delegate void LogoutReplyReceived(
            [Description("Agent info")]
            AgentInfo agent);
        #endregion

        #region telehubinfo_received
        [TranslatedScriptEvent("telehubinfo_received")]
        public class TelehubInfoReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey ObjectID;
            [TranslatedScriptEventParameter(2)]
            public string ObjectName;
            [TranslatedScriptEventParameter(3)]
            public Vector3 TelehubPos;
            [TranslatedScriptEventParameter(4)]
            public Quaternion TelehubRot;
            [TranslatedScriptEventParameter(5)]
            public AnArray SpawnPointPos = new AnArray();

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                var res = (TelehubInfo)m;
                var ev = new TelehubInfoReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                    ObjectID = res.ObjectID,
                    ObjectName = res.ObjectName,
                    TelehubPos = res.TelehubPos,
                    TelehubRot = res.TelehubRot
                };
                foreach (Vector3 v in res.SpawnPoints)
                {
                    ev.SpawnPointPos.Add(v);
                }
                vc.PostEvent(ev);
            }
        }

        [APIExtension("ViewerControl", "telehubinfo_received")]
        [StateEventDelegate]
        public delegate void TelehubInfoReceived(
            AgentInfo agent,
            LSLKey objectId,
            string objectName,
            Vector3 telehubPos,
            AnArray spawnpointPos);
        #endregion

        #region teleportlocal_received
        [TranslatedScriptEvent("teleportlocal_received")]
        public class TeleportLocalReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public Vector3 Position;
            [TranslatedScriptEventParameter(2)]
            public Vector3 LookAt;
            [TranslatedScriptEventParameter(3)]
            public int TeleportFlags;

            public static void ToScriptEvent(TeleportLocal m, ViewerConnection vc, uint circuitCode)
            {
                vc.PostEvent(new TeleportLocalReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                    Position = m.Position,
                    LookAt = m.LookAt,
                    TeleportFlags = (int)m.TeleportFlags
                });
            }
        }

        [APIExtension("ViewerControl", "teleportlocal_received")]
        [StateEventDelegate]
        public delegate void TeleportLocalReceived(
            AgentInfo agent,
            Vector3 position,
            Vector3 lookAt,
            int teleportFlags);
        #endregion

        #region teleportprogress_received
        [TranslatedScriptEvent("teleportprogress_received")]
        public class TeleportProgressReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public int TeleportFlags;
            [TranslatedScriptEventParameter(2)]
            public string Message;

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                var res = (TeleportProgress)m;
                vc.PostEvent(new TeleportProgressReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                    TeleportFlags = (int)res.TeleportFlags,
                    Message = res.Message
                });
            }
        }

        [APIExtension("ViewerControl", "teleportprogress_received")]
        [StateEventDelegate]
        public delegate void TeleportProgressReceived(
            AgentInfo agent,
            int teleportFlags,
            string message);
        #endregion

        #region teleportstart_received
        [TranslatedScriptEvent("teleportstart_received")]
        public class TeleportStartReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public int TeleportFlags;

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                var res = (TeleportStart)m;
                vc.PostEvent(new TeleportStartReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                    TeleportFlags = (int)res.TeleportFlags
                });
            }
        }

        [APIExtension("ViewerControl", "teleportstart_received")]
        [StateEventDelegate]
        public delegate void TeleportStartReceived(
            AgentInfo agent,
            int teleportFlags);
        #endregion

        #region teleportfailed_received
        [TranslatedScriptEvent("teleportfailed_received")]
        public class TeleportFailedReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey Reason;
            [TranslatedScriptEventParameter(2)]
            public AnArray AlertInfo = new AnArray();

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                var res = (TeleportFailed)m;
                var ev = new TeleportFailedReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                    Reason = res.Reason
                };

                foreach (TeleportFailed.AlertInfoEntry e in res.AlertInfo)
                {
                    ev.AlertInfo.Add(e.Message);
                    ev.AlertInfo.Add(e.ExtraParams);
                }
                vc.PostEvent(ev);
            }
        }

        [APIExtension("ViewerControl", "teleportfailed_received")]
        [StateEventDelegate]
        public delegate void TeleportFailedReceived(
            AgentInfo agent,
            string reason,
            AnArray alertInfo);
        #endregion

        #region economydata_received
        [TranslatedScriptEvent("economydata_received")]
        public class EconomyDataReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                vc.PostEvent(new EconomyDataReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                });
            }
        }

        [APIExtension("ViewerControl", "economydata_received")]
        [StateEventDelegate]
        public delegate void EconomyDataReceived(
            AgentInfo agent);
        #endregion

        #region alertmessage_received
        [TranslatedScriptEvent("alertmessage_received")]
        public class AlertMessageReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public string Message;
            [TranslatedScriptEventParameter(2)]
            public AnArray AlertInfo = new AnArray();

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                var res = (AlertMessage)m;
                var ev = new AlertMessageReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                    Message = res.Message
                };

                foreach (AlertMessage.Data d in res.AlertInfo)
                {
                    ev.AlertInfo.Add(d.Message);
                    ev.AlertInfo.Add(d.ExtraParams.ToHexString());
                }
                vc.PostEvent(ev);
            }
        }

        [APIExtension("ViewerControl", "alertmessage_received")]
        [StateEventDelegate]
        public delegate void AlertMessageReceived(
            AgentInfo agent,
            string message,
            AnArray alertInfo);
        #endregion

        #region agentdataupdate_received
        [TranslatedScriptEvent("agentdataupdate_received")]
        public class AgentDataUpdateReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public string FirstName = string.Empty;
            [TranslatedScriptEventParameter(2)]
            public string LastName = string.Empty;
            [TranslatedScriptEventParameter(3)]
            public string GroupTitle = string.Empty;
            [TranslatedScriptEventParameter(4)]
            public LSLKey ActiveGroupID = new LSLKey();
            [TranslatedScriptEventParameter(5)]
            public long GroupPowers;
            [TranslatedScriptEventParameter(6)]
            public string GroupName = string.Empty;

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                var res = (AgentDataUpdate)m;
                vc.PostEvent(new AgentDataUpdateReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                    FirstName = res.FirstName,
                    LastName = res.LastName,
                    GroupTitle = res.GroupTitle,
                    ActiveGroupID = res.ActiveGroupID,
                    GroupPowers = (long)res.GroupPowers,
                    GroupName = res.GroupName
                });
            }
        }

        [APIExtension("ViewerControl", "agentdataupdate_received")]
        [StateEventDelegate]
        public delegate void AgentDataUpdateReceived(
            AgentInfo agent,
            string firstName,
            string lastName,
            string groupTitle,
            LSLKey activeGroupId,
            long groupPowers,
            string groupName);
        #endregion

        #region agentdropgroup_received
        [TranslatedScriptEvent("agentdropgroup_received")]
        public class AgentDropGroupReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey GroupID = new LSLKey();

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                var res = (AgentDropGroup)m;
                vc.PostEvent(new AgentDropGroupReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                    GroupID = res.GroupID
                });
            }
        }

        [APIExtension("ViewerControl", "agentdropgroup_received")]
        [StateEventDelegate]
        public delegate void AgentDropGroupReceived(
            AgentInfo agent,
            LSLKey groupID);
        #endregion

        #region coarselocationupdate_received
        [TranslatedScriptEvent("coarselocationupdate_received")]
        public class CoarseLocationUpdateReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public int You;
            [TranslatedScriptEventParameter(2)]
            public int Prey;
            [TranslatedScriptEventParameter(3)]
            public AnArray AgentData = new AnArray();

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                var res = (CoarseLocationUpdate)m;
                var ev = new CoarseLocationUpdateReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                    Prey = res.Prey,
                    You = res.You
                };
                foreach (CoarseLocationUpdate.AgentDataEntry d in res.AgentData)
                {
                    ev.AgentData.Add(new LSLKey(d.AgentID));
                    ev.AgentData.Add(d.X);
                    ev.AgentData.Add(d.Y);
                    ev.AgentData.Add(d.Z);
                }
                vc.PostEvent(ev);
            }
        }

        [APIExtension("ViewerControl", "coarselocationupdate_received")]
        [StateEventDelegate]
        public delegate void CoarseLocationUpdateReceived(
            AgentInfo agent,
            int you,
            int prey,
            AnArray agentData);
        #endregion

        #region healthmessage_received
        [TranslatedScriptEvent("healthmessage_received")]
        public class HealthMessageReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public double Health;

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                var res = (HealthMessage)m;
                vc.PostEvent(new HealthMessageReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                    Health = res.Health
                });
            }
        }

        [APIExtension("ViewerControl", "healthmessage_received")]
        [StateEventDelegate]
        public delegate void HealthMessageUpdateReceived(
            AgentInfo agent,
            double health);
        #endregion

        #region avataranimation_received
        [TranslatedScriptEvent("avataranimation_received")]
        public class AvatarAnimationReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey Sender = new LSLKey();
            [TranslatedScriptEventParameter(2)]
            public AnArray AnimationData = new AnArray();

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                var res = (AvatarAnimation)m;
                var ev = new AvatarAnimationReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                    Sender = res.Sender
                };

                foreach (AvatarAnimation.AnimationData ad in res.AnimationList)
                {
                    ev.AnimationData.Add(new LSLKey(ad.AnimID));
                    ev.AnimationData.Add((int)ad.AnimSequenceID);
                    ev.AnimationData.Add(new LSLKey(ad.ObjectID));
                }
                vc.PostEvent(ev);
            }
        }

        [APIExtension("ViewerControl", "avataranimation_received")]
        [StateEventDelegate]
        public delegate void AvatarAnimationReceived(
            AgentInfo agent,
            LSLKey sender,
            AnArray animationData);
        #endregion

        #region avatarsitresponse_received
        [TranslatedScriptEvent("avatarsitresponse_received")]
        public class AvatarSitResponseReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey SitObject = new LSLKey();
            [TranslatedScriptEventParameter(2)]
            public int IsAutopilot;
            [TranslatedScriptEventParameter(3)]
            public Vector3 SitPosition;
            [TranslatedScriptEventParameter(4)]
            public Quaternion SitRotation;
            [TranslatedScriptEventParameter(5)]
            public Vector3 CameraEyeOffset;
            [TranslatedScriptEventParameter(6)]
            public Vector3 CameraAtOffset;
            [TranslatedScriptEventParameter(7)]
            public int ForceMouselook;

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                var res = (AvatarSitResponse)m;
                vc.PostEvent(new AvatarSitResponseReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                    SitObject = res.SitObject,
                    IsAutopilot = res.IsAutoPilot.ToLSLBoolean(),
                    SitPosition = res.SitPosition,
                    SitRotation = res.SitRotation,
                    CameraEyeOffset = res.CameraEyeOffset,
                    CameraAtOffset = res.CameraAtOffset,
                    ForceMouselook = res.ForceMouselook.ToLSLBoolean()
                });
            }
        }

        [APIExtension("ViewerControl", "avatarsitresponse_received")]
        [StateEventDelegate]
        public delegate void AvatarSitResponseReceived(
            AgentInfo agent,
            LSLKey sitObject,
            int isAutopilot,
            Vector3 sitPosition,
            Quaternion sitRotation,
            Vector3 cameraEyeOffset,
            Vector3 cameraAtOffset,
            int forceMouselook);
        #endregion

        #region cameraconstraint_received
        [TranslatedScriptEvent("cameraconstraint_received")]
        public class CameraConstraintReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public Quaternion CameraCollidePlane;

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                var res = (CameraConstraint)m;
                vc.PostEvent(new CameraConstraintReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                    CameraCollidePlane = new Quaternion(res.CameraCollidePlane.X, res.CameraCollidePlane.Y, res.CameraCollidePlane.Z, res.CameraCollidePlane.W)
                });
            }
        }

        [APIExtension("ViewerControl", "cameraconstraint_received")]
        [StateEventDelegate]
        public delegate void CameraConstraintReceived(
            AgentInfo agent,
            Quaternion cameraCollidePlane);
        #endregion

        #region clearfollowcamproperties_received
        [TranslatedScriptEvent("clearfollowcamproperties_received")]
        public class ClearFollowCamPropertiesReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey ObjectID = new LSLKey();

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                var res = (ClearFollowCamProperties)m;
                vc.PostEvent(new ClearFollowCamPropertiesReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                    ObjectID = res.ObjectID
                });
            }
        }

        [APIExtension("ViewerControl", "clearfollowcamproperties_received")]
        [StateEventDelegate]
        public delegate void ClearFollowCamPropertiesReceived(
            AgentInfo agent,
            LSLKey objectID);
        #endregion

        #region setfollowcamproperties_received
        [TranslatedScriptEvent("setfollowcamproperties_received")]
        public class SetFollowCamPropertiesReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey ObjectID = new LSLKey();
            [TranslatedScriptEventParameter(2)]
            public AnArray CameraParams = new AnArray();

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                var res = (SetFollowCamProperties)m;
                var ev = new SetFollowCamPropertiesReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                    ObjectID = res.ObjectID
                };
                foreach (SetFollowCamProperties.CameraProperty prop in res.CameraProperties)
                {
                    ev.CameraParams.Add(prop.Type);
                    ev.CameraParams.Add(prop.Value);
                }
                vc.PostEvent(ev);
            }
        }

        [APIExtension("ViewerControl", "setfollowcamproperties_received")]
        [StateEventDelegate]
        public delegate void SetFollowCamPropertiesReceived(
            AgentInfo agent,
            LSLKey objectID,
            AnArray cameraParams);
        #endregion

        #region chatfromsimulator_received
        [TranslatedScriptEvent("chatfromsimulator_received")]
        public class ChatFromSimulatorReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public string FromName;
            [TranslatedScriptEventParameter(2)]
            public LSLKey SourceID;
            [TranslatedScriptEventParameter(3)]
            public LSLKey OwnerID;
            [TranslatedScriptEventParameter(4)]
            public int SourceType;
            [TranslatedScriptEventParameter(5)]
            public int ChatType;
            [TranslatedScriptEventParameter(6)]
            public int AudibleLevel;
            [TranslatedScriptEventParameter(7)]
            public Vector3 Position;
            [TranslatedScriptEventParameter(8)]
            public string Message;

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                var res = (ChatFromSimulator)m;
                vc.PostEvent(new ChatFromSimulatorReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                    FromName = res.FromName,
                    SourceID = res.SourceID,
                    OwnerID = res.OwnerID,
                    SourceType = (int)res.SourceType,
                    ChatType = (int)res.ChatType,
                    AudibleLevel = (int)res.Audible,
                    Position = res.Position,
                    Message = res.Message
                });
            }
        }

        [APIExtension("ViewerControl", "chatfromsimulator_received")]
        [StateEventDelegate]
        public delegate void ChatFromSimulatorReceived(
            AgentInfo agent,
            string fromName,
            LSLKey sourceID,
            LSLKey ownerID,
            int sourceType,
            int chatType,
            int audibleLevel,
            Vector3 position,
            string message);
        #endregion

        #region
        [TranslatedScriptEvent("estatecovenantreply_received")]
        public class EstateCovenantReplyReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey CovenantID = new LSLKey();
            [TranslatedScriptEventParameter(2)]
            public long CovenantTimestamp;
            [TranslatedScriptEventParameter(3)]
            public string EstateName;
            [TranslatedScriptEventParameter(4)]
            public LSLKey EstateOwnerID = new LSLKey();

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                var res = (EstateCovenantReply)m;
                vc.PostEvent(new EstateCovenantReplyReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                    CovenantID = res.CovenantID,
                    CovenantTimestamp = res.CovenantTimestamp,
                    EstateName = res.EstateName,
                    EstateOwnerID = res.EstateOwnerID
                });
            }
        }

        [APIExtension("ViewerControl", "estatecovenantreply_received")]
        [StateEventDelegate]
        public delegate void EstateCovenantReplyReceived(
            AgentInfo agent,
            LSLKey covenantID,
            long covenantTimestamp,
            string estateName,
            LSLKey estateOwnerID);
        #endregion

        #region loadurl_received
        [TranslatedScriptEvent("loadurl_received")]
        public class LoadURLReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public string ObjectName = string.Empty;
            [TranslatedScriptEventParameter(2)]
            public LSLKey ObjectID = new LSLKey();
            [TranslatedScriptEventParameter(3)]
            public LSLKey OwnerID = new LSLKey();
            [TranslatedScriptEventParameter(4)]
            public int OwnerIsGroup;
            [TranslatedScriptEventParameter(5)]
            public string Message = string.Empty;
            [TranslatedScriptEventParameter(6)]
            public string URL = string.Empty;

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                var res = (LoadURL)m;
                vc.PostEvent(new LoadURLReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                    ObjectName = res.ObjectName,
                    ObjectID = res.ObjectID,
                    OwnerID = res.OwnerID,
                    OwnerIsGroup = res.OwnerIsGroup.ToLSLBoolean(),
                    Message = res.Message,
                    URL = res.URL
                });
            }
        }

        [APIExtension("ViewerControl", "loadurl_received")]
        [StateEventDelegate]
        public delegate void LoadURLReceived(
            AgentInfo agent,
            string objectName,
            LSLKey objectId,
            LSLKey ownerID,
            int ownerIsGroup,
            string message,
            string url);
        #endregion

        #region scriptteleportrequest_received
        [TranslatedScriptEvent("scriptteleportrequest_received")]
        public class ScriptTeleportRequestReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public string ObjectName = string.Empty;
            [TranslatedScriptEventParameter(2)]
            public string SimName = string.Empty;
            [TranslatedScriptEventParameter(3)]
            public Vector3 SimPosition;
            [TranslatedScriptEventParameter(4)]
            public Vector3 LookAt;

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                var res = (ScriptTeleportRequest)m;
                vc.PostEvent(new ScriptTeleportRequestReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                    ObjectName = res.ObjectName,
                    SimName = res.SimName,
                    SimPosition = res.SimPosition,
                    LookAt = res.LookAt
                });
            }
        }

        [APIExtension("ViewerControl", "scriptteleportrequest_received")]
        [StateEventDelegate]
        public delegate void ScriptTeleportRequestReceived(
            AgentInfo agent,
            string objectName,
            string simName,
            Vector3 simPosition,
            Vector3 lookAt);
        #endregion

        #region scriptquestion_received
        [TranslatedScriptEvent("scriptquestion_received")]
        public class ScriptQuestionReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey TaskID = new LSLKey();
            [TranslatedScriptEventParameter(2)]
            public LSLKey ItemID = new LSLKey();
            [TranslatedScriptEventParameter(3)]
            public string ObjectName;
            [TranslatedScriptEventParameter(4)]
            public string ObjectOwner;
            [TranslatedScriptEventParameter(5)]
            public int Questions;
            [TranslatedScriptEventParameter(6)]
            public LSLKey ExperienceID = new LSLKey();

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                var res = (ScriptQuestion)m;
                vc.PostEvent(new ScriptQuestionReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                    TaskID = res.TaskID,
                    ItemID = res.ItemID,
                    ObjectName = res.ObjectName,
                    ObjectOwner = res.ObjectOwner,
                    Questions = (int)res.Questions,
                    ExperienceID = res.ExperienceID
                });
            }
        }

        [APIExtension("ViewerControl", "scriptquestion_received")]
        [StateEventDelegate]
        public delegate void ScriptQuestionReceived(
            AgentInfo agent,
            LSLKey taskID,
            LSLKey itemID,
            string objectName,
            string objectOwner,
            int scriptPermissions,
            LSLKey experienceID);
        #endregion

        #region scriptdialog_received
        [TranslatedScriptEvent("scriptdialog_received")]
        public class ScriptDialogReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey ObjectID = new LSLKey();
            [TranslatedScriptEventParameter(2)]
            public string FirstName = string.Empty;
            [TranslatedScriptEventParameter(3)]
            public string LastName = string.Empty;
            [TranslatedScriptEventParameter(4)]
            public string ObjectName = string.Empty;
            [TranslatedScriptEventParameter(5)]
            public string Message = string.Empty;
            [TranslatedScriptEventParameter(6)]
            public int ChatChannel;
            [TranslatedScriptEventParameter(7)]
            public LSLKey ImageID = new LSLKey();
            [TranslatedScriptEventParameter(8)]
            public AnArray ButtonData = new AnArray();
            [TranslatedScriptEventParameter(9)]
            public AnArray OwnerData = new AnArray();

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                var res = (ScriptDialog)m;
                var ev = new ScriptDialogReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                    ObjectID = res.ObjectID,
                    FirstName = res.FirstName,
                    LastName = res.LastName,
                    ObjectName = res.ObjectName,
                    Message = res.Message,
                    ChatChannel = res.ChatChannel,
                    ImageID = res.ImageID
                };
                foreach (string button in res.Buttons)
                {
                    ev.ButtonData.Add(button);
                }
                foreach (UUID owner in res.OwnerData)
                {
                    ev.OwnerData.Add(owner);
                }
                vc.PostEvent(ev);
            }
        }

        [APIExtension("ViewerControl", "scriptdialog_received")]
        [StateEventDelegate]
        public delegate void ScriptDialogReceived(
            AgentInfo agent,
            LSLKey objectID,
            string firstName,
            string lastName,
            string objectName,
            string message,
            int chatchannel,
            LSLKey imageID,
            AnArray buttons,
            AnArray ownerdata);
        #endregion

        #region scriptcontrolchange_received
        [TranslatedScriptEvent("scriptcontrolchange_received")]
        public class ScriptControlChangeReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public AnArray ControlData = new AnArray();

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                var res = (ScriptControlChange)m;
                var ev = new ScriptControlChangeReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode)
                };
                foreach (ScriptControlChange.DataEntry d in res.Data)
                {
                    ev.ControlData.Add(d.TakeControls.ToLSLBoolean());
                    ev.ControlData.Add((int)d.Controls);
                    ev.ControlData.Add(d.PassToAgent.ToLSLBoolean());
                }
                vc.PostEvent(ev);
            }
        }

        [APIExtension("ViewerControl", "scriptcontrolchange_received")]
        [StateEventDelegate]
        public delegate void ScriptControlChangeReceived(AgentInfo agent, AnArray controlData);
        #endregion

        #region preloadsound_received
        [TranslatedScriptEvent("preloadsound_received")]
        public class PreloadSoundReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey ObjectID = new LSLKey();
            [TranslatedScriptEventParameter(2)]
            public LSLKey OwnerID = new LSLKey();
            [TranslatedScriptEventParameter(3)]
            public LSLKey SoundID = new LSLKey();

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                var res = (PreloadSound)m;
                vc.PostEvent(new PreloadSoundReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                    ObjectID = res.ObjectID,
                    OwnerID = res.OwnerID,
                    SoundID = res.SoundID
                });
            }
        }

        [APIExtension("ViewerControl", "preloadsound_received")]
        [StateEventDelegate]
        public delegate void PreloadsoundReceived(
            AgentInfo agent,
            LSLKey objectId,
            LSLKey ownerID,
            LSLKey soundID);
        #endregion

        #region attachedsound_received
        [TranslatedScriptEvent("attachedsound_received")]
        public class AttachedSoundReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey SoundID = new LSLKey();
            [TranslatedScriptEventParameter(2)]
            public LSLKey ObjectID = new LSLKey();
            [TranslatedScriptEventParameter(3)]
            public LSLKey OwnerID = new LSLKey();
            [TranslatedScriptEventParameter(4)]
            public double Gain;
            [TranslatedScriptEventParameter(5)]
            public int Flags;

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                var res = (AttachedSound)m;
                vc.PostEvent(new AttachedSoundReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                    SoundID = res.SoundID,
                    ObjectID = res.ObjectID,
                    OwnerID = res.OwnerID,
                    Gain = res.Gain,
                    Flags = (int)res.Flags
                });
            }
        }

        [APIExtension("ViewerControl", "attachedsound_received")]
        [StateEventDelegate]
        public delegate void AttachedSoundReceived(
            AgentInfo agent,
            LSLKey soundID,
            LSLKey objectID,
            LSLKey ownerID,
            double gain,
            int flags);
        #endregion

        #region soundtrigger_received
        [TranslatedScriptEvent("soundtrigger_received")]
        public class SoundTriggerReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey SoundID = new LSLKey();
            [TranslatedScriptEventParameter(2)]
            public LSLKey OwnerID = new LSLKey();
            [TranslatedScriptEventParameter(3)]
            public LSLKey ObjectID = new LSLKey();
            [TranslatedScriptEventParameter(4)]
            public LSLKey ParentID = new LSLKey();
            [TranslatedScriptEventParameter(5)]
            public int GridX;
            [TranslatedScriptEventParameter(6)]
            public int GridY;
            [TranslatedScriptEventParameter(7)]
            public Vector3 Position;
            [TranslatedScriptEventParameter(8)]
            public double Gain;

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                var res = (SoundTrigger)m;
                vc.PostEvent(new SoundTriggerReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                    SoundID = res.SoundID,
                    OwnerID = res.OwnerID,
                    ObjectID = res.ObjectID,
                    ParentID = res.ParentID,
                    GridX = res.GridPosition.GridX,
                    GridY = res.GridPosition.GridY,
                    Position = res.Position,
                    Gain = res.Gain
                });
            }
        }

        [APIExtension("ViewerControl", "soundtrigger_received")]
        [StateEventDelegate]
        public delegate void SoundTriggerReceived(
            AgentInfo agent,
            LSLKey soundID,
            LSLKey ownerID,
            LSLKey objectID,
            LSLKey parentID,
            int gridX,
            int gridY,
            Vector3 position,
            double gain);
        #endregion

        #region attachedsoundgainchange_received
        [TranslatedScriptEvent("attachedsoundgainchange_received")]
        public class AttachedSoundGainChangeReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey ObjectID = new LSLKey();
            [TranslatedScriptEventParameter(2)]
            public double Gain;

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                var res = (AttachedSoundGainChange)m;
                vc.PostEvent(new AttachedSoundGainChangeReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                    ObjectID = res.ObjectID,
                    Gain = res.Gain
                });
            }
        }

        [APIExtension("ViewerControl", "attachedsoundgainchange_received")]
        [StateEventDelegate]
        public delegate void AttachedSoundGainChangeReceived(
            AgentInfo agent,
            LSLKey objectID,
            double gain);
        #endregion

        #region featuredisabled_received
        [TranslatedScriptEvent("featuredisabled_received")]
        public class FeatureDisabledReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey TransactionID = new LSLKey();
            [TranslatedScriptEventParameter(2)]
            public string ErrorMessage = string.Empty;

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                var res = (FeatureDisabled)m;
                vc.PostEvent(new FeatureDisabledReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                    TransactionID = res.TransactionID,
                    ErrorMessage = res.ErrorMessage
                });
            }
        }

        [APIExtension("ViewerControl", "featuredisabled_received")]
        [StateEventDelegate]
        public delegate void FeatureDisabledReceived(
            AgentInfo agent,
            LSLKey transactionID,
            string errorMessage);
        #endregion

        #region paypricereply_received
        [TranslatedScriptEvent("paypricereply_received")]
        public class PayPriceReplyReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey ObjectID;
            [TranslatedScriptEventParameter(2)]
            public int DefaultPayPrice;
            [TranslatedScriptEventParameter(3)]
            public AnArray ButtonData = new AnArray();

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                var res = (PayPriceReply)m;
                var ev = new PayPriceReplyReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                    ObjectID = res.ObjectID,
                    DefaultPayPrice = res.DefaultPayPrice,
                };
                foreach(int val in res.ButtonData)
                {
                    ev.ButtonData.Add(val);
                }
                vc.PostEvent(ev);
            }
        }

        [APIExtension("ViewerControl", "paypricereply_received")]
        [StateEventDelegate]
        public delegate void PayPriceReplyReceived(
            AgentInfo agent,
            LSLKey objectID,
            int defaultPayPrice,
            AnArray buttonData);
        #endregion

        #region killobject_received
        [TranslatedScriptEvent("killobject_received")]
        public class KillObjectReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public AnArray LocalIDs;

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                var res = (KillObject)m;
                var ev = new KillObjectReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode)
                };
                foreach(uint localid in res.LocalIDs)
                {
                    ev.LocalIDs.Add((int)localid);
                }
                vc.PostEvent(ev);
            }
        }

        [APIExtension("ViewerControl", "killobject_received")]
        [StateEventDelegate]
        public delegate void KillObjectReceived(
            AgentInfo agent,
            AnArray localids);
        #endregion

        #region onlinenotification_received
        [TranslatedScriptEvent("onlinenotification_received")]
        public class OnlineNotificationReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public AnArray Agents;

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                OnlineNotification res = (OnlineNotification)m;
                var ev = new OnlineNotificationReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                };
                foreach(UUID id in res.AgentIDs)
                {
                    ev.Agents.Add(new LSLKey(id));
                }
                vc.PostEvent(ev);
            }
        }

        [APIExtension("ViewerControl", "onlinenotification_received")]
        [StateEventDelegate]
        public delegate void OnlineNotificationReceived(
            AgentInfo agent,
            AnArray agents);
        #endregion

        #region offlinenotification_received
        [TranslatedScriptEvent("offlinenotification_received")]
        public class OfflineNotificationReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public AnArray Agents;

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                OnlineNotification res = (OnlineNotification)m;
                var ev = new OnlineNotificationReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                };
                foreach (UUID id in res.AgentIDs)
                {
                    ev.Agents.Add(new LSLKey(id));
                }
                vc.PostEvent(ev);
            }
        }

        [APIExtension("ViewerControl", "offlinenotification_received")]
        [StateEventDelegate]
        public delegate void OfflineNotificationReceived(
            AgentInfo agent,
            AnArray agents);
        #endregion

        #region improvedinstantmessage_received
        [TranslatedScriptEvent("improvedinstantmessage_received")]
        public class ImprovedInstantMessageReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public int FromGroup;
            [TranslatedScriptEventParameter(2)]
            public LSLKey ToAgentID;
            [TranslatedScriptEventParameter(3)]
            public int ParentEstateID;
            [TranslatedScriptEventParameter(4)]
            public LSLKey RegionID;
            [TranslatedScriptEventParameter(5)]
            public Vector3 Position;
            [TranslatedScriptEventParameter(6)]
            public int IsOffline;
            [TranslatedScriptEventParameter(7)]
            public int Dialog;
            [TranslatedScriptEventParameter(8)]
            public LSLKey ID;
            [TranslatedScriptEventParameter(9)]
            public long Timestamp;
            [TranslatedScriptEventParameter(10)]
            public string FromAgentName;
            [TranslatedScriptEventParameter(11)]
            public string Message;
            [TranslatedScriptEventParameter(12)]
            public ByteArrayApi.ByteArray BinaryBucket;

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                var res = (ImprovedInstantMessage)m;
                vc.PostEvent(new ImprovedInstantMessageReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                    FromGroup = res.FromGroup.ToLSLBoolean(),
                    ToAgentID = res.ToAgentID,
                    ParentEstateID = (int)res.ParentEstateID,
                    RegionID = res.RegionID,
                    Position = res.Position,
                    IsOffline = res.IsOffline.ToLSLBoolean(),
                    Dialog = (int)res.Dialog,
                    ID = res.ID,
                    Timestamp = res.Timestamp.AsLong,
                    FromAgentName = res.FromAgentName,
                    Message = res.Message,
                    BinaryBucket = new ByteArrayApi.ByteArray(res.BinaryBucket)
                });
            }
        }

        [APIExtension("ViewerControl", "improvedinstantmessage_received")]
        [StateEventDelegate]
        public delegate void ImprovedInstantMessageReceived(
            AgentInfo agent,
            int fromGroup,
            LSLKey toAgentID,
            int parentEstateID,
            LSLKey regionID,
            Vector3 position,
            int isOffline,
            int dialog,
            LSLKey id,
            long timestamp,
            string fromAgentName,
            string message,
            ByteArrayApi.ByteArray binaryBucket);
        #endregion

        #region uuidgroupnamereply_received
        [TranslatedScriptEvent("uuidgroupnamereply_received")]
        public class UUIDGroupNameReplyReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public AnArray Data;

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                var res = (UUIDGroupNameReply)m;
                var ev = new UUIDGroupNameReplyReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode)
                };
                foreach(UUIDGroupNameReply.Data d in res.UUIDNameBlock)
                {
                    ev.Data.Add(new LSLKey(d.ID));
                    ev.Data.Add(d.GroupName);
                }
                vc.PostEvent(ev);
            }
        }

        [APIExtension("ViewerControl", "uuidgroupnamereply_received")]
        [StateEventDelegate]
        public delegate void UUIDGroupNameReplyReceived(
            AgentInfo agent,
            AnArray data);
        #endregion

        #region uuidnamereply_received
        [TranslatedScriptEvent("uuidnamereply_received")]
        public class UUIDNameReplyReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public AnArray Data;

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                var res = (UUIDNameReply)m;
                var ev = new UUIDNameReplyReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode)
                };
                foreach(UUIDNameReply.Data d in res.UUIDNameBlock)
                {
                    ev.Data.Add(new LSLKey(d.ID));
                    ev.Data.Add(d.FirstName + " " + d.LastName);
                }
                vc.PostEvent(ev);
            }
        }

        [APIExtension("ViewerControl", "uuidnamereply_received")]
        [StateEventDelegate]
        public delegate void UUIDNameReplyReceived(
            AgentInfo agent, 
            AnArray data);
        #endregion

        #region derezack_received
        [TranslatedScriptEvent("derezack_received")]
        public class DeRezAckReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey TransactionID;
            [TranslatedScriptEventParameter(2)]
            public int Success;

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                var res = (DeRezAck)m;
                vc.PostEvent(new DeRezAckReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                    TransactionID = res.TransactionID,
                    Success = res.Success.ToLSLBoolean()
                });
            }
        }

        [APIExtension("ViewerControl", "derezack_received")]
        [StateEventDelegate]
        public delegate void DeRezAckReceived(
            AgentInfo agent,
            LSLKey transactionID,
            int success);
        #endregion

        #region forceobjectselect_received
        [TranslatedScriptEvent("forceobjectselect_received")]
        public class ForceObjectSelectReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public int ResetList;
            [TranslatedScriptEventParameter(2)]
            public AnArray LocalIDs;

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                var res = (ForceObjectSelect)m;
                var ev = new ForceObjectSelectReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                    ResetList = res.ResetList.ToLSLBoolean()
                };

                foreach(uint id in res.LocalIDs)
                {
                    ev.LocalIDs.Add((int)id);
                }
                vc.PostEvent(ev);
            }
        }

        [APIExtension("ViewerControl", "forceobjectselect_received")]
        [StateEventDelegate]
        public delegate void ForceObjectSelectReceived(
            AgentInfo agent,
            int resetList,
            AnArray localIDs);
        #endregion

        #region objectanimation_received
        [TranslatedScriptEvent("objectanimation_received")]
        public class ObjectAnimationReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey Sender;
            [TranslatedScriptEventParameter(2)]
            public AnArray AnimationData;

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                var res = (ObjectAnimation)m;
                var ev = new ObjectAnimationReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                    Sender = res.Sender
                };
                foreach(ObjectAnimation.AnimationData d in res.AnimationList)
                {
                    ev.AnimationData.Add(new LSLKey(d.AnimID));
                    ev.AnimationData.Add((int)d.AnimSequenceID);
                }
                vc.PostEvent(ev);
            }
        }

        [APIExtension("ViewerControl", "objectanimation_received")]
        [StateEventDelegate]
        public delegate void ObjectAnimationReceived(
            AgentInfo agent,
            LSLKey sender,
            AnArray animationData);
        #endregion

        #region moneybalancereply_received
        [TranslatedScriptEvent("moneybalancereply_received")]
        public class MoneyBalanceReplyReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey TransactionID;
            [TranslatedScriptEventParameter(2)]
            public int TransactionSuccess;
            [TranslatedScriptEventParameter(3)]
            public int MoneyBalance;
            [TranslatedScriptEventParameter(4)]
            public int SquareMetersCredit;
            [TranslatedScriptEventParameter(5)]
            public int SquareMetersCommitted;
            [TranslatedScriptEventParameter(6)]
            public string Description;
            [TranslatedScriptEventParameter(7)]
            public int TransactionType;
            [TranslatedScriptEventParameter(8)]
            public LSLKey SourceID;
            [TranslatedScriptEventParameter(9)]
            public int IsSourceGroup;
            [TranslatedScriptEventParameter(10)]
            public LSLKey DestID;
            [TranslatedScriptEventParameter(11)]
            public int IsDestGroup;
            [TranslatedScriptEventParameter(12)]
            public int Amount;
            [TranslatedScriptEventParameter(13)]
            public string ItemDescription;

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                var res = (MoneyBalanceReply)m;
                vc.PostEvent(new MoneyBalanceReplyReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                    TransactionID = res.TransactionID,
                    TransactionSuccess = res.TransactionSuccess.ToLSLBoolean(),
                    MoneyBalance = res.MoneyBalance,
                    SquareMetersCredit = res.SquareMetersCredit,
                    SquareMetersCommitted = res.SquareMetersCommitted,
                    Description = res.Description,
                    TransactionType = res.TransactionType,
                    SourceID = res.SourceID,
                    IsSourceGroup = res.IsSourceGroup.ToLSLBoolean(),
                    DestID = res.DestID,
                    IsDestGroup = res.IsDestGroup.ToLSLBoolean(),
                    Amount = res.Amount,
                    ItemDescription = res.ItemDescription
                });
            }
        }

        [APIExtension("ViewerControl", "moneybalancereply_received")]
        [StateEventDelegate]
        public delegate void MoneyBalanceReplyReceived(
            AgentInfo agent,
            LSLKey transactionID,
            int transactionSuccess,
            int moneyBalance,
            int squareMetersCredit,
            int squareMetersCommitted,
            string destination,
            int transactionType,
            LSLKey sourceID,
            int isSourceGroup,
            LSLKey destID,
            int isDestGroup,
            int amount,
            string itemDescription);
        #endregion

        #region grantgodlikepowers_received
        public class GrantGodlikePowersReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public int GodLevel;
            [TranslatedScriptEventParameter(2)]
            public LSLKey Token;

            public static void ToScriptEvent(Message m, ViewerConnection vc, uint circuitCode)
            {
                var res = (GrantGodlikePowers)m;
                vc.PostEvent(new GrantGodlikePowersReceivedEvent
                {
                    Agent = new AgentInfo(m, circuitCode),
                    GodLevel = res.GodLevel,
                    Token = res.Token
                });
            }
        }
        #endregion

        [TranslatedScriptEventsInfo]
        public static readonly Type[] TranslatedEvents = new Type[] {
            typeof(RegionHandshakeReceivedEvent),
            typeof(LogoutReplyReceivedEvent),
            typeof(TelehubInfoReceivedEvent),
            typeof(TeleportLocalReceivedEvent),
            typeof(EconomyDataReceivedEvent),
            typeof(TeleportProgressReceivedEvent),
            typeof(TeleportStartReceivedEvent),
            typeof(TeleportFailedReceivedEvent),
            typeof(AlertMessageReceivedEvent),
            typeof(AgentDataUpdateReceivedEvent),
            typeof(HealthMessageReceivedEvent),
            typeof(AvatarAnimationReceivedEvent),
            typeof(AvatarSitResponseReceivedEvent),
            typeof(CameraConstraintReceivedEvent),
            typeof(ClearFollowCamPropertiesReceivedEvent),
            typeof(SetFollowCamPropertiesReceivedEvent),
            typeof(ChatFromSimulatorReceivedEvent),
            typeof(EstateCovenantReplyReceivedEvent),
            typeof(LoadURLReceivedEvent),
            typeof(ScriptTeleportRequestReceivedEvent),
            typeof(ScriptQuestionReceivedEvent),
            typeof(ScriptDialogReceivedEvent),
            typeof(ScriptControlChangeReceivedEvent),
            typeof(PreloadSoundReceivedEvent),
            typeof(AttachedSoundReceivedEvent),
            typeof(SoundTriggerReceivedEvent),
            typeof(AttachedSoundGainChangeReceivedEvent),
            typeof(FeatureDisabledReceivedEvent),
            typeof(PayPriceReplyReceivedEvent),
            typeof(KillObjectReceivedEvent),
            typeof(OnlineNotificationReceivedEvent),
            typeof(OfflineNotificationReceivedEvent),
            typeof(ImprovedInstantMessageReceivedEvent),
            typeof(UUIDGroupNameReplyReceivedEvent),
            typeof(UUIDNameReplyReceivedEvent),
            typeof(DeRezAckReceivedEvent),
            typeof(ForceObjectSelectReceivedEvent),
            typeof(ObjectAnimationReceivedEvent),
            typeof(MoneyBalanceReplyReceivedEvent),
            typeof(GrantGodlikePowersReceivedEvent)
        };
    }
}

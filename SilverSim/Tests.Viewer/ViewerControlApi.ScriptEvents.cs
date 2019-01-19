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
using SilverSim.Scripting.Lsl.Api.Hashtable;
using SilverSim.Types;
using SilverSim.Viewer.Messages;
using SilverSim.Viewer.Messages.Agent;
using SilverSim.Viewer.Messages.Alert;
using SilverSim.Viewer.Messages.Avatar;
using SilverSim.Viewer.Messages.Camera;
using SilverSim.Viewer.Messages.Chat;
using SilverSim.Viewer.Messages.Circuit;
using SilverSim.Viewer.Messages.Common;
using SilverSim.Viewer.Messages.Economy;
using SilverSim.Viewer.Messages.Estate;
using SilverSim.Viewer.Messages.Friend;
using SilverSim.Viewer.Messages.God;
using SilverSim.Viewer.Messages.IM;
using SilverSim.Viewer.Messages.Inventory;
using SilverSim.Viewer.Messages.Names;
using SilverSim.Viewer.Messages.Object;
using SilverSim.Viewer.Messages.Parcel;
using SilverSim.Viewer.Messages.Region;
using SilverSim.Viewer.Messages.Script;
using SilverSim.Viewer.Messages.Simulator;
using SilverSim.Viewer.Messages.Sound;
using SilverSim.Viewer.Messages.Telehub;
using SilverSim.Viewer.Messages.Teleport;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace SilverSim.Tests.Viewer
{
    public partial class ViewerControlApi
    {
        #region agentmovementcomplete_received
        [TranslatedScriptEvent("agentmovementcomplete_received")]
        public class AgentMovementCompleteReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;

            [TranslatedScriptEventParameter(1)]
            public Vector3 Position;

            [TranslatedScriptEventParameter(2)]
            public Vector3 LookAt;

            [TranslatedScriptEventParameter(3)]
            public int GridX;

            [TranslatedScriptEventParameter(4)]
            public int GridY;

            [TranslatedScriptEventParameter(5)]
            public int Timestamp;

            [TranslatedScriptEventParameter(6)]
            public string ChannelVersion;

            public static void HandleAgentMovementComplete(Message m, ViewerConnection vc, ViewerAgentAccessor accessor)
            {
                var msg = (AgentMovementComplete)m;
                vc.PostEvent(new AgentMovementCompleteReceivedEvent
                {
                    Agent = accessor,
                    Position = msg.Position,
                    LookAt = msg.LookAt,
                    GridX = msg.GridPosition.GridX,
                    GridY = msg.GridPosition.GridY,
                    Timestamp = (int)msg.Timestamp,
                    ChannelVersion = msg.ChannelVersion
                });
            }
        }

        [APIExtension(ExtensionName, "agentmovementcomplete_received")]
        [StateEventDelegate]
        public delegate void AgentMovementCompleteReceived(
            [Description("Agent info")]
            ViewerAgentAccessor agent,
            Vector3 position,
            Vector3 lookat,
            int gridX,
            int gridY,
            int timestamp,
            string channelVersion);
        #endregion

        #region regioninfo_received event
        [APIExtension(ExtensionName, "regioninfodata")]
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
            public ViewerAgentAccessor Agent;

            [TranslatedScriptEventParameter(1)]
            public RegionInfoData RegionData;

            public static void HandleRegionInfo(Message m, ViewerConnection vc, ViewerAgentAccessor accessor)
            {
                var msg = (RegionInfo)m;
                vc.PostEvent(new RegionInfoReceivedEvent
                {
                    Agent = accessor,
                    RegionData = new RegionInfoData(msg)
                });
            }
        }

        [APIExtension(ExtensionName, "regioninfo_received")]
        [StateEventDelegate]
        public delegate void RegionInfoReceived(
            [Description("Agent info")]
            ViewerAgentAccessor agent,
            LSLKey regionID,
            RegionInfoData regionData);
        #endregion

        #region regionhandshake_received event
        [APIExtension(ExtensionName, "regionhandshakedata")]
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
            public ViewerAgentAccessor Agent;

            [TranslatedScriptEventParameter(1)]
            public LSLKey RegionID;

            [TranslatedScriptEventParameter(2)]
            public RegionHandshakeData RegionData;

            public static void HandleRegionHandshake(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var msg = (RegionHandshake)m;
                vc.PostEvent(new RegionHandshakeReceivedEvent
                {
                    Agent = agent,
                    RegionID = msg.RegionID,
                    RegionData = new RegionHandshakeData(msg)
                });
            }
        }

        [APIExtension(ExtensionName, "regionhandshake_received")]
        [StateEventDelegate]
        public delegate void RegionHandshakeReceived(
            [Description("Agent info")]
            ViewerAgentAccessor agent,
            LSLKey regionID,
            RegionHandshakeData regionData);
        #endregion

        #region logoutreply_received
        [TranslatedScriptEvent("logoutreply_received")]
        public class LogoutReplyReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent { get; }

            public LogoutReplyReceivedEvent(ViewerAgentAccessor accessor)
            {
                Agent = accessor;
            }
        }

        [APIExtension(ExtensionName, "logoutreply_received")]
        [StateEventDelegate]
        public delegate void LogoutReplyReceived(
            [Description("Agent info")]
            ViewerAgentAccessor agent);
        #endregion

        #region telehubinfo_received
        [TranslatedScriptEvent("telehubinfo_received")]
        public class TelehubInfoReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
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

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (TelehubInfo)m;
                var ev = new TelehubInfoReceivedEvent
                {
                    Agent = agent,
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

        [APIExtension(ExtensionName, "telehubinfo_received")]
        [StateEventDelegate]
        public delegate void TelehubInfoReceived(
            ViewerAgentAccessor agent,
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
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public Vector3 Position;
            [TranslatedScriptEventParameter(2)]
            public Vector3 LookAt;
            [TranslatedScriptEventParameter(3)]
            public int TeleportFlags;

            public static void ToScriptEvent(TeleportLocal m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                vc.PostEvent(new TeleportLocalReceivedEvent
                {
                    Agent = agent,
                    Position = m.Position,
                    LookAt = m.LookAt,
                    TeleportFlags = (int)m.TeleportFlags
                });
            }
        }

        [APIExtension(ExtensionName, "teleportlocal_received")]
        [StateEventDelegate]
        public delegate void TeleportLocalReceived(
            ViewerAgentAccessor agent,
            Vector3 position,
            Vector3 lookAt,
            int teleportFlags);
        #endregion

        #region teleportprogress_received
        [TranslatedScriptEvent("teleportprogress_received")]
        public class TeleportProgressReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public int TeleportFlags;
            [TranslatedScriptEventParameter(2)]
            public string Message;

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (TeleportProgress)m;
                vc.PostEvent(new TeleportProgressReceivedEvent
                {
                    Agent = agent,
                    TeleportFlags = (int)res.TeleportFlags,
                    Message = res.Message
                });
            }
        }

        [APIExtension(ExtensionName, "teleportprogress_received")]
        [StateEventDelegate]
        public delegate void TeleportProgressReceived(
            ViewerAgentAccessor agent,
            int teleportFlags,
            string message);
        #endregion

        #region teleportstart_received
        [TranslatedScriptEvent("teleportstart_received")]
        public class TeleportStartReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public int TeleportFlags;

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (TeleportStart)m;
                vc.PostEvent(new TeleportStartReceivedEvent
                {
                    Agent = agent,
                    TeleportFlags = (int)res.TeleportFlags
                });
            }
        }

        [APIExtension(ExtensionName, "teleportstart_received")]
        [StateEventDelegate]
        public delegate void TeleportStartReceived(
            ViewerAgentAccessor agent,
            int teleportFlags);
        #endregion

        #region teleportfailed_received
        [TranslatedScriptEvent("teleportfailed_received")]
        public class TeleportFailedReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey Reason;
            [TranslatedScriptEventParameter(2)]
            public AnArray AlertInfo = new AnArray();

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (TeleportFailed)m;
                var ev = new TeleportFailedReceivedEvent
                {
                    Agent = agent,
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

        [APIExtension(ExtensionName, "teleportfailed_received")]
        [StateEventDelegate]
        public delegate void TeleportFailedReceived(
            ViewerAgentAccessor agent,
            string reason,
            AnArray alertInfo);
        #endregion

        #region economydata_received
        [TranslatedScriptEvent("economydata_received")]
        public class EconomyDataReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                vc.PostEvent(new EconomyDataReceivedEvent
                {
                    Agent = agent
                });
            }
        }

        [APIExtension(ExtensionName, "economydata_received")]
        [StateEventDelegate]
        public delegate void EconomyDataReceived(
            ViewerAgentAccessor agent);
        #endregion

        #region alertmessage_received
        [TranslatedScriptEvent("alertmessage_received")]
        public class AlertMessageReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public string Message;
            [TranslatedScriptEventParameter(2)]
            public AnArray AlertInfo = new AnArray();

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (AlertMessage)m;
                var ev = new AlertMessageReceivedEvent
                {
                    Agent = agent,
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

        [APIExtension(ExtensionName, "alertmessage_received")]
        [StateEventDelegate]
        public delegate void AlertMessageReceived(
            ViewerAgentAccessor agent,
            string message,
            AnArray alertInfo);
        #endregion

        #region agentdataupdate_received
        [TranslatedScriptEvent("agentdataupdate_received")]
        public class AgentDataUpdateReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
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

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (AgentDataUpdate)m;
                vc.PostEvent(new AgentDataUpdateReceivedEvent
                {
                    Agent = agent,
                    FirstName = res.FirstName,
                    LastName = res.LastName,
                    GroupTitle = res.GroupTitle,
                    ActiveGroupID = res.ActiveGroupID,
                    GroupPowers = (long)res.GroupPowers,
                    GroupName = res.GroupName
                });
            }
        }

        [APIExtension(ExtensionName, "agentdataupdate_received")]
        [StateEventDelegate]
        public delegate void AgentDataUpdateReceived(
            ViewerAgentAccessor agent,
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
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey GroupID = new LSLKey();

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (AgentDropGroup)m;
                vc.PostEvent(new AgentDropGroupReceivedEvent
                {
                    Agent = agent,
                    GroupID = res.GroupID
                });
            }
        }

        [APIExtension(ExtensionName, "agentdropgroup_received")]
        [StateEventDelegate]
        public delegate void AgentDropGroupReceived(
            ViewerAgentAccessor agent,
            LSLKey groupID);
        #endregion

        #region coarselocationupdate_received
        [TranslatedScriptEvent("coarselocationupdate_received")]
        public class CoarseLocationUpdateReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public int You;
            [TranslatedScriptEventParameter(2)]
            public int Prey;
            [TranslatedScriptEventParameter(3)]
            public AnArray AgentData = new AnArray();

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (CoarseLocationUpdate)m;
                var ev = new CoarseLocationUpdateReceivedEvent
                {
                    Agent = agent,
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

        [APIExtension(ExtensionName, "coarselocationupdate_received")]
        [StateEventDelegate]
        public delegate void CoarseLocationUpdateReceived(
            ViewerAgentAccessor agent,
            int you,
            int prey,
            AnArray agentData);
        #endregion

        #region healthmessage_received
        [TranslatedScriptEvent("healthmessage_received")]
        public class HealthMessageReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public double Health;

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (HealthMessage)m;
                vc.PostEvent(new HealthMessageReceivedEvent
                {
                    Agent = agent,
                    Health = res.Health
                });
            }
        }

        [APIExtension(ExtensionName, "healthmessage_received")]
        [StateEventDelegate]
        public delegate void HealthMessageUpdateReceived(
            ViewerAgentAccessor agent,
            double health);
        #endregion

        #region avataranimation_received
        [APIExtension(ExtensionName, "avataranimationdata")]
        [APIDisplayName("avataranimationdata")]
        [APIIsVariableType]
        [APIAccessibleMembers]
        [APICloneOnAssignment]
        [Serializable]
        public sealed class AvatarAnimationDataBlock
        {
            public LSLKey AnimID = new LSLKey();
            public int SequenceID;
            public UUID ObjectID;
        }

        [APIExtension(ExtensionName, "avataranimationdatalist")]
        [APIDisplayName("avataranimationdatalist")]
        [APIIsVariableType]
        [APIAccessibleMembers("Count", "Length")]
        [Serializable]
        public sealed class AvatarAnimationDataList : List<AvatarAnimationDataBlock>
        {
            public int Length => Count;

            public sealed class LSLEnumerator : IEnumerator<AvatarAnimationDataBlock>
            {
                private readonly AvatarAnimationDataList Src;
                private int Position = -1;

                public LSLEnumerator(AvatarAnimationDataList src)
                {
                    Src = src;
                }

                public AvatarAnimationDataBlock Current => Src[Position];

                object IEnumerator.Current => Current;

                public void Dispose()
                {
                }

                public bool MoveNext() => ++Position < Src.Count;

                public void Reset() => Position = -1;
            }

            public LSLEnumerator GetLslForeachEnumerator() => new LSLEnumerator(this);
        }

        [TranslatedScriptEvent("avataranimation_received")]
        public class AvatarAnimationReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey Sender = new LSLKey();
            [TranslatedScriptEventParameter(2)]
            public AvatarAnimationDataList AnimationData = new AvatarAnimationDataList();

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (AvatarAnimation)m;
                var ev = new AvatarAnimationReceivedEvent
                {
                    Agent = agent,
                    Sender = res.Sender
                };

                foreach (AvatarAnimation.AnimationData ad in res.AnimationList)
                {
                    ev.AnimationData.Add(new AvatarAnimationDataBlock
                    {
                        AnimID = ad.AnimID,
                        SequenceID = (int)ad.AnimSequenceID,
                        ObjectID = ad.ObjectID
                    });
                }
                vc.PostEvent(ev);
            }
        }

        [APIExtension(ExtensionName, "avataranimation_received")]
        [StateEventDelegate]
        public delegate void AvatarAnimationReceived(
            ViewerAgentAccessor agent,
            LSLKey sender,
            AvatarAnimationDataList animationData);
        #endregion

        #region avatarsitresponse_received
        [TranslatedScriptEvent("avatarsitresponse_received")]
        public class AvatarSitResponseReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
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

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (AvatarSitResponse)m;
                vc.PostEvent(new AvatarSitResponseReceivedEvent
                {
                    Agent = agent,
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

        [APIExtension(ExtensionName, "avatarsitresponse_received")]
        [StateEventDelegate]
        public delegate void AvatarSitResponseReceived(
            ViewerAgentAccessor agent,
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
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public Quaternion CameraCollidePlane;

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (CameraConstraint)m;
                vc.PostEvent(new CameraConstraintReceivedEvent
                {
                    Agent = agent,
                    CameraCollidePlane = new Quaternion(res.CameraCollidePlane.X, res.CameraCollidePlane.Y, res.CameraCollidePlane.Z, res.CameraCollidePlane.W)
                });
            }
        }

        [APIExtension(ExtensionName, "cameraconstraint_received")]
        [StateEventDelegate]
        public delegate void CameraConstraintReceived(
            ViewerAgentAccessor agent,
            Quaternion cameraCollidePlane);
        #endregion

        #region clearfollowcamproperties_received
        [TranslatedScriptEvent("clearfollowcamproperties_received")]
        public class ClearFollowCamPropertiesReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey ObjectID = new LSLKey();

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (ClearFollowCamProperties)m;
                vc.PostEvent(new ClearFollowCamPropertiesReceivedEvent
                {
                    Agent = agent,
                    ObjectID = res.ObjectID
                });
            }
        }

        [APIExtension(ExtensionName, "clearfollowcamproperties_received")]
        [StateEventDelegate]
        public delegate void ClearFollowCamPropertiesReceived(
            ViewerAgentAccessor agent,
            LSLKey objectID);
        #endregion

        #region setfollowcamproperties_received
        [TranslatedScriptEvent("setfollowcamproperties_received")]
        public class SetFollowCamPropertiesReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey ObjectID = new LSLKey();
            [TranslatedScriptEventParameter(2)]
            public AnArray CameraParams = new AnArray();

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (SetFollowCamProperties)m;
                var ev = new SetFollowCamPropertiesReceivedEvent
                {
                    Agent = agent,
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

        [APIExtension(ExtensionName, "setfollowcamproperties_received")]
        [StateEventDelegate]
        public delegate void SetFollowCamPropertiesReceived(
            ViewerAgentAccessor agent,
            LSLKey objectID,
            AnArray cameraParams);
        #endregion

        #region chatfromsimulator_received
        [APIExtension("ViewerControl")]
        public const int CHAT_SOURCE_TYPE_SYSTEM = 0;
        [APIExtension("ViewerControl")]
        public const int CHAT_SOURCE_TYPE_AGENT = 1;
        [APIExtension("ViewerControl")]
        public const int CHAT_SOURCE_TYPE_OBJECT = 2;

        [TranslatedScriptEvent("chatfromsimulator_received")]
        public class ChatFromSimulatorReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
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

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (ChatFromSimulator)m;
                vc.PostEvent(new ChatFromSimulatorReceivedEvent
                {
                    Agent = agent,
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

        [APIExtension(ExtensionName, "chatfromsimulator_received")]
        [StateEventDelegate]
        public delegate void ChatFromSimulatorReceived(
            ViewerAgentAccessor agent,
            string fromName,
            LSLKey sourceID,
            LSLKey ownerID,
            int sourceType,
            int chatType,
            int audibleLevel,
            Vector3 position,
            string message);
        #endregion

        #region estatecovenantreply_received
        [TranslatedScriptEvent("estatecovenantreply_received")]
        public class EstateCovenantReplyReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey CovenantID = new LSLKey();
            [TranslatedScriptEventParameter(2)]
            public long CovenantTimestamp;
            [TranslatedScriptEventParameter(3)]
            public string EstateName;
            [TranslatedScriptEventParameter(4)]
            public LSLKey EstateOwnerID = new LSLKey();

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (EstateCovenantReply)m;
                vc.PostEvent(new EstateCovenantReplyReceivedEvent
                {
                    Agent = agent,
                    CovenantID = res.CovenantID,
                    CovenantTimestamp = res.CovenantTimestamp,
                    EstateName = res.EstateName,
                    EstateOwnerID = res.EstateOwnerID
                });
            }
        }

        [APIExtension(ExtensionName, "estatecovenantreply_received")]
        [StateEventDelegate]
        public delegate void EstateCovenantReplyReceived(
            ViewerAgentAccessor agent,
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
            public ViewerAgentAccessor Agent;
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

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (LoadURL)m;
                vc.PostEvent(new LoadURLReceivedEvent
                {
                    Agent = agent,
                    ObjectName = res.ObjectName,
                    ObjectID = res.ObjectID,
                    OwnerID = res.OwnerID,
                    OwnerIsGroup = res.OwnerIsGroup.ToLSLBoolean(),
                    Message = res.Message,
                    URL = res.URL
                });
            }
        }

        [APIExtension(ExtensionName, "loadurl_received")]
        [StateEventDelegate]
        public delegate void LoadURLReceived(
            ViewerAgentAccessor agent,
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
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public string ObjectName = string.Empty;
            [TranslatedScriptEventParameter(2)]
            public string SimName = string.Empty;
            [TranslatedScriptEventParameter(3)]
            public Vector3 SimPosition;
            [TranslatedScriptEventParameter(4)]
            public Vector3 LookAt;

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (ScriptTeleportRequest)m;
                vc.PostEvent(new ScriptTeleportRequestReceivedEvent
                {
                    Agent = agent,
                    ObjectName = res.ObjectName,
                    SimName = res.SimName,
                    SimPosition = res.SimPosition,
                    LookAt = res.LookAt
                });
            }
        }

        [APIExtension(ExtensionName, "scriptteleportrequest_received")]
        [StateEventDelegate]
        public delegate void ScriptTeleportRequestReceived(
            ViewerAgentAccessor agent,
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
            public ViewerAgentAccessor Agent;
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

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (ScriptQuestion)m;
                vc.PostEvent(new ScriptQuestionReceivedEvent
                {
                    Agent = agent,
                    TaskID = res.TaskID,
                    ItemID = res.ItemID,
                    ObjectName = res.ObjectName,
                    ObjectOwner = res.ObjectOwner,
                    Questions = (int)res.Questions,
                    ExperienceID = res.ExperienceID
                });
            }
        }

        [APIExtension(ExtensionName, "scriptquestion_received")]
        [StateEventDelegate]
        public delegate void ScriptQuestionReceived(
            ViewerAgentAccessor agent,
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
            public ViewerAgentAccessor Agent;
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

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (ScriptDialog)m;
                var ev = new ScriptDialogReceivedEvent
                {
                    Agent = agent,
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

        [APIExtension(ExtensionName, "scriptdialog_received")]
        [StateEventDelegate]
        public delegate void ScriptDialogReceived(
            ViewerAgentAccessor agent,
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
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public AnArray ControlData = new AnArray();

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (ScriptControlChange)m;
                var ev = new ScriptControlChangeReceivedEvent
                {
                    Agent = agent
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

        [APIExtension(ExtensionName, "scriptcontrolchange_received")]
        [StateEventDelegate]
        public delegate void ScriptControlChangeReceived(ViewerAgentAccessor agent, AnArray controlData);
        #endregion

        #region preloadsound_received
        [TranslatedScriptEvent("preloadsound_received")]
        public class PreloadSoundReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey ObjectID = new LSLKey();
            [TranslatedScriptEventParameter(2)]
            public LSLKey OwnerID = new LSLKey();
            [TranslatedScriptEventParameter(3)]
            public LSLKey SoundID = new LSLKey();

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (PreloadSound)m;
                vc.PostEvent(new PreloadSoundReceivedEvent
                {
                    Agent = agent,
                    ObjectID = res.ObjectID,
                    OwnerID = res.OwnerID,
                    SoundID = res.SoundID
                });
            }
        }

        [APIExtension(ExtensionName, "preloadsound_received")]
        [StateEventDelegate]
        public delegate void PreloadsoundReceived(
            ViewerAgentAccessor agent,
            LSLKey objectId,
            LSLKey ownerID,
            LSLKey soundID);
        #endregion

        #region attachedsound_received
        [TranslatedScriptEvent("attachedsound_received")]
        public class AttachedSoundReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
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

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (AttachedSound)m;
                vc.PostEvent(new AttachedSoundReceivedEvent
                {
                    Agent = agent,
                    SoundID = res.SoundID,
                    ObjectID = res.ObjectID,
                    OwnerID = res.OwnerID,
                    Gain = res.Gain,
                    Flags = (int)res.Flags
                });
            }
        }

        [APIExtension(ExtensionName, "attachedsound_received")]
        [StateEventDelegate]
        public delegate void AttachedSoundReceived(
            ViewerAgentAccessor agent,
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
            public ViewerAgentAccessor Agent;
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

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (SoundTrigger)m;
                vc.PostEvent(new SoundTriggerReceivedEvent
                {
                    Agent = agent,
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

        [APIExtension(ExtensionName, "soundtrigger_received")]
        [StateEventDelegate]
        public delegate void SoundTriggerReceived(
            ViewerAgentAccessor agent,
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
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey ObjectID = new LSLKey();
            [TranslatedScriptEventParameter(2)]
            public double Gain;

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (AttachedSoundGainChange)m;
                vc.PostEvent(new AttachedSoundGainChangeReceivedEvent
                {
                    Agent = agent,
                    ObjectID = res.ObjectID,
                    Gain = res.Gain
                });
            }
        }

        [APIExtension(ExtensionName, "attachedsoundgainchange_received")]
        [StateEventDelegate]
        public delegate void AttachedSoundGainChangeReceived(
            ViewerAgentAccessor agent,
            LSLKey objectID,
            double gain);
        #endregion

        #region featuredisabled_received
        [TranslatedScriptEvent("featuredisabled_received")]
        public class FeatureDisabledReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey TransactionID = new LSLKey();
            [TranslatedScriptEventParameter(2)]
            public string ErrorMessage = string.Empty;

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (FeatureDisabled)m;
                vc.PostEvent(new FeatureDisabledReceivedEvent
                {
                    Agent = agent,
                    TransactionID = res.TransactionID,
                    ErrorMessage = res.ErrorMessage
                });
            }
        }

        [APIExtension(ExtensionName, "featuredisabled_received")]
        [StateEventDelegate]
        public delegate void FeatureDisabledReceived(
            ViewerAgentAccessor agent,
            LSLKey transactionID,
            string errorMessage);
        #endregion

        #region paypricereply_received
        [TranslatedScriptEvent("paypricereply_received")]
        public class PayPriceReplyReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey ObjectID;
            [TranslatedScriptEventParameter(2)]
            public int DefaultPayPrice;
            [TranslatedScriptEventParameter(3)]
            public AnArray ButtonData = new AnArray();

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (PayPriceReply)m;
                var ev = new PayPriceReplyReceivedEvent
                {
                    Agent = agent,
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

        [APIExtension(ExtensionName, "paypricereply_received")]
        [StateEventDelegate]
        public delegate void PayPriceReplyReceived(
            ViewerAgentAccessor agent,
            LSLKey objectID,
            int defaultPayPrice,
            AnArray buttonData);
        #endregion

        #region killobject_received
        [TranslatedScriptEvent("killobject_received")]
        public class KillObjectReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public AnArray LocalIDs = new AnArray();

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (KillObject)m;
                var ev = new KillObjectReceivedEvent
                {
                    Agent = agent
                };
                foreach(uint localid in res.LocalIDs)
                {
                    ev.LocalIDs.Add((int)localid);
                }
                vc.PostEvent(ev);
            }
        }

        [APIExtension(ExtensionName, "killobject_received")]
        [StateEventDelegate]
        public delegate void KillObjectReceived(
            ViewerAgentAccessor agent,
            AnArray localids);
        #endregion

        #region onlinenotification_received
        [TranslatedScriptEvent("onlinenotification_received")]
        public class OnlineNotificationReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public AnArray Agents = new AnArray();

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                OnlineNotification res = (OnlineNotification)m;
                var ev = new OnlineNotificationReceivedEvent
                {
                    Agent = agent,
                };
                foreach(UUID id in res.AgentIDs)
                {
                    ev.Agents.Add(new LSLKey(id));
                }
                vc.PostEvent(ev);
            }
        }

        [APIExtension(ExtensionName, "onlinenotification_received")]
        [StateEventDelegate]
        public delegate void OnlineNotificationReceived(
            ViewerAgentAccessor agent,
            AnArray agents);
        #endregion

        #region offlinenotification_received
        [TranslatedScriptEvent("offlinenotification_received")]
        public class OfflineNotificationReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public AnArray Agents = new AnArray();

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                OnlineNotification res = (OnlineNotification)m;
                var ev = new OnlineNotificationReceivedEvent
                {
                    Agent = agent,
                };
                foreach (UUID id in res.AgentIDs)
                {
                    ev.Agents.Add(new LSLKey(id));
                }
                vc.PostEvent(ev);
            }
        }

        [APIExtension(ExtensionName, "offlinenotification_received")]
        [StateEventDelegate]
        public delegate void OfflineNotificationReceived(
            ViewerAgentAccessor agent,
            AnArray agents);
        #endregion

        #region improvedinstantmessage_received
        [TranslatedScriptEvent("improvedinstantmessage_received")]
        public class ImprovedInstantMessageReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
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

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (ImprovedInstantMessage)m;
                vc.PostEvent(new ImprovedInstantMessageReceivedEvent
                {
                    Agent = agent,
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

        [APIExtension(ExtensionName, "improvedinstantmessage_received")]
        [StateEventDelegate]
        public delegate void ImprovedInstantMessageReceived(
            ViewerAgentAccessor agent,
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
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public AnArray Data = new AnArray();

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (UUIDGroupNameReply)m;
                var ev = new UUIDGroupNameReplyReceivedEvent
                {
                    Agent = agent
                };
                foreach(UUIDGroupNameReply.Data d in res.UUIDNameBlock)
                {
                    ev.Data.Add(new LSLKey(d.ID));
                    ev.Data.Add(d.GroupName);
                }
                vc.PostEvent(ev);
            }
        }

        [APIExtension(ExtensionName, "uuidgroupnamereply_received")]
        [StateEventDelegate]
        public delegate void UUIDGroupNameReplyReceived(
            ViewerAgentAccessor agent,
            AnArray data);
        #endregion

        #region uuidnamereply_received
        [TranslatedScriptEvent("uuidnamereply_received")]
        public class UUIDNameReplyReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public AnArray Data = new AnArray();

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (UUIDNameReply)m;
                var ev = new UUIDNameReplyReceivedEvent
                {
                    Agent = agent
                };
                foreach(UUIDNameReply.Data d in res.UUIDNameBlock)
                {
                    ev.Data.Add(new LSLKey(d.ID));
                    ev.Data.Add(d.FirstName + " " + d.LastName);
                }
                vc.PostEvent(ev);
            }
        }

        [APIExtension(ExtensionName, "uuidnamereply_received")]
        [StateEventDelegate]
        public delegate void UUIDNameReplyReceived(
            ViewerAgentAccessor agent, 
            AnArray data);
        #endregion

        #region derezack_received
        [TranslatedScriptEvent("derezack_received")]
        public class DeRezAckReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey TransactionID;
            [TranslatedScriptEventParameter(2)]
            public int Success;

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (DeRezAck)m;
                vc.PostEvent(new DeRezAckReceivedEvent
                {
                    Agent = agent,
                    TransactionID = res.TransactionID,
                    Success = res.Success.ToLSLBoolean()
                });
            }
        }

        [APIExtension(ExtensionName, "derezack_received")]
        [StateEventDelegate]
        public delegate void DeRezAckReceived(
            ViewerAgentAccessor agent,
            LSLKey transactionID,
            int success);
        #endregion

        #region forceobjectselect_received
        [TranslatedScriptEvent("forceobjectselect_received")]
        public class ForceObjectSelectReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public int ResetList;
            [TranslatedScriptEventParameter(2)]
            public AnArray LocalIDs = new AnArray();

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (ForceObjectSelect)m;
                var ev = new ForceObjectSelectReceivedEvent
                {
                    Agent = agent,
                    ResetList = res.ResetList.ToLSLBoolean()
                };

                foreach(uint id in res.LocalIDs)
                {
                    ev.LocalIDs.Add((int)id);
                }
                vc.PostEvent(ev);
            }
        }

        [APIExtension(ExtensionName, "forceobjectselect_received")]
        [StateEventDelegate]
        public delegate void ForceObjectSelectReceived(
            ViewerAgentAccessor agent,
            int resetList,
            AnArray localIDs);
        #endregion

        #region objectanimation_received
        [APIExtension(ExtensionName, "objectanimationdata")]
        [APIDisplayName("objectanimationdata")]
        [APIIsVariableType]
        [APIAccessibleMembers]
        [APICloneOnAssignment]
        [Serializable]
        public sealed class ObjectAnimationDataBlock
        {
            public LSLKey AnimID = new LSLKey();
            public int SequenceID;
        }

        [APIExtension(ExtensionName, "objectanimationdatalist")]
        [APIDisplayName("objectanimationdatalist")]
        [APIIsVariableType]
        [APIAccessibleMembers("Count", "Length")]
        [Serializable]
        public sealed class ObjectAnimationDataList : List<ObjectAnimationDataBlock>
        {
            public int Length => Count;

            public sealed class LSLEnumerator : IEnumerator<ObjectAnimationDataBlock>
            {
                private readonly ObjectAnimationDataList Src;
                private int Position = -1;

                public LSLEnumerator(ObjectAnimationDataList src)
                {
                    Src = src;
                }

                public ObjectAnimationDataBlock Current => Src[Position];

                object IEnumerator.Current => Current;

                public void Dispose()
                {
                }

                public bool MoveNext() => ++Position < Src.Count;

                public void Reset() => Position = -1;
            }

            public LSLEnumerator GetLslForeachEnumerator() => new LSLEnumerator(this);
        }

        [TranslatedScriptEvent("objectanimation_received")]
        public class ObjectAnimationReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey Sender;
            [TranslatedScriptEventParameter(2)]
            public ObjectAnimationDataList AnimationData = new ObjectAnimationDataList();

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (ObjectAnimation)m;
                var ev = new ObjectAnimationReceivedEvent
                {
                    Agent = agent,
                    Sender = res.Sender
                };
                foreach(ObjectAnimation.AnimationData d in res.AnimationList)
                {
                    ev.AnimationData.Add(new ObjectAnimationDataBlock
                    {
                        AnimID = d.AnimID,
                        SequenceID = (int)d.AnimSequenceID
                    });
                }
                vc.PostEvent(ev);
            }
        }

        [APIExtension(ExtensionName, "objectanimation_received")]
        [StateEventDelegate]
        public delegate void ObjectAnimationReceived(
            ViewerAgentAccessor agent,
            LSLKey sender,
            ObjectAnimationDataList animationData);
        #endregion

        #region moneybalancereply_received
        [TranslatedScriptEvent("moneybalancereply_received")]
        public class MoneyBalanceReplyReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
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

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (MoneyBalanceReply)m;
                vc.PostEvent(new MoneyBalanceReplyReceivedEvent
                {
                    Agent = agent,
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

        [APIExtension(ExtensionName, "moneybalancereply_received")]
        [StateEventDelegate]
        public delegate void MoneyBalanceReplyReceived(
            ViewerAgentAccessor agent,
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
        [TranslatedScriptEvent("grantgodlikepowers_received")]
        public class GrantGodlikePowersReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public int GodLevel;
            [TranslatedScriptEventParameter(2)]
            public LSLKey Token;

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (GrantGodlikePowers)m;
                vc.PostEvent(new GrantGodlikePowersReceivedEvent
                {
                    Agent = agent,
                    GodLevel = res.GodLevel,
                    Token = res.Token
                });
            }
        }

        [APIExtension(ExtensionName, "grantgodlikepowers_received")]
        [StateEventDelegate]
        public delegate void GrantGodlikePowersReceived(
            ViewerAgentAccessor agent,
            int godLevel,
            LSLKey token);
        #endregion

        #region objectupdate_received
        [TranslatedScriptEvent("objectupdate_received")]
        public class ObjectUpdateReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public double TimeDilation;
            [TranslatedScriptEventParameter(2)]
            public readonly VcObjectDataList ObjectList = new VcObjectDataList();

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                ObjectUpdate ou = m as ObjectUpdate;
                if (ou != null)
                {
                    var ev = new ObjectUpdateReceivedEvent
                    {
                        Agent = agent,
                        TimeDilation = ou.TimeDilation / 65535.0
                    };

                    foreach (UnreliableObjectUpdate.ObjData d in ou.ObjectData)
                    {
                        ev.ObjectList.Add(new VcObjectData(d));
                    }
                    vc.PostEvent(ev);
                }
                ObjectUpdateCompressed ouc = m as ObjectUpdateCompressed;
                if(ouc != null)
                {
                    var ev = new ObjectUpdateReceivedEvent
                    {
                        Agent = agent,
                        TimeDilation = ouc.TimeDilation / 65535.0
                    };

                    foreach (ObjectUpdateCompressed.ObjData d in ouc.ObjectData)
                    {
                        ev.ObjectList.Add(new VcObjectData(d.Data, d.UpdateFlags));
                    }
                    vc.PostEvent(ev);
                }
            }
        }

        [APIExtension(ExtensionName, "objectupdate_received")]
        [StateEventDelegate]
        public delegate void ObjectUpdateReceived(
            ViewerAgentAccessor agent,
            double timeDilation,
            VcObjectDataList objectlist);
        #endregion

        #region parcelinforeply_received
        [APIExtension(ExtensionName, "parcelinforeplydata")]
        [APIDisplayName("parcelinforeplydata")]
        [APIIsVariableType]
        [APIAccessibleMembers]
        [Serializable]
        public class ParcelInfoReplyData
        {
            public LSLKey ParcelID { get; }
            public LSLKey OwnerID { get; }
            public string Name { get; }
            public string Description { get; }
            public int ActualArea { get; }
            public int BillableArea { get; }
            public int Flags { get; }
            public Vector3 GlobalPos { get; }
            public string SimName { get; }
            public LSLKey SnapshotID { get; }
            public double Dwell { get; }
            public int SalePrice { get; }
            public int AuctionID { get; }

            public ParcelInfoReplyData(ParcelInfoReply msg)
            {
                ParcelID = new UUID(msg.ParcelID.GetBytes(), 0);
                OwnerID = msg.OwnerID;
                Name = msg.Name;
                Description = msg.Description;
                ActualArea = msg.ActualArea;
                BillableArea = msg.BillableArea;
                Flags = msg.Flags;
                GlobalPos = msg.GlobalPosition;
                SimName = msg.SimName;
                SnapshotID = msg.SnapshotID;
                Dwell = msg.Dwell;
                SalePrice = msg.SalePrice;
                AuctionID = (int)msg.AuctionID;
            }
        }

        [TranslatedScriptEvent("parcelinforeply_received")]
        public class ParcelInfoReplyReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public ParcelInfoReplyData ParcelData;

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (ParcelInfoReply)m;
                vc.PostEvent(new ParcelInfoReplyReceivedEvent
                {
                    Agent = agent,
                    ParcelData = new ParcelInfoReplyData(res)
                });
            }
        }

        [APIExtension(ExtensionName, "parcelinforeply_received")]
        [StateEventDelegate]
        public delegate void ParcelInfoReplyReceived(
            ViewerAgentAccessor agent,
            ParcelInfoReplyData parcelData);
        #endregion

        #region parcelobjectownersreply_received
        [APIExtension(ExtensionName, "parcelobjectownersreplydata")]
        [APIDisplayName("parcelobjectownersreplydata")]
        [APIIsVariableType]
        [APIAccessibleMembers]
        [Serializable]
        public class ParcelObjectOwnersReplyData
        {
            public LSLKey OwnerID { get; }
            public bool IsGroupOwned { get; }
            public int Count { get; }
            public bool IsOnline { get; }

            public ParcelObjectOwnersReplyData(ParcelObjectOwnersReply.DataEntry d)
            {
                OwnerID = d.OwnerID;
                IsGroupOwned = d.IsGroupOwned;
                Count = d.Count;
                IsOnline = d.IsOnline;
            }
        }

        [APIExtension("ViewerControl", "parcelobjectownersreplydatalist")]
        [APIDisplayName("parcelobjectownersreplydatalist")]
        [APIIsVariableType]
        [APIAccessibleMembers]
        [Serializable]
        public class ParcelObjectOwnersReplyDataList : List<ParcelObjectOwnersReplyData>
        {
            public int Length => Count;

            public sealed class LSLEnumerator : IEnumerator<ParcelObjectOwnersReplyData>
            {
                private readonly ParcelObjectOwnersReplyDataList Src;
                private int Position = -1;

                public LSLEnumerator(ParcelObjectOwnersReplyDataList src)
                {
                    Src = src;
                }

                public ParcelObjectOwnersReplyData Current => Src[Position];

                object IEnumerator.Current => Current;

                public void Dispose()
                {
                }

                public bool MoveNext() => ++Position < Src.Count;

                public void Reset() => Position = -1;
            }

            public LSLEnumerator GetLslForeachEnumerator() => new LSLEnumerator(this);
        }

        [TranslatedScriptEvent("parcelobjectownersreply_received")]
        public class ParcelObjectOwnersReplyReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public ParcelObjectOwnersReplyDataList Data = new ParcelObjectOwnersReplyDataList();

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (ParcelObjectOwnersReply)m;
                var e = new ParcelObjectOwnersReplyReceivedEvent
                {
                    Agent = agent
                };

                foreach(ParcelObjectOwnersReply.DataEntry entry in res.Data)
                {
                    e.Data.Add(new ParcelObjectOwnersReplyData(entry));
                }

                vc.PostEvent(e);
            }
        }

        [APIExtension(ExtensionName, "parcelobjectownersreply_received")]
        [StateEventDelegate]
        public delegate void ParcelObjectOwnersReplyReceived(
            ViewerAgentAccessor agent,
            ParcelObjectOwnersReplyDataList data);
        #endregion

        #region simulatorviewertimemessage_received
        [TranslatedScriptEvent("simulatorviewertimemessage_received")]
        public class SimulatorViewerTimeMessageReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public long UsecSinceStart;
            [TranslatedScriptEventParameter(2)]
            public int SecPerDay;
            [TranslatedScriptEventParameter(3)]
            public int SecPerYear;
            [TranslatedScriptEventParameter(4)]
            public Vector3 SunDirection;
            [TranslatedScriptEventParameter(5)]
            public double SunPhase;
            [TranslatedScriptEventParameter(6)]
            public Vector3 SunAngVelocity;

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (SimulatorViewerTimeMessage)m;
                vc.PostEvent(new SimulatorViewerTimeMessageReceivedEvent
                {
                    Agent = agent,
                    UsecSinceStart = (long)res.UsecSinceStart,
                    SecPerDay = (int)res.SecPerDay,
                    SecPerYear = (int)res.SecPerYear,
                    SunDirection = res.SunDirection,
                    SunPhase = res.SunPhase,
                    SunAngVelocity = res.SunAngVelocity
                });
            }
        }

        [APIExtension(ExtensionName, "simulatorviewertimemessage_received")]
        [StateEventDelegate]
        public delegate void SimulatorViewerTimeMessageReceived(
            ViewerAgentAccessor agent,
            long usecSinceStart,
            int secPerDay,
            int secPerYear,
            Vector3 sunDirection,
            double sunPhase,
            Vector3 sunAngVelocity);
        #endregion

        #region scriptrunningreply_received
        [TranslatedScriptEvent("scriptrunningreply_received")]
        public class ScriptRunningReplyReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey ObjectID;
            [TranslatedScriptEventParameter(2)]
            public LSLKey ItemID;
            [TranslatedScriptEventParameter(3)]
            public int IsRunning;

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (ScriptRunningReply)m;
                vc.PostEvent(new ScriptRunningReplyReceivedEvent
                {
                    Agent = agent,
                    ObjectID = res.ObjectID,
                    ItemID = res.ItemID,
                    IsRunning = res.IsRunning.ToLSLBoolean()
                });
            }
        }

        [APIExtension(ExtensionName, "scriptrunningreply_received")]
        [StateEventDelegate]
        public delegate void ScriptRunningReplyReceived(
            ViewerAgentAccessor agent,
            LSLKey objectID,
            LSLKey itemID,
            int isRunning);
        #endregion

        #region parcelproperties_received
        [APIExtension(ExtensionName, "parcelproperties")]
        [APIDisplayName("parcelproperties")]
        [APIIsVariableType]
        [APIAccessibleMembers]
        public sealed class ParcelPropertiesData
        {
            public int SelfCount;
            public int OtherCount;
            public int PublicCount;
            public int LocalID;
            public LSLKey OwnerID = new LSLKey();
            public int IsGroupOwned;
            public int AuctionID;
            public long ClaimDate;
            public int ClaimPrice;
            public int RentPrice;
            public Vector3 AABBMin;
            public Vector3 AABBMax;
            public ByteArrayApi.ByteArray Bitmap = new ByteArrayApi.ByteArray();
            public int Area;
            public int Status;
            public int SimWideMaxPrims;
            public int SimWideTotalPrims;
            public int MaxPrims;
            public int TotalPrims;
            public int OwnerPrims;
            public int GroupPrims;
            public int OtherPrims;
            public int SelectedPrims;
            public double ParcelPrimBonus;
            public int OtherCleanTime;
            public int ParcelFlags;
            public int SalePrice;
            public string Name = string.Empty;
            public string Description = string.Empty;
            public string MusicURL = string.Empty;
            public string MediaURL = string.Empty;
            public LSLKey MediaID = new LSLKey();
            public int MediaAutoscale;
            public LSLKey GroupID = new LSLKey();
            public int PassPrice;
            public double PassHours;
            public int Category;
            public LSLKey AuthBuyerID = new LSLKey();
            public LSLKey SnapshotID = new LSLKey();
            public Vector3 UserLocation;
            public Vector3 UserLookAt;
            public int LandingType;
            public int RegionPushOverride;
            public int RegionDenyAnonymous;
            public int RegionDenyIdentified;
            public int RegionDenyTransacted;
            public int RegionDenyAgeUnverified;
            public int Privacy;
            public int SeeAVs;
            public int AnyAVSounds;
            public int GroupAVSounds;
            public string MediaDesc = string.Empty;
            public int MediaWidth;
            public int MediaHeight;
            public int MediaLoop;
            public string MediaType = string.Empty;
            public int ObscureMedia;
            public int ObscureMusic;

            public ParcelPropertiesData()
            {
            }

            public ParcelPropertiesData(ParcelProperties msg)
            {
                SelfCount = msg.SelfCount;
                OtherCount = msg.OtherCount;
                PublicCount = msg.PublicCount;
                LocalID = msg.LocalID;
                OwnerID = msg.OwnerID;
                IsGroupOwned = msg.IsGroupOwned.ToLSLBoolean();
                AuctionID = (int)msg.AuctionID;
                ClaimDate = (long)msg.ClaimDate.DateTimeToUnixTime();
                ClaimPrice = msg.ClaimPrice;
                RentPrice = msg.RentPrice;
                AABBMin = msg.AABBMin;
                AABBMax = msg.AABBMax;
                Bitmap = new ByteArrayApi.ByteArray(msg.Bitmap);
                Area = msg.Area;
                Status = (int)msg.Status;
                SimWideMaxPrims = msg.SimWideMaxPrims;
                SimWideTotalPrims = msg.SimWideTotalPrims;
                MaxPrims = msg.MaxPrims;
                TotalPrims = msg.TotalPrims;
                OwnerPrims = msg.OwnerPrims;
                GroupPrims = msg.GroupPrims;
                OtherPrims = msg.OtherPrims;
                SelectedPrims = msg.SelectedPrims;
                ParcelPrimBonus = msg.ParcelPrimBonus;
                OtherCleanTime = msg.OtherCleanTime;
                ParcelFlags = (int)msg.ParcelFlags;
                SalePrice = msg.SalePrice;
                Name = msg.Name;
                Description = msg.Description;
                MusicURL = msg.MusicURL;
                MediaURL = msg.MediaURL;
                MediaID = msg.MediaID;
                MediaAutoscale = msg.MediaAutoScale.ToLSLBoolean();
                GroupID = msg.GroupID;
                PassPrice = msg.PassPrice;
                PassHours = msg.PassHours;
                Category = (int)msg.Category;
                AuthBuyerID = msg.AuthBuyerID;
                SnapshotID = msg.SnapshotID;
                UserLocation = msg.UserLocation;
                UserLookAt = msg.UserLookAt;
                LandingType = (int)msg.LandingType;
                RegionPushOverride = msg.RegionPushOverride.ToLSLBoolean();
                RegionDenyAnonymous = msg.RegionDenyAnonymous.ToLSLBoolean();
                RegionDenyIdentified = msg.RegionDenyIdentified.ToLSLBoolean();
                RegionDenyTransacted = msg.RegionDenyTransacted.ToLSLBoolean();
                RegionDenyAgeUnverified = msg.RegionDenyAgeUnverified.ToLSLBoolean();
                Privacy = msg.Privacy.ToLSLBoolean();
                SeeAVs = msg.SeeAVs.ToLSLBoolean();
                AnyAVSounds = msg.AnyAVSounds.ToLSLBoolean();
                GroupAVSounds = msg.GroupAVSounds.ToLSLBoolean();
                MediaDesc = msg.MediaDesc;
                MediaWidth = msg.MediaWidth;
                MediaHeight = msg.MediaHeight;
                MediaLoop = msg.MediaLoop.ToLSLBoolean();
                MediaType = msg.MediaType;
                ObscureMedia = msg.ObscureMedia.ToLSLBoolean();
                ObscureMusic = msg.ObscureMusic.ToLSLBoolean();
            }
        }

        [TranslatedScriptEvent("parcelproperties_received")]
        public class ParcelPropertiesReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public int RequestResult;
            [TranslatedScriptEventParameter(2)]
            public int SequenceID;
            [TranslatedScriptEventParameter(3)]
            public int SnapSelection;
            [TranslatedScriptEventParameter(4)]
            public ParcelPropertiesData ParcelData;

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (ParcelProperties)m;
                vc.PostEvent(new ParcelPropertiesReceivedEvent
                {
                    Agent = agent,
                    RequestResult = (int)res.RequestResult,
                    SequenceID = res.SequenceID,
                    SnapSelection = res.SnapSelection.ToLSLBoolean(),
                    ParcelData = new ParcelPropertiesData(res)
                });
            }
        }

        [APIExtension(ExtensionName, "parcelproperties_received")]
        [StateEventDelegate]
        public delegate void ParcelPropertiesReeived(
            ViewerAgentAccessor agent,
            int requestResult,
            int sequenceID,
            int snapSelection,
            ParcelPropertiesData parcelData);
        #endregion

        #region crossedregion_received
        [TranslatedScriptEvent("crossedregion_received")]
        public class CrossedRegionReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public int GridPositionX;
            [TranslatedScriptEventParameter(2)]
            public int GridPositionY;
            [TranslatedScriptEventParameter(3)]
            public string SimIP;
            [TranslatedScriptEventParameter(4)]
            public int SimPort;
            [TranslatedScriptEventParameter(5)]
            public int RegionSizeX;
            [TranslatedScriptEventParameter(6)]
            public int RegionSizeY;

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (CrossedRegion)m;
                vc.PostEvent(new CrossedRegionReceivedEvent
                {
                    Agent = agent,
                    GridPositionX = (int)res.GridPosition.X,
                    GridPositionY = (int)res.GridPosition.Y,
                    SimIP = res.SimIP.ToString(),
                    SimPort = res.SimPort,
                    RegionSizeX = (int)res.RegionSize.X,
                    RegionSizeY = (int)res.RegionSize.Y
                });
            }
        }

        [APIExtension(ExtensionName, "crossedregion_received")]
        [StateEventDelegate]
        public delegate void CrossedRegionReceived(
            ViewerAgentAccessor agent,
            int gridPosX,
            int gridPosY,
            string simIP,
            int simPort,
            int regionSizeX,
            int regionSizeY);
        #endregion

        #region enablesimulator_received
        [TranslatedScriptEvent("enablesimulator_received")]
        public class EnableSimulatorReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public string SimIP;
            [TranslatedScriptEventParameter(2)]
            public int SimPort;
            [TranslatedScriptEventParameter(3)]
            public int RegionSizeX;
            [TranslatedScriptEventParameter(4)]
            public int RegionSizeY;

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (EnableSimulator)m;
                vc.PostEvent(new EnableSimulatorReceivedEvent
                {
                    Agent = agent,
                    SimIP = res.SimIP.ToString(),
                    SimPort = res.SimPort,
                    RegionSizeX = (int)res.RegionSize.X,
                    RegionSizeY = (int)res.RegionSize.Y
                });
            }
        }

        [APIExtension(ExtensionName, "enablesimulator_received")]
        [StateEventDelegate]
        public delegate void EnableSimulatorReceived(
            ViewerAgentAccessor agent,
            string simIP,
            int simPort,
            int regionSizeX,
            int regionSizeY);
        #endregion

        #region removeinventoryfolder_received
        [TranslatedScriptEvent("removeinventoryfolder_received")]
        public class RemoveInventoryFolderReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public AnArray FolderList = new AnArray();

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var res = (RemoveInventoryFolder)m;
                var ev = new RemoveInventoryFolderReceivedEvent
                {
                    Agent = agent
                };
                foreach(UUID id in res.FolderData)
                {
                    ev.FolderList.Add(new LSLKey(id));
                }
                vc.PostEvent(ev);
            }
        }

        [APIExtension(ExtensionName, "removeinventoryfolder_received")]
        [StateEventDelegate]
        public delegate void RemoveInventoryFolderReceived(
            ViewerAgentAccessor agent,
            AnArray folderList);
        #endregion

        #region disablesimulator_received
        [TranslatedScriptEvent("disablesimulator_received")]
        public class DisableSimulatorReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                vc.PostEvent(new DisableSimulatorReceivedEvent
                {
                    Agent = agent
                });
            }
        }

        [APIExtension(ExtensionName, "disablesimulator_received")]
        [StateEventDelegate]
        public delegate void DisableSimulatorReceived(
            ViewerAgentAccessor agent);
        #endregion

        #region viewerfrozenmessage_received
        [TranslatedScriptEvent("viewerfrozenmessage_received")]
        public class ViewerFrozenMessageReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public int Frozen;

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var msg = (ViewerFrozenMessage)m;
                vc.PostEvent(new ViewerFrozenMessageReceivedEvent
                {
                    Agent = agent,
                    Frozen = msg.Frozen.ToLSLBoolean()
                });
            }
        }

        [APIExtension(ExtensionName, "viewerfrozenmessage_received")]
        [StateEventDelegate]
        public delegate void ViewerFrozenMessageReceived(ViewerAgentAccessor agent, int frozen);
        #endregion

        #region changeuserrights_received
        [TranslatedScriptEvent("changeuserrights_received")]
        public class ChangeUserRightsReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public AnArray StridedRightsList = new AnArray();

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var msg = (ChangeUserRights)m;
                var ev = new ChangeUserRightsReceivedEvent
                {
                    Agent = agent
                };

                foreach(ChangeUserRights.RightsEntry d in msg.Rights)
                {
                    ev.StridedRightsList.Add(new LSLKey(d.AgentRelated));
                    ev.StridedRightsList.Add((int)d.RelatedRights);
                }
                vc.PostEvent(ev);
            }
        }

        [APIExtension(ExtensionName, "changeuserrights_received")]
        [StateEventDelegate]
        public delegate void ChangeUserRightsReceived(ViewerAgentAccessor agent, AnArray stridedRightsList);
        #endregion

        #region parcelwellreply_received
        [TranslatedScriptEvent("parceldwellreply_received")]
        public class ParcelDwellReplyReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey ParcelID;
            [TranslatedScriptEventParameter(2)]
            public double Dwell;

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var msg = (ParcelDwellReply)m;
                vc.PostEvent(new ParcelDwellReplyReceivedEvent
                {
                    Agent = agent,
                    ParcelID = new LSLKey(new UUID(msg.ParcelID.GetBytes(), 0)),
                    Dwell = msg.Dwell
                });
            }
        }

        [APIExtension(ExtensionName, "parceldwellreply_received")]
        [StateEventDelegate]
        public delegate void ParcelDwellReplyReceived(ViewerAgentAccessor agent, LSLKey parcelID, double dwell);
        #endregion

        #region parcelmediacommandmessage_received
        [TranslatedScriptEvent("parcelmediacommandmessage_received")]
        public class ParcelMediaCommandMessageReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public int Flags;
            [TranslatedScriptEventParameter(2)]
            public int Command;
            [TranslatedScriptEventParameter(3)]
            public double Time;

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var msg = (ParcelMediaCommandMessage)m;
                vc.PostEvent(new ParcelMediaCommandMessageReceivedEvent
                {
                    Agent = agent,
                    Flags = (int)msg.Flags,
                    Command = (int)msg.Command,
                    Time = msg.Time
                });
            }
        }

        [APIExtension(ExtensionName, "parcelmediacommandmessage_received")]
        [StateEventDelegate]
        public delegate void ParcelMediaCommandMessageReceived(
            ViewerAgentAccessor agent,
            int flags,
            int command,
            double time);
        #endregion

        #region parcelmediaupdate_received
        [TranslatedScriptEvent("parcelmediaupdate_received")]
        public class ParcelMediaUpdateReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey MediaID;
            [TranslatedScriptEventParameter(2)]
            public int MediaAutoscale;
            [TranslatedScriptEventParameter(3)]
            public string MediaType;
            [TranslatedScriptEventParameter(4)]
            public string MediaDesc;
            [TranslatedScriptEventParameter(5)]
            public int MediaWidth;
            [TranslatedScriptEventParameter(6)]
            public int MediaHeight;
            [TranslatedScriptEventParameter(7)]
            public int MediaLoop;

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var msg = (ParcelMediaUpdate)m;
                vc.PostEvent(new ParcelMediaUpdateReceivedEvent
                {
                    Agent = agent,
                    MediaID = msg.MediaID,
                    MediaAutoscale = msg.MediaAutoScale.ToLSLBoolean(),
                    MediaType = msg.MediaType,
                    MediaDesc = msg.MediaDesc,
                    MediaWidth = msg.MediaWidth,
                    MediaHeight = msg.MediaHeight,
                    MediaLoop = msg.MediaLoop.ToLSLBoolean()
                });
            }
        }

        [APIExtension(ExtensionName, "parcelmediaupdate_received")]
        [StateEventDelegate]
        public delegate void ParcelMediaUpdateReceived(
            ViewerAgentAccessor agent,
            LSLKey mediaID,
            int mediaAutoscale,
            string mediaType,
            string mediaDesc,
            int mediaWidth,
            int mediaHeight,
            int mediaLoop);
        #endregion

        #region simstats_received
        [TranslatedScriptEvent("simstats_received")]
        public class SimStatsReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public int RegionX;
            [TranslatedScriptEventParameter(2)]
            public int RegionY;
            [TranslatedScriptEventParameter(3)]
            public int RegionFlags;
            [TranslatedScriptEventParameter(4)]
            public HashtableApi.Hashtable Stat = new HashtableApi.Hashtable();
            [TranslatedScriptEventParameter(5)]
            public int PID;
            [TranslatedScriptEventParameter(6)]
            public AnArray RegionFlagsExtended = new AnArray();

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var msg = (SimStats)m;
                var ev = new SimStatsReceivedEvent
                {
                    Agent = agent,
                    RegionX = (int)msg.RegionX,
                    RegionY = (int)msg.RegionY,
                    RegionFlags = (int)msg.RegionFlags,
                    PID = msg.PID
                };
                foreach(SimStats.Data d in msg.Stat)
                {
                    ev.Stat.Add(d.StatID.ToString(), new Real(d.StatValue));
                }

                foreach(ulong d in msg.RegionFlagsExtended)
                {
                    ev.RegionFlagsExtended.Add(new LongInteger((long)d));
                }
                vc.PostEvent(ev);
            }
        }

        [APIExtension(ExtensionName, "simstats_received")]
        [StateEventDelegate]
        public delegate void SimStatsReceived(ViewerAgentAccessor agent, int regionX, int regionY, int regionFlags, HashtableApi.Hashtable stat, int pid, AnArray regionFlagsExtended);
        #endregion

        #region objectpropertiesfamily_received
        [APIExtension(ExtensionName, "objectpropertiesfamilydata")]
        [APIDisplayName("objectpropertiesfamilydata")]
        [APIAccessibleMembers]
        [APIIsVariableType]
        public sealed class VcObjectPropertiesFamilyData
        {
            public string Name = string.Empty;
            public string Description = string.Empty;
            public LSLKey OwnerID = new LSLKey();
            public LSLKey GroupID = new LSLKey();
            public int BaseMask;
            public int OwnerMask;
            public int GroupMask;
            public int EveryoneMask;
            public int NextOwnerMask;
            public int OwnershipCost;
            public int SaleType;
            public int SalePrice;
            public int Category;
            public LSLKey LastOwnerID = new LSLKey();
        }

        [TranslatedScriptEvent("objectpropertiesfamily_received")]
        public class ObjectPropertiesFamilyReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public int RequestFlags;
            [TranslatedScriptEventParameter(2)]
            public LSLKey ObjectID;
            [TranslatedScriptEventParameter(3)]
            public VcObjectPropertiesFamilyData Data;

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var msg = (ObjectPropertiesFamily)m;
                vc.PostEvent(new ObjectPropertiesFamilyReceivedEvent
                {
                    Agent = agent,
                    RequestFlags = (int)msg.RequestFlags,
                    ObjectID = msg.ObjectID,
                    Data = new VcObjectPropertiesFamilyData
                    {
                        OwnerID = msg.OwnerID,
                        GroupID = msg.GroupID,
                        BaseMask = (int)msg.BaseMask,
                        OwnerMask = (int)msg.OwnerMask,
                        GroupMask = (int)msg.GroupMask,
                        EveryoneMask = (int)msg.EveryoneMask,
                        NextOwnerMask = (int)msg.NextOwnerMask,
                        OwnershipCost = msg.OwnershipCost,
                        SaleType = (int)msg.SaleType,
                        SalePrice = msg.SalePrice,
                        Category = (int)msg.Category,
                        LastOwnerID = msg.LastOwnerID,
                        Name = msg.Name,
                        Description = msg.Description
                    }
                });
            }
        }

        [APIExtension(ExtensionName, "objectpropertiesfamily_received")]
        [StateEventDelegate]
        public delegate void ObjectPropertiesFamilyReceived(
            ViewerAgentAccessor agent,
            int requestFlags,
            LSLKey objectID,
            VcObjectPropertiesFamilyData data);
        #endregion

        #region objectpropertiesfamily_received
        [TranslatedScriptEvent("objectproperties_received")]
        public class ObjectPropertiesReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public VcObjectPropertiesDataList ObjectProperties = new VcObjectPropertiesDataList();

            public static void ToScriptEvent(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
            {
                var msg = (ObjectProperties)m;
                var ev = new ObjectPropertiesReceivedEvent
                {
                    Agent = agent,
                };

                foreach(byte[] d in msg.ObjectData)
                {
                    ev.ObjectProperties.Add(new VcObjectPropertiesData(d));
                }
                vc.PostEvent(ev);
            }
        }

        [APIExtension(ExtensionName, "objectproperties_received")]
        [StateEventDelegate]
        public delegate void ObjectPropertiesReceived(
            ViewerAgentAccessor agent,
            VcObjectPropertiesDataList propertieslist);
        #endregion

        [TranslatedScriptEventsInfo]
        public static readonly Type[] TranslatedEvents = new Type[] {
            typeof(AgentDataUpdateReceivedEvent),
            typeof(AgentMovementCompleteReceivedEvent),
            typeof(AlertMessageReceivedEvent),
            typeof(AttachedSoundGainChangeReceivedEvent),
            typeof(AttachedSoundReceivedEvent),
            typeof(AvatarAnimationReceivedEvent),
            typeof(AvatarSitResponseReceivedEvent),
            typeof(CameraConstraintReceivedEvent),
            typeof(ChangeUserRightsReceivedEvent),
            typeof(ChatFromSimulatorReceivedEvent),
            typeof(ClearFollowCamPropertiesReceivedEvent),
            typeof(CrossedRegionReceivedEvent),
            typeof(DeRezAckReceivedEvent),
            typeof(DisableSimulatorReceivedEvent),
            typeof(EconomyDataReceivedEvent),
            typeof(EnableSimulatorReceivedEvent),
            typeof(EstablishAgentCommunicationReceivedEvent),
            typeof(EstateCovenantReplyReceivedEvent),
            typeof(EstateOwnerMessageEstateUpdateInfoReceivedEvent),
            typeof(EstateOwnerMessageSetExperienceReceivedEvent),
            typeof(EventQueueGetFinishedEvent),
            typeof(FeatureDisabledReceivedEvent),
            typeof(ForceObjectSelectReceivedEvent),
            typeof(GrantGodlikePowersReceivedEvent),
            typeof(HealthMessageReceivedEvent),
            typeof(ImprovedInstantMessageReceivedEvent),
            typeof(KillObjectReceivedEvent),
            typeof(LoadURLReceivedEvent),
            typeof(LogoutReplyReceivedEvent),
            typeof(MoneyBalanceReplyReceivedEvent),
            typeof(ObjectAnimationReceivedEvent),
            typeof(ObjectPhysicsPropertiesReceivedEvent),
            typeof(ObjectPropertiesFamilyReceivedEvent),
            typeof(ObjectPropertiesReceivedEvent),
            typeof(ObjectUpdateReceivedEvent),
            typeof(OfflineNotificationReceivedEvent),
            typeof(OnlineNotificationReceivedEvent),
            typeof(ParcelDwellReplyReceivedEvent),
            typeof(ParcelInfoReplyReceivedEvent),
            typeof(ParcelMediaCommandMessageReceivedEvent),
            typeof(ParcelMediaUpdateReceivedEvent),
            typeof(ParcelObjectOwnersReplyReceivedEvent),
            typeof(ParcelPropertiesReceivedEvent),
            typeof(ParcelVoiceInfoReceivedEvent),
            typeof(PayPriceReplyReceivedEvent),
            typeof(PreloadSoundReceivedEvent),
            typeof(RegionHandshakeReceivedEvent),
            typeof(RegionInfoReceivedEvent),
            typeof(RemoveInventoryFolderReceivedEvent),
            typeof(ScriptControlChangeReceivedEvent),
            typeof(ScriptDialogReceivedEvent),
            typeof(ScriptQuestionReceivedEvent),
            typeof(ScriptRunningReplyReceivedEvent),
            typeof(ScriptTeleportRequestReceivedEvent),
            typeof(SetFollowCamPropertiesReceivedEvent),
            typeof(SimStatsReceivedEvent),
            typeof(SimulatorViewerTimeMessageReceived),
            typeof(SoundTriggerReceivedEvent),
            typeof(TelehubInfoReceivedEvent),
            typeof(TeleportLocalReceivedEvent),
            typeof(TeleportProgressReceivedEvent),
            typeof(TeleportStartReceivedEvent),
            typeof(TeleportFailedReceivedEvent),
            typeof(UUIDGroupNameReplyReceivedEvent),
            typeof(UUIDNameReplyReceivedEvent),
            typeof(ViewerFrozenMessageReceivedEvent)
        };
    }
}

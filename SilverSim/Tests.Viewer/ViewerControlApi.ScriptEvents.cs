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
using SilverSim.Types;
using System;
using System.ComponentModel;

namespace SilverSim.Tests.Viewer
{
    public partial class ViewerControlApi
    {
        #region regionhandshake_received event
        [TranslatedScriptEvent("regionhandshake_received")]
        public class RegionHandshakeReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public LSLKey AgentID { get; }

            [TranslatedScriptEventParameter(1)]
            public LSLKey RegionID { get; }

            public RegionHandshakeReceivedEvent(LSLKey agentID, LSLKey regionID)
            {
                AgentID = agentID;
                RegionID = regionID;
            }
        }

        [APIExtension("ViewerControl", "regionhandshake_received")]
        [StateEventDelegate]
        public delegate void RegionHandshakeReceived(
            [Description("Agent id")]
            LSLKey agentId,
            [Description("Region id")]
            LSLKey regionId);
        #endregion

        #region logoutreply_received
        [TranslatedScriptEvent("logoutreply_received")]
        public class LogoutReplyReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public LSLKey AgentID { get; }

            [TranslatedScriptEventParameter(1)]
            public LSLKey RegionID { get; }

            [TranslatedScriptEventParameter(2)]
            public int CircuitCode { get; }

            public LogoutReplyReceivedEvent(LSLKey agentID, LSLKey regionID, int circuitcode)
            {
                AgentID = agentID;
                RegionID = regionID;
                CircuitCode = circuitcode;
            }
        }

        [APIExtension("ViewerControl", "logoutreply_received")]
        [StateEventDelegate]
        public delegate void LogoutReplyReceived(
            [Description("Agent id")]
            LSLKey agentId,
            [Description("Region id")]
            LSLKey regionId,
            [Description("Circuit code")]
            int circuitCode);
        #endregion

        #region telehubinfo_received
        [TranslatedScriptEvent("telehubinfo_received")]
        public class TelehubInfoReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public LSLKey AgentID;
            [TranslatedScriptEventParameter(1)]
            public LSLKey RegionID;
            [TranslatedScriptEventParameter(2)]
            public LSLKey ObjectID;
            [TranslatedScriptEventParameter(3)]
            public string ObjectName;
            [TranslatedScriptEventParameter(4)]
            public Vector3 TelehubPos;
            [TranslatedScriptEventParameter(5)]
            public Quaternion TelehubRot;
            [TranslatedScriptEventParameter(6)]
            public AnArray SpawnPointPos = new AnArray();
        }

        [APIExtension("ViewerControl", "telehubinfo_received")]
        [StateEventDelegate]
        public delegate void TelehubInfoReceived(
            LSLKey agentId,
            LSLKey regionId,
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
            public LSLKey AgentID;
            [TranslatedScriptEventParameter(1)]
            public LSLKey RegionID;
            [TranslatedScriptEventParameter(2)]
            public Vector3 Position;
            [TranslatedScriptEventParameter(3)]
            public Vector3 LookAt;
            [TranslatedScriptEventParameter(4)]
            public int TeleportFlags;
        }

        [APIExtension("ViewerControl", "teleportlocal_received")]
        [StateEventDelegate]
        public delegate void TeleportLocalReceived(
            LSLKey agentId,
            LSLKey regionId,
            Vector3 position,
            Vector3 lookAt,
            int teleportFlags);
        #endregion

        #region teleportprogress_received
        [TranslatedScriptEvent("teleportprogress_received")]
        public class TeleportProgressReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public LSLKey AgentID;
            [TranslatedScriptEventParameter(1)]
            public LSLKey RegionID;
            [TranslatedScriptEventParameter(2)]
            public int TeleportFlags;
            [TranslatedScriptEventParameter(3)]
            public string Message;
        }

        [APIExtension("ViewerControl", "teleportprogress_received")]
        [StateEventDelegate]
        public delegate void TeleportProgressReceived(
            LSLKey agentId,
            LSLKey regionId,
            int teleportFlags,
            string message);
        #endregion

        #region teleportstart_received
        [TranslatedScriptEvent("teleportstart_received")]
        public class TeleportStartReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public LSLKey AgentID;
            [TranslatedScriptEventParameter(1)]
            public LSLKey RegionID;
            [TranslatedScriptEventParameter(2)]
            public int TeleportFlags;
        }

        [APIExtension("ViewerControl", "teleportstart_received")]
        [StateEventDelegate]
        public delegate void TeleportStartReceived(
            LSLKey agentId,
            LSLKey regionId,
            int teleportFlags);
        #endregion

        #region teleportfailed_received
        [TranslatedScriptEvent("teleportfailed_received")]
        public class TeleportFailedReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public LSLKey AgentID;
            [TranslatedScriptEventParameter(1)]
            public LSLKey RegionID;
            [TranslatedScriptEventParameter(2)]
            public LSLKey Reason;
            [TranslatedScriptEventParameter(3)]
            public AnArray AlertInfo = new AnArray();
        }

        [APIExtension("ViewerControl", "teleportfailed_received")]
        [StateEventDelegate]
        public delegate void TeleportFailedReceived(
            LSLKey agentId,
            LSLKey regionId,
            string reason,
            AnArray alertInfo);
        #endregion

        #region economydata_received
        [TranslatedScriptEvent("economydata_received")]
        public class EconomyDataReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public LSLKey AgentID;
            [TranslatedScriptEventParameter(1)]
            public LSLKey RegionID;
        }

        [APIExtension("ViewerControl", "economydata_received")]
        [StateEventDelegate]
        public delegate void EconomyDataReceived(
            LSLKey agentId,
            LSLKey regionId);
        #endregion

        #region alertmessage_received
        [TranslatedScriptEvent("alertmessage_received")]
        public class AlertMessageReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public string Message;
            [TranslatedScriptEventParameter(1)]
            public AnArray AlertInfo = new AnArray();
        }

        [APIExtension("ViewerControl", "alertmessage_received")]
        [StateEventDelegate]
        public delegate void AlertMessageReceived(
            string message,
            AnArray alertInfo);
        #endregion

        #region agentdataupdate_received
        [TranslatedScriptEvent("agentdataupdate_received")]
        public class AgentDataUpdateReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public LSLKey AgentID = new LSLKey();
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
        }

        [APIExtension("ViewerControl", "agentdataupdate_received")]
        [StateEventDelegate]
        public delegate void AgentDataUpdateReceived(
            LSLKey agentID,
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
            public LSLKey AgentID = new LSLKey();
            [TranslatedScriptEventParameter(1)]
            public LSLKey GroupID = new LSLKey();
        }

        [APIExtension("ViewerControl", "agentdropgroup_received")]
        [StateEventDelegate]
        public delegate void AgentDropGroupReceived(
            LSLKey agentID,
            LSLKey groupID);
        #endregion

        #region coarselocationupdate_received
        [TranslatedScriptEvent("coarselocationupdate_received")]
        public class CoarseLocationUpdateReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public int You;
            [TranslatedScriptEventParameter(1)]
            public int Prey;
            [TranslatedScriptEventParameter(2)]
            public AnArray AgentData = new AnArray();
        }

        [APIExtension("ViewerControl", "coarselocationupdate_received")]
        public delegate void CoarseLocationUpdateReceived(
            int you,
            int prey,
            AnArray agentData);
        #endregion

        #region healthmessage_received
        [TranslatedScriptEvent("healthmessage_received")]
        public class HealthMessageReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public double Health;
        }

        [APIExtension("ViewerControl", "healthmessage_received")]
        public delegate void HealthMessageUpdateReceived(
            double health);
        #endregion

        #region avataranimation_received
        [TranslatedScriptEvent("avataranimation_received")]
        public class AvatarAnimationReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public LSLKey Sender = new LSLKey();
            [TranslatedScriptEventParameter(1)]
            public AnArray AnimationData = new AnArray();
        }

        [APIExtension("ViewerControl", "avataranimation_received")]
        public delegate void AvatarAnimationReceived(
            LSLKey sender,
            AnArray animationData);
        #endregion

        #region avatarsitresponse_received
        [TranslatedScriptEvent("avatarsitresponse_received")]
        public class AvatarSitResponseReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public LSLKey SitObject = new LSLKey();
            [TranslatedScriptEventParameter(1)]
            public int IsAutopilot;
            [TranslatedScriptEventParameter(2)]
            public Vector3 SitPosition;
            [TranslatedScriptEventParameter(3)]
            public Quaternion SitRotation;
            [TranslatedScriptEventParameter(4)]
            public Vector3 CameraEyeOffset;
            [TranslatedScriptEventParameter(5)]
            public Vector3 CameraAtOffset;
            [TranslatedScriptEventParameter(6)]
            public int ForceMouselook;
        }

        [APIExtension("ViewerControl", "avatarsitresponse_received")]
        public delegate void AvatarSitResponseReceived(
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
            public Quaternion CameraCollidePlane;
        }

        [APIExtension("ViewerControl", "cameraconstraint_received")]
        public delegate void CameraConstraintReceived(
            Quaternion cameraCollidePlane);
        #endregion

        #region clearfollowcamproperties_received
        [TranslatedScriptEvent("clearfollowcamproperties_received")]
        public class ClearFollowCamPropertiesReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public LSLKey ObjectID = new LSLKey();
        }

        [APIExtension("ViewerControl", "clearfollowcamproperties_received")]
        public delegate void ClearFollowCamPropertiesReceived(
            LSLKey objectID);
        #endregion

        #region setfollowcamproperties_received
        [TranslatedScriptEvent("setfollowcamproperties_received")]
        public class SetFollowCamPropertiesReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public LSLKey ObjectID = new LSLKey();
            [TranslatedScriptEventParameter(1)]
            public AnArray CameraParams = new AnArray();
        }

        [APIExtension("ViewerControl", "setfollowcamproperties_received")]
        public delegate void SetFollowCamPropertiesReceived(
            LSLKey objectID,
            AnArray cameraParams);
        #endregion

        #region chatfromsimulator_received
        [TranslatedScriptEvent("chatfromsimulator_received")]
        public class ChatFromSimulatorReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public string FromName;
            [TranslatedScriptEventParameter(1)]
            public LSLKey SourceID;
            [TranslatedScriptEventParameter(2)]
            public LSLKey OwnerID;
            [TranslatedScriptEventParameter(3)]
            public int SourceType;
            [TranslatedScriptEventParameter(4)]
            public int ChatType;
            [TranslatedScriptEventParameter(5)]
            public int AudibleLevel;
            [TranslatedScriptEventParameter(6)]
            public Vector3 Position;
            [TranslatedScriptEventParameter(7)]
            public string Message;
        }

        [APIExtension("ViewerControl", "chatfromsimulator_received")]
        public delegate void ChatFromSimulatorReceived(
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
            public LSLKey CovenantID = new LSLKey();
            [TranslatedScriptEventParameter(1)]
            public long CovenantTimestamp;
            [TranslatedScriptEventParameter(2)]
            public string EstateName;
            [TranslatedScriptEventParameter(3)]
            public LSLKey EstateOwnerID = new LSLKey();
        }

        [APIExtension("ViewerControl", "estatecovenantreply_received")]
        public delegate void EstateCovenantReplyReceived(
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
            public string ObjectName = string.Empty;
            [TranslatedScriptEventParameter(1)]
            public LSLKey ObjectID = new LSLKey();
            [TranslatedScriptEventParameter(2)]
            public LSLKey OwnerID = new LSLKey();
            [TranslatedScriptEventParameter(3)]
            public int OwnerIsGroup;
            [TranslatedScriptEventParameter(4)]
            public string Message = string.Empty;
            [TranslatedScriptEventParameter(5)]
            public string URL = string.Empty;
        }

        [APIExtension("ViewerControl", "loadurl_received")]
        public delegate void LoadURLReceived(
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
            public string ObjectName = string.Empty;
            [TranslatedScriptEventParameter(1)]
            public string SimName = string.Empty;
            [TranslatedScriptEventParameter(2)]
            public Vector3 SimPosition;
            [TranslatedScriptEventParameter(3)]
            public Vector3 LookAt;
        }

        [APIExtension("ViewerControl", "scriptteleportrequest_received")]
        public delegate void ScriptTeleportRequestReceived(
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
            public LSLKey TaskID = new LSLKey();
            [TranslatedScriptEventParameter(1)]
            public LSLKey ItemID = new LSLKey();
            [TranslatedScriptEventParameter(2)]
            public string ObjectName;
            [TranslatedScriptEventParameter(3)]
            public string ObjectOwner;
            [TranslatedScriptEventParameter(4)]
            public int Questions;
            [TranslatedScriptEventParameter(5)]
            public LSLKey ExperienceID = new LSLKey();
        }

        [APIExtension("ViewerControl", "scriptquestion_received")]
        public delegate void ScriptQuestionReceived(
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
            public LSLKey ObjectID = new LSLKey();
            [TranslatedScriptEventParameter(1)]
            public string FirstName = string.Empty;
            [TranslatedScriptEventParameter(2)]
            public string LastName = string.Empty;
            [TranslatedScriptEventParameter(3)]
            public string ObjectName = string.Empty;
            [TranslatedScriptEventParameter(4)]
            public string Message = string.Empty;
            [TranslatedScriptEventParameter(5)]
            public int ChatChannel;
            [TranslatedScriptEventParameter(6)]
            public LSLKey ImageID = new LSLKey();
            [TranslatedScriptEventParameter(7)]
            public AnArray ButtonData = new AnArray();
            [TranslatedScriptEventParameter(8)]
            public AnArray OwnerData = new AnArray();
        }

        [APIExtension("ViewerControl", "scriptdialog_received")]
        public delegate void ScriptDialogReceived(
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
            public AnArray ControlData = new AnArray();
        }

        [APIExtension("ViewerControl", "scriptcontrolchange_received")]
        public delegate void ScriptControlChangeReceived(AnArray controlData);
        #endregion

        #region preloadsound_received
        [TranslatedScriptEvent("preloadsound_received")]
        public class PreloadSoundReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public LSLKey ObjectID = new LSLKey();
            [TranslatedScriptEventParameter(1)]
            public LSLKey OwnerID = new LSLKey();
            [TranslatedScriptEventParameter(2)]
            public LSLKey SoundID = new LSLKey();
        }

        [APIExtension("ViewerControl", "preloadsound_received")]
        public delegate void PreloadsoundReceived(
            LSLKey objectId,
            LSLKey ownerID,
            LSLKey soundID);
        #endregion

        #region attachedsound_received
        [TranslatedScriptEvent("attachedsound_received")]
        public class AttachedSoundReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public LSLKey SoundID = new LSLKey();
            [TranslatedScriptEventParameter(1)]
            public LSLKey ObjectID = new LSLKey();
            [TranslatedScriptEventParameter(2)]
            public LSLKey OwnerID = new LSLKey();
            [TranslatedScriptEventParameter(3)]
            public double Gain;
            [TranslatedScriptEventParameter(4)]
            public int Flags;
        }

        [APIExtension("ViewerControl", "attachedsound_received")]
        public delegate void AttachedSoundReceived(
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
            public LSLKey SoundID = new LSLKey();
            [TranslatedScriptEventParameter(1)]
            public LSLKey OwnerID = new LSLKey();
            [TranslatedScriptEventParameter(2)]
            public LSLKey ObjectID = new LSLKey();
            [TranslatedScriptEventParameter(3)]
            public LSLKey ParentID = new LSLKey();
            [TranslatedScriptEventParameter(4)]
            public int GridX;
            [TranslatedScriptEventParameter(5)]
            public int GridY;
            [TranslatedScriptEventParameter(6)]
            public Vector3 Position;
            [TranslatedScriptEventParameter(7)]
            public double Gain;
        }

        [APIExtension("ViewerControl", "soundtrigger_received")]
        public delegate void SoundTriggerReceived(
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
            public LSLKey ObjectID = new LSLKey();
            [TranslatedScriptEventParameter(1)]
            public double Gain;
        }

        [APIExtension("ViewerControl", "attachedsoundgainchange_received")]
        public delegate void AttachedSoundGainChangeReceived(
            LSLKey objectID,
            double gain);
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
            typeof(AttachedSoundGainChangeReceivedEvent)
        };
    }
}

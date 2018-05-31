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
            typeof(AgentDataUpdateReceivedEvent)
        };
    }
}

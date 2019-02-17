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

using SilverSim.Http.Client;
using SilverSim.Scene.Types.Script;
using SilverSim.Scripting.Lsl;
using SilverSim.Scripting.Lsl.Api.ByteString;
using SilverSim.Scripting.Lsl.Api.Hashtable;
using SilverSim.Types;
using SilverSim.Types.StructuredData.Llsd;
using System.IO;

namespace SilverSim.Tests.Viewer
{
    public partial class ViewerControlApi
    {
        [APIExtension(ExtensionName, "capsUpdateAgentLanguage")]
        public int CapsUpdateAgentLanguage(ScriptInstance instance, string uri, string langname, int ispublic)
        {
            lock (instance)
            {
                return (int)new HttpClient.Post(uri, "application/llsd+xml", (Stream sout) =>
                {
                    LlsdXml.Serialize(new Map
                    {
                        { "language", langname },
                        { "language_is_public", ispublic != 0 }
                    }, sout);
                })
                {
                    TimeoutMs = 20000
                }.ExecuteStatusRequest();
            }
        }

        [APIExtension(ExtensionName, "capsCopyInventoryFromNotecard")]
        public int CapsCopyInventoryFromNotecard(ScriptInstance instance, string uri, LSLKey destinationFolderID, LSLKey notecardId, LSLKey notecardInventoryId, int callbackId) =>
            CapsCopyInventoryFromNotecard(instance, uri, destinationFolderID, UUID.Zero, notecardId, notecardInventoryId, callbackId);

        [APIExtension(ExtensionName, "capsCopyInventoryFromNotecard")]
        public int CapsCopyInventoryFromNotecard(ScriptInstance instance, string uri, LSLKey destinationFolderID, LSLKey objectId, LSLKey notecardId, LSLKey notecardInventoryId, int callbackId)
        {
            lock (instance)
            {
                return (int)new HttpClient.Post(uri, "application/llsd+xml", (Stream sout) =>
                {
                    LlsdXml.Serialize(new Map
                    {
                        { "object-id", objectId.AsUUID },
                        { "notecard-id", notecardId.AsUUID },
                        { "item-id", notecardInventoryId.AsUUID },
                        { "folder-id", destinationFolderID.AsUUID },
                        { "callback-id", callbackId }
                    }, sout);
                })
                {
                    TimeoutMs = 20000
                }.ExecuteStatusRequest();
            }
        }

        [APIExtension(ExtensionName, "dispatchregioninfo")]
        [APIDisplayName("dispatchregioninfo")]
        [APIAccessibleMembers]
        [APIIsVariableType]
        public sealed class DispatchRegionInfo
        {
            public int BlockTerraform;
            public int BlockFly;
            public int BlockFlyOver;
            public int AllowDamage;
            public int AllowLandResell;
            public int AgentLimit;
            public double PrimBonusFactor;
            public int SimAccess;
            public int RestrictPushing;
            public int AllowLandJoinDivide;
            public int BlockShowInSearch;
        }

        [APIExtension(ExtensionName, "capsDispatchRegionInfo")]
        public int CapsDispatchRegionInfo(ScriptInstance instance, string uri, DispatchRegionInfo regionInfo)
        {
            lock (instance)
            {
                return (int)new HttpClient.Post(uri,
                    new HttpClient.LlsdXmlRequest(new Map
                    {
                        { "block_terraform", regionInfo.BlockTerraform != 0 },
                        { "block_fly", regionInfo.BlockFly != 0 },
                        { "block_fly_over", regionInfo.BlockFlyOver != 0 },
                        { "allow_damage", regionInfo.AllowDamage != 0 },
                        { "allow_land_resell", regionInfo.AllowLandResell != 0 },
                        { "agent_limit", regionInfo.AgentLimit },
                        { "prim_bonus", regionInfo.PrimBonusFactor },
                        { "sim_access", regionInfo.SimAccess },
                        { "restrict_pushobject", regionInfo.RestrictPushing != 0 },
                        { "allow_parcel_changes", regionInfo.AllowLandJoinDivide != 0 },
                        { "block_parcel_search", regionInfo.BlockShowInSearch != 0 }
                    }))
                {
                    TimeoutMs = 20000
                }.ExecuteStatusRequest();
            }
        }

        [APIExtension(ExtensionName, "estatechangeinfo")]
        [APIDisplayName("estatechangeinfo")]
        [APIAccessibleMembers]
        [APIIsVariableType]
        public sealed class EstateChangeInfo
        {
            public string Name;
            public double SunHour;
            public int IsSunFixed;
            public int IsExternallyVisible;
            public int AllowDirectTeleport;
            public int DenyAnonymous;
            public int DenyAgeUnverified;
            public int AllowVoiceChat;
            public LSLKey InvoiceID = UUID.Random;
        }

        [APIExtension(ExtensionName, "capsEstateChangeInfo")]
        public int CapsEstateChangeInfo(ScriptInstance instance, string uri, EstateChangeInfo estateInfo)
        {
            lock (instance)
            {
                return (int)new HttpClient.Post(uri,
                    new HttpClient.LlsdXmlRequest(new Map
                    {
                        { "estate_name", estateInfo.Name },
                        { "sun_hour", estateInfo.SunHour },
                        { "is_sun_fixed", estateInfo.IsSunFixed != 0 },
                        { "is_externally_visible", estateInfo.IsExternallyVisible != 0 },
                        { "allow_direct_teleport", estateInfo.AllowDirectTeleport != 0 },
                        { "deny_anonymous", estateInfo.DenyAnonymous != 0 },
                        { "deny_age_unverified", estateInfo.DenyAgeUnverified != 0 },
                        { "allow_voice_chat", estateInfo.AllowVoiceChat != 0 },
                        { "invoice", estateInfo.InvoiceID.AsUUID }
                    }))
                {
                    TimeoutMs = 20000
                }.ExecuteStatusRequest();
            }
        }

        [APIExtension(ExtensionName, "capsParcelNavigateMedia")]
        public int CapsParcelNavigateMedia(ScriptInstance instance, ViewerAgentAccessor agentinfo, string uri, int parcellocalid, string mediaurl)
        {
            lock (instance)
            {
                return (int)new HttpClient.Post(uri,
                    new HttpClient.LlsdXmlRequest(new Map
                    {
                        { "agent-id", agentinfo.AgentID },
                        { "local-id", parcellocalid },
                        { "url", mediaurl }
                    }))
                {
                    TimeoutMs = 20000
                }.ExecuteStatusRequest();
            }
        }

        [APIExtension(ExtensionName, "capsObjectMediaNavigate")]
        public int CapsObjectMediaNavigate(ScriptInstance instance, string uri, LSLKey objectID, int textureIndex, string mediaurl)
        {
            lock (instance)
            {
                return (int)new HttpClient.Post(uri,
                    new HttpClient.LlsdXmlRequest(new Map
                    {
                        { "object_id", objectID.AsUUID },
                        { "current_url", mediaurl },
                        { "texture_index", textureIndex }
                    }))
                {
                    TimeoutMs = 20000
                }.ExecuteStatusRequest();
            }
        }

        [APIExtension(ExtensionName, "capsUpdateAgentInventory")]
        public HashtableApi.Hashtable CapsUpdateAgentInventory(ScriptInstance instance, string uri, LSLKey itemid, string contentType, ByteArrayApi.ByteArray assetData)
        {
            HashtableApi.Hashtable res = new HashtableApi.Hashtable();
            lock (instance)
            {
                Map result;
                using (Stream s = new HttpClient.Post(uri,
                    new HttpClient.LlsdXmlRequest(new Map
                    {
                        { "item_id", itemid.AsUUID },
                    }))
                {
                    TimeoutMs = 20000
                }.ExecuteStreamRequest())
                {
                    result = (Map)LlsdXml.Deserialize(s);
                }

                res["result"] = result["state"].ToString();
                if(result["state"].ToString() == "upload")
                {
                    string uploader_uri = res["uploader"].ToString();

                    using (Stream s = new HttpClient.Post(
                        uploader_uri,
                        contentType,
                        assetData.Length, (Stream sd) => sd.Write(assetData.Data, 0, assetData.Length))
                    {
                        TimeoutMs = 20000
                    }.ExecuteStreamRequest())
                    {
                        result = (Map)LlsdXml.Deserialize(s);
                    }
                    res["result"] = result["state"].ToString();
                    if(result["state"].ToString() == "complete")
                    {
                        res["assetid"] = new LSLKey(result["new_asset"].AsUUID);
                    }
                    if(result.ContainsKey("compiled"))
                    {
                        res["compiled"] = ((bool)result["compiled"].AsBoolean).ToLSLBoolean();
                    }
                    if(result.ContainsKey("errors"))
                    {
                        res.Add("errors", (AnArray)result["errors"]);
                    }
                }
            }

            return res;
        }

        [APIExtension(ExtensionName, "capsUpdateTaskInventory")]
        public HashtableApi.Hashtable CapsUpdateTaskInventory(ScriptInstance instance, string uri, LSLKey objectid, LSLKey itemid, string contentType, ByteArrayApi.ByteArray assetData)
        {
            HashtableApi.Hashtable res = new HashtableApi.Hashtable();
            lock (instance)
            {
                Map result;
                using (Stream s = new HttpClient.Post(uri,
                    new HttpClient.LlsdXmlRequest(new Map
                    {
                        { "task_id", objectid.AsUUID },
                        { "item_id", itemid.AsUUID },
                    }))
                {
                    TimeoutMs = 20000
                }.ExecuteStreamRequest())
                {
                    result = (Map)LlsdXml.Deserialize(s);
                }

                res["result"] = result["state"].ToString();
                if (result["state"].ToString() == "upload")
                {
                    string uploader_uri = res["uploader"].ToString();

                    using (Stream s = new HttpClient.Post(
                        uploader_uri,
                        contentType,
                        assetData.Length, (Stream sd) => sd.Write(assetData.Data, 0, assetData.Length))
                    {
                        TimeoutMs = 20000
                    }.ExecuteStreamRequest())
                    {
                        result = (Map)LlsdXml.Deserialize(s);
                    }
                    res["result"] = result["state"].ToString();
                    if (result["state"].ToString() == "complete")
                    {
                        res["assetid"] = new LSLKey(result["new_asset"].AsUUID);
                    }
                    if (result.ContainsKey("compiled"))
                    {
                        res["compiled"] = ((bool)result["compiled"].AsBoolean).ToLSLBoolean();
                    }
                    if (result.ContainsKey("errors"))
                    {
                        res.Add("errors", (AnArray)result["errors"]);
                    }
                }
            }

            return res;
        }
    }
}

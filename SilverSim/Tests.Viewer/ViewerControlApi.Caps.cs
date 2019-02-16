﻿// SilverSim is distributed under the terms of the
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
    }
}

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
using SilverSim.Scripting.Lsl.Api.Hashtable;
using SilverSim.Types;
using SilverSim.Types.StructuredData.Llsd;
using System.Collections.Generic;
using System.IO;

namespace SilverSim.Tests.Viewer
{
    public partial class ViewerControlApi
    {
        [APIExtension(ExtensionName, "vcSeedRequest")]
        public HashtableApi.Hashtable SeedRequest(
            ScriptInstance instance,
            string seedCaps,
            AnArray elements)
        {
            lock (instance)
            {
                byte[] post;

                using (var ms = new MemoryStream())
                {
                    LlsdXml.Serialize(elements, ms);
                    post = ms.ToArray();
                }

                Map resdata;

                try
                {
                    using (Stream res = new HttpClient.Post(seedCaps, "application/llsd+xml", post.Length, (Stream req) => req.Write(post, 0, post.Length)).ExecuteStreamRequest())
                    {
                        resdata = LlsdXml.Deserialize(res) as Map;
                    }
                }
                catch
                {
                    return new HashtableApi.Hashtable();
                }

                if (null == resdata)
                {
                    return new HashtableApi.Hashtable();
                }

                var result = new HashtableApi.Hashtable();
                foreach (KeyValuePair<string, IValue> kvp in resdata)
                {
                    result.Add(kvp.Key, kvp.Value);
                }
                return result;
            }
        }
    }
}

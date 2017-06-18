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

using log4net;
using SilverSim.Main.Common;
using SilverSim.Scene.Types.Object;
using SilverSim.Tests.Extensions;
using SilverSim.Types;
using SilverSim.Types.Asset.Format;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SilverSim.Tests.Assets.Formats
{
    public class ObjectDeserialization : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Setup()
        {

        }

        public void Cleanup()
        {

        }

        public bool Run()
        {
            var manifests = new List<string>();
            foreach(string manifest in GetType().Assembly.GetManifestResourceNames())
            {
                if(manifest.StartsWith("SilverSim.Tests.Resources.Objects.") && manifest.EndsWith(".xml"))
                {
                    manifests.Add(manifest);
                }
            }

            foreach (string manifest in manifests)
            {
                m_Log.InfoFormat("Testing decoder with asset {0}", manifest);
                Stream resource = GetType().Assembly.GetManifestResourceStream(manifest);
                try
                {
                    ObjectXML.FromXml(resource, UUI.Unknown, XmlDeserializationOptions.ReadKeyframeMotion);
                }
                catch (Exception e)
                {
                    m_Log.InfoFormat("Failed to parse asset {0}: {1}\n{2}", e.GetType().FullName, e.StackTrace, e.StackTrace.ToString());
                    return false;
                }

                var reflist = new List<UUID>();
                resource = GetType().Assembly.GetManifestResourceStream(manifest);
                ObjectReferenceDecoder.GetReferences(resource, "", reflist);
                m_Log.InfoFormat("Found {0} references", reflist.Count);
            }

            return true;
        }

        public void Startup(ConfigurationLoader loader)
        {
        }
    }
}

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

using SilverSim.Main.Common;
using SilverSim.Scene.Types.Object;
using SilverSim.Scene.Types.Script;
using SilverSim.Scripting.Lsl;
using SilverSim.ServiceInterfaces.Asset;
using SilverSim.Types.Asset;
using SilverSim.Types.Asset.Format;
using SilverSim.Types.Asset.Format.Mesh;
using System.IO;
using CSJ2K;
using log4net;

namespace SilverSim.Tests.Lsl
{
    [LSLImplementation]
    [ScriptApiName("AssetTesting")]
    [PluginName("AssetTesting")]
    public class TestAssetsApi : IPlugin, IScriptApi
    {
        private static readonly ILog m_Log = LogManager.GetLogger("TEST ASSETS");

        public void Startup(ConfigurationLoader loader)
        {
            /* intentionally left empty */
        }

        [APIExtension("Testing", "_test_exportasset")]
        public void ExportAsset(ScriptInstance instance, LSLKey assetid, string basefilename)
        {
            lock(instance)
            {
                AssetServiceInterface assetService = instance.Part.ObjectGroup.AssetService;
                AssetData asset;
                if(assetService.TryGetValue(assetid.AsUUID, out asset))
                {
                    using (var fs = new FileStream(basefilename + asset.FileExtension, FileMode.Create, FileAccess.Write))
                    {
                        fs.Write(asset.Data, 0, asset.Data.Length);
                    }
                }
            }
        }

        [APIExtension("Testing", "_test_assetexists")]
        public int CheckAssetExists(ScriptInstance instance, LSLKey assetid)
        {
            lock(instance)
            {
                return instance.Part.ObjectGroup.AssetService.Exists(assetid.AsUUID) ? 1 : 0;
            }
        }

        [APIExtension("Testing", "_test_assetformat")]
        public int CheckAssetFormat(ScriptInstance instance, LSLKey assetid)
        {
            lock(instance)
            {
                AssetServiceInterface assetService = instance.Part.ObjectGroup.AssetService;
                AssetData asset;
                if (!assetService.TryGetValue(assetid.AsUUID, out asset))
                {
                    return 0;
                }

                switch(asset.Type)
                {
                    case AssetType.Bodypart:
                    case AssetType.Clothing:
                        try
                        {
                            new Wearable(asset);
                            return 1;
                        }
                        catch
                        {
                            return 0;
                        }

                    case AssetType.Gesture:
                        try
                        {
                            new Gesture(asset);
                            return 1;
                        }
                        catch
                        {
                            return 0;
                        }

                    case AssetType.Landmark:
                        try
                        {
                            new Landmark(asset);
                            return 1;
                        }
                        catch
                        {
                            return 0;
                        }

                    case AssetType.Mesh:
                        try
                        {
                            new LLMesh(asset);
                            return 1;
                        }
                        catch
                        {
                            return 0;
                        }

                    case AssetType.Notecard:
                        try
                        {
                            new Notecard(asset);
                            return 1;
                        }
                        catch
                        {
                            return 0;
                        }

                    case AssetType.Object:
                        try
                        {
                            ObjectXML.FromXml(asset.InputStream, instance.Part.Owner);
                            return 1;
                        }
                        catch
                        {
                            return 0;
                        }

                    case AssetType.Texture:
                        try
                        {
                            using (J2kImage.FromStream(asset.InputStream))
                            {

                            }
                            return 1;
                        }
                        catch
                        {
                            return 0;
                        }

                    default:
                        m_Log.WarnFormat("Testing asset type {0} not supported", asset.Type.ToString());
                        return 0;
                }
            }
        }
    }
}

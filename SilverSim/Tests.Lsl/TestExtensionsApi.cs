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
using Nini.Config;
using SilverSim.Main.Common;
using SilverSim.Scene.Types.Object;
using SilverSim.Scene.Types.Object.Parameters;
using SilverSim.Scene.Types.Scene;
using SilverSim.Scene.Types.Script;
using SilverSim.Scene.Types.Script.Events;
using SilverSim.Scripting.Common;
using SilverSim.Scripting.Lsl;
using SilverSim.ServiceInterfaces.Asset;
using SilverSim.Tests.Extensions;
using SilverSim.Tests.Scripting;
using SilverSim.Types;
using SilverSim.Types.Asset;
using SilverSim.Types.Parcel;
using SilverSim.Types.Primitive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SilverSim.Tests.Lsl
{
    [LSLImplementation]
    [ScriptApiName("TestExtensions")]
    [PluginName("Testing")]
    public class TestExtensionsApi : IScriptApi, IPlugin
    {
        private static readonly ILog m_Log = LogManager.GetLogger("SCRIPT");
        private ConfigurationLoader m_Loader;
        private TestRunner m_TestRunner;
        private RunScript m_ScriptRunner;
        private AssetServiceInterface m_AssetService;
        private readonly string m_AssetServiceName;

        [APIExtension("Testing")]
        public const int LOG_INFO = 0;
        [APIExtension("Testing")]
        public const int LOG_WARN = 1;
        [APIExtension("Testing")]
        public const int LOG_ERROR = 2;
        [APIExtension("Testing")]
        public const int LOG_FATAL = 3;
        [APIExtension("Testing")]
        public const int LOG_DEBUG = 4;

        public TestExtensionsApi(IConfig config)
        {
            m_AssetServiceName = config.GetString("AssetService", "AssetService");
        }

        public void Startup(ConfigurationLoader loader)
        {
            m_Loader = loader;
            loader.GetService(m_AssetServiceName, out m_AssetService);
            m_TestRunner = m_Loader.GetServicesByValue<TestRunner>()[0];
            List<RunScript> scriptrunners = m_Loader.GetServicesByValue<RunScript>();
            if(scriptrunners.Count > 0)
            {
                m_ScriptRunner = scriptrunners[0];
            }
        }

        [APIExtension("Testing", "_test_AddAvatarName")]
        public void TestAddAvatarName(ScriptInstance instance, LSLKey key, string firstName, string lastName)
        {
            lock(instance)
            {
                instance.Part.ObjectGroup.Scene.AvatarNameService.Store(new UGUIWithName
                {
                    FirstName = firstName,
                    LastName = lastName,
                    ID = key.AsUUID,
                    HomeURI = new Uri(m_Loader.HomeURI, UriKind.Absolute),
                    IsAuthoritative = true
                });
            }
        }

        [APIExtension("Testing", "_test_GetLandAccessList")]
        [ForcedSleep(0.1)]
        public AnArray GetLandAccessList(ScriptInstance instance)
        {
            var res = new AnArray();
            lock (instance)
            {
                ObjectPart part = instance.Part;
                ObjectGroup grp = part.ObjectGroup;
                SceneInterface scene = grp.Scene;
                ParcelInfo pInfo;
                if (scene.Parcels.TryGetValue(grp.GlobalPosition, out pInfo))
                {
                    foreach (ParcelAccessEntry pae in scene.Parcels.WhiteList[scene.ID, pInfo.ID])
                    {
                        res.Add(new LSLKey(pae.Accessor.ID));
                        res.Add(pae.Accessor.HomeURI?.ToString() ?? string.Empty);
                        res.Add(new LongInteger(pae.ExpiresAt?.AsLong ?? 0));
                    }
                }
            }
            return res;
        }

        [APIExtension("Testing", "_test_GetLandBanList")]
        [ForcedSleep(0.1)]
        public AnArray GetLandBanList(ScriptInstance instance)
        {
            var res = new AnArray();
            lock (instance)
            {
                ObjectPart part = instance.Part;
                ObjectGroup grp = part.ObjectGroup;
                SceneInterface scene = grp.Scene;
                ParcelInfo pInfo;
                if (scene.Parcels.TryGetValue(grp.GlobalPosition, out pInfo))
                {
                    foreach(ParcelAccessEntry pae in scene.Parcels.BlackList[scene.ID, pInfo.ID])
                    {
                        res.Add(new LSLKey(pae.Accessor.ID));
                        res.Add(pae.Accessor.HomeURI?.ToString() ?? string.Empty);
                        res.Add(new LongInteger(pae.ExpiresAt?.AsLong ?? 0));
                    }
                }
            }
            return res;
        }

        [APIExtension("Testing", "_test_ObjectKey2LocalId")]
        public int TestObjectKey2LocalId(ScriptInstance instance, LSLKey key)
        {
            lock(instance)
            {
                ObjectPart part;
                if(instance.Part.ObjectGroup.Scene.Primitives.TryGetValue(key, out part))
                {
                    return (int)part.LocalID[instance.Part.ObjectGroup.Scene.ID];
                }
            }
            return 0;
        }

        [APIExtension("Testing", "_test_LocalId2ObjectKey")]
        public LSLKey TestObjectLocalId2Key(ScriptInstance instance, int localid)
        {
            lock(instance)
            {
                ObjectPart part;
                if(instance.Part.ObjectGroup.Scene.Primitives.TryGetValue((uint)localid, out part))
                {
                    return part.ID;
                }
            }
            return UUID.Zero;
        }

        [APIExtension("Testing", "_test_setserverparam")]
        public void TestSetServerParam(ScriptInstance instance, string paraname, string paravalue)
        {
            lock(instance)
            {
                m_Loader.GetServerParamStorage()[UUID.Zero, paraname] = paravalue;
            }
        }

        [APIExtension("Testing", "_test_scriptresetevent")]
        public void TestScriptReset(ScriptInstance instance)
        {
            lock(instance)
            {
                instance.PostEvent(new ResetScriptEvent());
            }
        }


        [APIExtension("Testing", "_test_Shutdown")]
        public void TestShutdown(ScriptInstance instance)
        {
            lock (instance)
            {
                m_Log.Info("Shutdown triggered by script");
                if (null != m_ScriptRunner)
                {
                    m_ScriptRunner.Shutdown();
                }
                else
                {
                    m_Loader.TriggerShutdown();
                }
            }
        }

        [APIExtension("Testing", "_test_Result")]
        public void TestResult(ScriptInstance instance, int result)
        {
            lock (instance)
            {
                m_TestRunner.OtherThreadResult = (result != 0);
            }
        }

        [APIExtension("Testing", "_test_Log")]
        public void Log(ScriptInstance instance, int logLevel, string message)
        {
            switch(logLevel)
            {
                case LOG_WARN:
                    m_Log.Warn(message);
                    break;

                case LOG_ERROR:
                    m_Log.Error(message);
                    break;

                case LOG_FATAL:
                    m_Log.Fatal(message);
                    break;

                case LOG_DEBUG:
                    m_Log.Debug(message);
                    break;

                case LOG_INFO:
                default:
                    m_Log.Info(message);
                    break;
            }
        }

        [APIExtension("Testing", "_test_ossl_perms")]
        public int TestOsslPerms(ScriptInstance instance, string functionname)
        {
            lock (instance)
            {
                try
                {
                    ((Script)instance).CheckThreatLevel(functionname);
                    return 1;
                }
                catch
                {
                    return 0;
                }
            }
        }

        [APIExtension("Testing", "_test_InjectScript")]
        public int InjectScript(ScriptInstance instance, string name, string filename, int startparameter, LSLKey experienceID)
        {
            lock (instance)
            {
                UUID assetid = UUID.Random;
                ObjectPartInventoryItem item = new ObjectPartInventoryItem(UUID.Random, instance.Item)
                {
                    Name = name,
                    AssetID = assetid,
                    ExperienceID = new UEI(experienceID.ToString())
                };

                IScriptAssembly scriptAssembly = null;
                try
                {
                    using (var reader = new StreamReader(filename, new UTF8Encoding(false)))
                    {
                        m_AssetService.Store(new AssetData
                        {
                            ID = assetid,
                            Type = AssetType.LSLText,
                            Data = reader.ReadToEnd().ToUTF8Bytes()
                        });
                    }
                    using (var reader = new StreamReader(filename, new UTF8Encoding(false)))
                    {
                        scriptAssembly = CompilerRegistry.ScriptCompilers.Compile(AppDomain.CurrentDomain, UGUI.Unknown, assetid, reader, includeOpen: instance.Part.OpenScriptInclude);
                    }
                    m_Log.InfoFormat("Compilation of injected {1} ({0}) successful", assetid, name);
                }
                catch (CompilerException e)
                {
                    m_Log.ErrorFormat("Compilation of injected {1} ({0}) failed: {2}", assetid, name, e.Message);
                    m_Log.WarnFormat("Stack Trace:\n{0}", e.StackTrace);
                    return 0;
                }
                catch (Exception e)
                {
                    m_Log.ErrorFormat("Compilation of injected {1} ({0}) failed: {2}", assetid, name, e.Message);
                    m_Log.WarnFormat("Stack Trace:\n{0}", e.StackTrace);
                    return 0;
                }

                ScriptInstance scriptInstance;
                try
                {
                    scriptInstance = scriptAssembly.Instantiate(instance.Part, item);
                }
                catch (Exception e)
                {
                    m_Log.ErrorFormat("Instancing of injected {1} ({0}) failed: {2}", assetid, name, e.Message);
                    m_Log.WarnFormat("Stack Trace:\n{0}", e.StackTrace);
                    return 0;
                }
                instance.Part.Inventory.Add(item);
                item.ScriptInstance = scriptInstance;
                try
                {
                    item.ScriptInstance.Start(startparameter);
                }
                catch (Exception e)
                {
                    m_Log.ErrorFormat("Starting of injected {1} ({0}) failed: {2}", assetid, name, e.Message);
                    m_Log.WarnFormat("Stack Trace:\n{0}", e.StackTrace);
                    return 0;
                }
                return 1;
            }
        }

        [APIExtension("Testing", "_test_InjectScript")]
        public int InjectScript(ScriptInstance instance, string name, string filename, int startparameter) =>
            InjectScript(instance, name, filename, startparameter, UUID.Zero);

        [APIExtension("Testing", "_test_GetInventoryItemID")]
        public LSLKey GetInventoryKey(ScriptInstance instance, string item)
        {
            lock (instance)
            {
                ObjectPartInventoryItem objitem;
                if (instance.Part.Inventory.TryGetValue(item, out objitem))
                {
                    return objitem.ID;
                }
                return UUID.Zero;
            }
        }

        [APIExtension("Testing", "_test_InjectCollisionStart")]
        public void InjectCollisionStart(ScriptInstance instance, 
            LSLKey targetPrim, 
            LSLKey sourceLinkset,
            string name,
            int objType,
            Vector3 position,
            Quaternion rotation,
            Vector3 velocity,
            int linkNumber)
        {
            lock (instance)
            {
                ObjectPart part;
                if (instance.Part.ObjectGroup.Scene.Primitives.TryGetValue(targetPrim, out part))
                {
                    var di = new DetectInfo
                    {
                        Key = sourceLinkset,
                        Name = name,
                        Group = UGI.Unknown,
                        ObjType = (DetectedTypeFlags)objType,
                        Owner = instance.Part.Owner,
                        Position = position,
                        Rotation = rotation,
                        Velocity = velocity,
                        LinkNumber = linkNumber
                    };
                    var ev = new CollisionEvent { Type = CollisionEvent.CollisionType.Start };
                    ev.Detected.Add(di);
                    part.PostEvent(ev);
                }
            }
        }


        [APIExtension("Testing", "_test_InjectCollision")]
        public void InjectCollision(ScriptInstance instance,
            LSLKey targetPrim,
            LSLKey sourceLinkset,
            string name,
            int objType,
            Vector3 position,
            Quaternion rotation,
            Vector3 velocity,
            int linkNumber)
        {
            lock (instance)
            {
                ObjectPart part;
                if (instance.Part.ObjectGroup.Scene.Primitives.TryGetValue(targetPrim, out part))
                {
                    var di = new DetectInfo
                    {
                        Key = sourceLinkset,
                        Name = name,
                        Group = UGI.Unknown,
                        ObjType = (DetectedTypeFlags)objType,
                        Owner = instance.Part.Owner,
                        Position = position,
                        Rotation = rotation,
                        Velocity = velocity,
                        LinkNumber = linkNumber
                    };
                    var ev = new CollisionEvent { Type = CollisionEvent.CollisionType.Continuous };
                    ev.Detected.Add(di);
                    part.PostEvent(ev);
                }
            }
        }


        [APIExtension("Testing", "_test_InjectCollisionEnd")]
        public void InjectCollisionEnd(ScriptInstance instance,
            LSLKey targetPrim,
            LSLKey sourceLinkset,
            string name,
            int objType,
            Vector3 position,
            Quaternion rotation,
            Vector3 velocity,
            int linkNumber)
        {
            lock (instance)
            {
                ObjectPart part;
                if (instance.Part.ObjectGroup.Scene.Primitives.TryGetValue(targetPrim, out part))
                {
                    var di = new DetectInfo
                    {
                        Key = sourceLinkset,
                        Name = name,
                        Group = UGI.Unknown,
                        ObjType = (DetectedTypeFlags)objType,
                        Owner = instance.Part.Owner,
                        Position = position,
                        Rotation = rotation,
                        Velocity = velocity,
                        LinkNumber = linkNumber
                    };
                    var ev = new CollisionEvent { Type = CollisionEvent.CollisionType.End };
                    ev.Detected.Add(di);
                    part.PostEvent(ev);
                }
            }
        }

        [APIExtension("Testing", "_test_EnableAnimesh")]
        public void EnableAnimesh(ScriptInstance instance, LSLKey key, int enable)
        {
            lock(instance)
            {
                ObjectPart part;
                if(instance.Part.ObjectGroup.Scene.Primitives.TryGetValue(key.AsUUID, out part))
                {
                    ObjectPart.PrimitiveShape shape = part.Shape;
                    if (shape.SculptType == PrimitiveSculptType.Mesh &&
                        shape.Type == PrimitiveShapeType.Sculpt)
                    {
                        part.ExtendedMesh = new ExtendedMeshParams
                        {
                            Flags = enable != 0 ?
                                ExtendedMeshParams.MeshFlags.AnimatedMeshEnabled :
                                ExtendedMeshParams.MeshFlags.None
                        };
                    }
                }
            }
        }

        [APIExtension("Testing", "_test_GetAnimeshState")]
        public int GetAnimeshState(ScriptInstance instance, LSLKey key)
        {
            lock (instance)
            {
                ObjectPart part;
                if (instance.Part.ObjectGroup.Scene.Primitives.TryGetValue(key.AsUUID, out part))
                {
                    ObjectPart.PrimitiveShape shape = part.Shape;
                    if (shape.SculptType == PrimitiveSculptType.Mesh &&
                        shape.Type == PrimitiveShapeType.Sculpt)
                    {
                        return ((part.ExtendedMesh.Flags & ExtendedMeshParams.MeshFlags.AnimatedMeshEnabled) != 0).ToLSLBoolean();
                    }
                }
            }
            return 0;
        }
    }
}

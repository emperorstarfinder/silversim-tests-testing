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
using SilverSim.OpenSimArchiver.RegionArchiver;
using SilverSim.Scene.Management.Scene;
using SilverSim.Scene.ServiceInterfaces.Chat;
using SilverSim.Scene.ServiceInterfaces.Scene;
using SilverSim.Scene.Types.Object;
using SilverSim.Scene.Types.Scene;
using SilverSim.Scene.Types.Script;
using SilverSim.Scene.Types.Script.Events;
using SilverSim.Scripting.Common;
using SilverSim.ServiceInterfaces.Estate;
using SilverSim.ServiceInterfaces.Experience;
using SilverSim.ServiceInterfaces.Grid;
using SilverSim.Tests.Extensions;
using SilverSim.Types;
using SilverSim.Types.Asset;
using SilverSim.Types.Estate;
using SilverSim.Types.Experience;
using SilverSim.Types.Grid;
using SilverSim.Types.Inventory;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace SilverSim.Tests.Scripting
{
    public class RunScript : ITest, IPluginShutdown
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly ILog m_PublicChatLog = LogManager.GetLogger("PUBLIC_CHANNEL");
        private static readonly ILog m_DebugChatLog = LogManager.GetLogger("DEBUG_CHANNEL");

        private UUID m_AssetID;
        private string m_ScriptFile;
        private int m_TimeoutMs;
        private UUID m_RegionID;
        private readonly ManualResetEvent m_RunTimeoutEvent = new ManualResetEvent(false);
        private TestRunner m_Runner;
        private UGUI m_RegionOwner;
        private UUID m_ObjectID;
        private UGUI m_ObjectOwner;
        private UGUI m_ObjectLastOwner;
        private UGUI m_ObjectCreator;
        private UGUI m_ScriptOwner;
        private UGUI m_ScriptLastOwner;
        private UGUI m_ScriptCreator;
        private UGUI m_EstateOwner;
        private string m_RegionName;
        private string m_ObjectName;
        private string m_ObjectDescription;
        private string m_ScriptName;
        private string m_ScriptDescription;
        private string m_EstateName;
        private uint m_EstateID;
        private Vector3 m_Position;
        private Quaternion m_Rotation;
        private int m_RegionPort;
        private string m_ExperienceName;
        private UUID m_ExperienceID;
        private GridServiceInterface m_RegionStorage;
        private SceneFactoryInterface m_SceneFactory;
        private EstateServiceInterface m_EstateService;
        private SceneList m_Scenes;
        private Timer m_KillTimer;
        private int m_StartParameter;
        private string m_LoadOarFileName;
        private string[] m_AdditionalObjectConfigs = new string[0];

        private InventoryPermissionsMask m_ObjectPermissionsBase = InventoryPermissionsMask.All;
        private InventoryPermissionsMask m_ObjectPermissionsOwner = InventoryPermissionsMask.All;
        private InventoryPermissionsMask m_ObjectPermissionsGroup = InventoryPermissionsMask.All;
        private InventoryPermissionsMask m_ObjectPermissionsNext = InventoryPermissionsMask.All;
        private InventoryPermissionsMask m_ObjectPermissionsEveryone = InventoryPermissionsMask.All;

        private InventoryPermissionsMask m_ScriptPermissionsBase = InventoryPermissionsMask.All;
        private InventoryPermissionsMask m_ScriptPermissionsOwner = InventoryPermissionsMask.All;
        private InventoryPermissionsMask m_ScriptPermissionsGroup = InventoryPermissionsMask.All;
        private InventoryPermissionsMask m_ScriptPermissionsNext = InventoryPermissionsMask.All;
        private InventoryPermissionsMask m_ScriptPermissionsEveryone = InventoryPermissionsMask.All;
        private ConfigurationLoader m_Loader;

        public InventoryPermissionsMask GetPermissions(IConfig section, string name, InventoryPermissionsMask defaultvalue = InventoryPermissionsMask.All)
        {
            if(section.Contains(name))
            {
                var mask = InventoryPermissionsMask.None;
                foreach (string value in section.GetString(name).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    InventoryPermissionsMask addmask;
                    if(Enum.TryParse(value, out addmask))
                    {
                        mask |= addmask;
                    }
                }
                return mask;
            }
            else
            {
                return defaultvalue;
            }
        }

        public void Startup(ConfigurationLoader loader)
        {
            m_Scenes = loader.Scenes;
            m_Loader = loader;
            IConfig config = loader.Config.Configs[GetType().FullName];

            /* we use same asset id keying here so to make them compatible with the other scripts */
            foreach (string key in config.GetKeys())
            {
                if (UUID.TryParse(key, out m_AssetID))
                {
                    m_ScriptFile = config.GetString(key);
                    break;
                }
            }

            m_ObjectPermissionsBase = GetPermissions(config, "ObjectPermisionsBase");
            m_ObjectPermissionsOwner = GetPermissions(config, "ObjectPermisionsOwner");
            m_ObjectPermissionsGroup = GetPermissions(config, "ObjectPermisionsGroup");
            m_ObjectPermissionsNext = GetPermissions(config, "ObjectPermisionsNext");
            m_ObjectPermissionsEveryone = GetPermissions(config, "ObjectPermisionsEveryone");

            m_ScriptPermissionsBase = GetPermissions(config, "ScriptPermisionsBase");
            m_ScriptPermissionsOwner = GetPermissions(config, "ScriptPermisionsOwner");
            m_ScriptPermissionsGroup = GetPermissions(config, "ScriptPermisionsGroup");
            m_ScriptPermissionsNext = GetPermissions(config, "ScriptPermisionsNext");
            m_ScriptPermissionsEveryone = GetPermissions(config, "ScriptPermisionsEveryone");

            m_LoadOarFileName = config.GetString("OarFilename", string.Empty);

            m_TimeoutMs = config.GetInt("RunTimeout", 1000);
            m_RegionID = UUID.Parse(config.GetString("RegionID"));
            m_RegionOwner = new UGUI(config.GetString("RegionOwner"));
            m_EstateOwner = new UGUI(config.GetString("EstateOwner", m_RegionOwner.ToString()));
            m_EstateID = (uint)config.GetInt("EstateID", 100);
            m_EstateName = config.GetString("EstateName", "My Estate");

            m_ObjectID = UUID.Parse(config.GetString("ID", UUID.Random.ToString()));
            m_RegionName = config.GetString("RegionName", "Testing Region");
            m_RegionPort = config.GetInt("RegionPort", 9300);
            m_Runner = loader.GetServicesByValue<TestRunner>()[0];
            m_Position = Vector3.Parse(config.GetString("Position", "<128, 128, 23>"));
            m_Rotation = Quaternion.Parse(config.GetString("Rotation", "<0,0,0,1>"));

            m_ObjectName = config.GetString("ObjectName", "Object");
            m_ScriptName = config.GetString("ScriptName", "Script");
            m_ExperienceName = config.GetString("ExperienceName", "My Experience");
            UUID.TryParse(config.GetString("ExperienceID", UUID.Zero.ToString()), out m_ExperienceID);

            m_ObjectDescription = config.GetString("ObjectDescription", "");
            m_ScriptDescription = config.GetString("ScriptDescription", "");

            m_RegionStorage = loader.GetService<GridServiceInterface>("RegionStorage");
            m_SceneFactory = loader.GetService<SceneFactoryInterface>("DefaultSceneImplementation");
            m_EstateService = loader.GetService<EstateServiceInterface>("EstateService");

            m_ObjectOwner = new UGUI(config.GetString("ObjectOwner"));
            if (config.Contains("ObjectCreator"))
            {
                m_ObjectCreator = new UGUI(config.GetString("ObjectCreator"));
            }
            else
            {
                m_ObjectCreator = m_ObjectOwner;
            }
            if (config.Contains("ObjectLastOwner"))
            {
                m_ObjectLastOwner = new UGUI(config.GetString("ObjectLastOwner"));
            }
            else
            {
                m_ObjectLastOwner = m_ObjectOwner;
            }

            m_ScriptOwner = new UGUI(config.GetString("ScriptOwner"));
            if(config.Contains("ScriptCreator"))
            {
                m_ScriptCreator = new UGUI(config.GetString("ScriptCreator"));
            }
            else
            {
                m_ScriptCreator = m_ScriptOwner;
            }
            if (config.Contains("ScriptLastOwner"))
            {
                m_ScriptLastOwner = new UGUI(config.GetString("ScriptLastOwner"));
            }
            else
            {
                m_ScriptLastOwner = m_ScriptOwner;
            }

            m_StartParameter = config.GetInt("StartParameter", 0);

            if(string.IsNullOrEmpty(m_ScriptFile))
            {
                throw new ArgumentException("Script filename and UUID missing");
            }

            m_AdditionalObjectConfigs = config.GetString("AdditionalObjects", string.Empty).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if(m_AdditionalObjectConfigs.Length == 1 && m_AdditionalObjectConfigs[0] == string.Empty)
            {
                m_AdditionalObjectConfigs = new string[0];
            }

            CompilerRegistry.ScriptCompilers.DefaultCompilerName = config.GetString("DefaultCompiler");
        }

        public void Setup()
        {

        }

        public void Cleanup()
        {

        }

        private bool TryAddAdditionalObject(SceneInterface scene, string sectionName)
        {
            IConfig config = m_Loader.Config.Configs[sectionName];
            Vector3 position = Vector3.Parse(config.GetString("Position", m_Position.ToString()));
            Quaternion rotation = Quaternion.Parse(config.GetString("Rotation", m_Rotation.ToString()));
            UUID objectid = UUID.Parse(config.GetString("ID", UUID.Random.ToString()));

            string objectName = config.GetString("ObjectName", sectionName);
            string scriptName = config.GetString("ScriptName", "Script");
            string experienceName = config.GetString("ExperienceName", "My Experience");
            UUID experienceID;
            UUID.TryParse(config.GetString("ExperienceID", m_ExperienceID.ToString()), out experienceID);

            string objectDescription = config.GetString("ObjectDescription", "");
            string scriptDescription = config.GetString("ScriptDescription", "");

            var objectOwner = new UGUI(config.GetString("ObjectOwner", m_ObjectOwner.ToString()));
            var objectCreator = new UGUI(config.GetString("ObjectCreator", m_ObjectCreator.ToString()));
            var objectLastOwner = new UGUI(config.GetString("ObjectLastOwner", m_ObjectLastOwner.ToString()));
            var scriptOwner = new UGUI(config.GetString("ScriptOwner", m_ScriptOwner.ToString()));
            var scriptCreator = new UGUI(config.GetString("ScriptCreator", m_ScriptCreator.ToString()));
            var scriptLastOwner = new UGUI(config.GetString("ScriptLastOwner", m_ScriptLastOwner.ToString()));
            int startParameter = config.GetInt("StartParameter", m_StartParameter);

            InventoryPermissionsMask objectPermissionsBase = GetPermissions(config, "ObjectPermisionsBase", m_ObjectPermissionsBase);
            InventoryPermissionsMask objectPermissionsOwner = GetPermissions(config, "ObjectPermisionsOwner", m_ObjectPermissionsOwner);
            InventoryPermissionsMask objectPermissionsGroup = GetPermissions(config, "ObjectPermisionsGroup", m_ObjectPermissionsGroup);
            InventoryPermissionsMask objectPermissionsNext = GetPermissions(config, "ObjectPermisionsNext", m_ObjectPermissionsNext);
            InventoryPermissionsMask objectPermissionsEveryone = GetPermissions(config, "ObjectPermisionsEveryone", m_ObjectPermissionsEveryone);

            InventoryPermissionsMask scriptPermissionsBase = GetPermissions(config, "ScriptPermisionsBase", m_ScriptPermissionsBase);
            InventoryPermissionsMask scriptPermissionsOwner = GetPermissions(config, "ScriptPermisionsOwner", m_ScriptPermissionsOwner);
            InventoryPermissionsMask scriptPermissionsGroup = GetPermissions(config, "ScriptPermisionsGroup", m_ScriptPermissionsGroup);
            InventoryPermissionsMask scriptPermissionsNext = GetPermissions(config, "ScriptPermisionsNext", m_ScriptPermissionsNext);
            InventoryPermissionsMask scriptPermissionsEveryone = GetPermissions(config, "ScriptPermisionsEveryone", m_ScriptPermissionsEveryone);

            UUID assetID = UUID.Zero;
            string scriptFile = string.Empty;

            /* we use same asset id keying here so to make them compatible with the other scripts */
            foreach (string key in config.GetKeys())
            {
                if (UUID.TryParse(key, out assetID))
                {
                    scriptFile = config.GetString(key);
                    break;
                }
            }

            IScriptAssembly scriptAssembly = null;
            if (!string.IsNullOrEmpty(scriptFile))
            {
                try
                {
                    using (var reader = new StreamReader(m_ScriptFile, new UTF8Encoding(false)))
                    {
                        scriptAssembly = CompilerRegistry.ScriptCompilers.Compile(AppDomain.CurrentDomain, UGUI.Unknown, m_AssetID, reader);
                    }
                    m_Log.InfoFormat("Compilation of {1} ({0}) successful", m_AssetID, m_ScriptFile);
                }
                catch (CompilerException e)
                {
                    m_Log.ErrorFormat("Compilation of {1} ({0}) failed: {2}", m_AssetID, m_ScriptFile, e.Message);
                    m_Log.WarnFormat("Stack Trace:\n{0}", e.StackTrace.ToString());
                    return false;
                }
                catch (Exception e)
                {
                    m_Log.ErrorFormat("Compilation of {1} ({0}) failed: {2}", m_AssetID, m_ScriptFile, e.Message);
                    m_Log.WarnFormat("Stack Trace:\n{0}", e.StackTrace.ToString());
                    return false;
                }
            }

            try
            {
                ExperienceServiceInterface experienceService = scene.ExperienceService;
                if (null != experienceService)
                {
                    ExperienceInfo test;
                    if (!experienceService.TryGetValue(experienceID, out test))
                    {
                        experienceService.Add(new ExperienceInfo
                        {
                            ID = experienceID,
                            Name = experienceName,
                            Creator = scriptOwner,
                            Owner = scriptOwner,
                            Properties = ExperiencePropertyFlags.Grid /* make this grid-wide since otherwise we have to configure a lot more */
                        });
                    }
                }
                else
                {
                    experienceID = UUID.Zero;
                }
            }
            catch (Exception e)
            {
                m_Log.Error("Creating experience failed", e);
                return false;
            }

            try
            {
                var grp = new ObjectGroup();
                var part = new ObjectPart(objectid);
                grp.Add(1, part.ID, part);
                part.ObjectGroup = grp;
                grp.Owner = objectOwner;
                grp.LastOwner = objectLastOwner;
                part.Creator = objectCreator;
                part.Name = objectName;
                part.Description = objectDescription;
                part.GlobalPosition = position;
                part.GlobalRotation = rotation;
                part.BaseMask = objectPermissionsBase;
                part.OwnerMask = objectPermissionsOwner;
                part.NextOwnerMask = objectPermissionsNext;
                part.EveryoneMask = objectPermissionsEveryone;
                part.GroupMask = objectPermissionsGroup;

                var item = new ObjectPartInventoryItem()
                {
                    AssetType = AssetType.LSLText,
                    AssetID = UUID.Random,
                    InventoryType = InventoryType.LSL,
                    LastOwner = scriptLastOwner,
                    Creator = scriptCreator,
                    Owner = scriptOwner,
                    Name = scriptName,
                    Description = scriptDescription
                };
                item.Permissions.Base = scriptPermissionsBase;
                item.Permissions.Current = scriptPermissionsOwner;
                item.Permissions.EveryOne = scriptPermissionsEveryone;
                item.Permissions.Group = scriptPermissionsGroup;
                item.Permissions.NextOwner = scriptPermissionsNext;
                item.ExperienceID = experienceID;

                scene.Add(grp);
                if (scriptAssembly != null)
                {
                    ScriptInstance scriptInstance = scriptAssembly.Instantiate(part, item);
                    part.Inventory.Add(item);
                    item.ScriptInstance = scriptInstance;
                    item.ScriptInstance.Start(startParameter);
                }
            }
            catch (Exception e)
            {
                m_Log.Error("Adding object failed", e);
                return false;
            }
            return true;
        }

        private TextReader OpenFile(string name)
        {
            return new StreamReader(Path.Combine(Path.GetDirectoryName(m_ScriptFile), name));
        }

        public bool Run()
        {
            bool success = true;
            int successcnt = 0;

            m_Log.InfoFormat("Testing Execution of {1} ({0})", m_AssetID, m_ScriptFile);
            IScriptAssembly scriptAssembly = null;
            try
            {
                using (var reader = new StreamReader(m_ScriptFile, new UTF8Encoding(false)))
                {
                    scriptAssembly = CompilerRegistry.ScriptCompilers.Compile(AppDomain.CurrentDomain, UGUI.Unknown, m_AssetID, reader, includeOpen: OpenFile);
                }
                m_Log.InfoFormat("Compilation of {1} ({0}) successful", m_AssetID, m_ScriptFile);
                ++successcnt;
            }
            catch (CompilerException e)
            {
                m_Log.ErrorFormat("Compilation of {1} ({0}) failed: {2}", m_AssetID, m_ScriptFile, e.Message);
                m_Log.WarnFormat("Stack Trace:\n{0}", e.StackTrace.ToString());
                return false;
            }
            catch (Exception e)
            {
                m_Log.ErrorFormat("Compilation of {1} ({0}) failed: {2}", m_AssetID, m_ScriptFile, e.Message);
                m_Log.WarnFormat("Stack Trace:\n{0}", e.StackTrace.ToString());
                return false;
            }

            RegionInfo rInfo;
            try
            {
                var estate = new EstateInfo
                {
                    ParentEstateID = 1,
                    ID = m_EstateID,
                    Owner = m_EstateOwner,
                    Name = m_EstateName
                };
                m_EstateService.Add(estate);
                m_EstateService.RegionMap[m_RegionID] = m_EstateID;

                rInfo = new RegionInfo
                {
                    Name = m_RegionName,
                    ID = m_RegionID,
                    Location = new GridVector { GridX = 10000, GridY = 10000 },
                    Size = new GridVector { X = 256, Y = 256 },
                    ProductName = "Mainland",
                    ServerPort = (uint)m_RegionPort,
                    Owner = m_RegionOwner,
                    Flags = RegionFlags.RegionOnline
                };
                m_RegionStorage.RegisterRegion(rInfo);
            }
            catch(Exception e)
            {
                m_Log.Error("Registration of region failed", e);
                return false;
            }

            SceneInterface scene;
            try
            {
                scene = m_SceneFactory.Instantiate(rInfo);
            }
            catch(Exception e)
            {
                m_Log.ErrorFormat("Running of {1} ({0}) failed: Failed to start region ID {2}: {3}: {4}\n{5}", m_AssetID, m_ScriptFile, m_RegionID, e.GetType().FullName, e.Message, e.StackTrace);
                success = false;
                scene = null;
            }

            try
            {
                if (success)
                {
                    m_Scenes.Add(scene);
                    scene.LoadSceneSync();
                }
            }
            catch(Exception e)
            {
                m_Log.Error("Starting region failed", e);
                return false;
            }

            try
            {
                if(success)
                {
                    ExperienceServiceInterface experienceService = scene.ExperienceService;
                    if(null != experienceService)
                    {
                        experienceService.Add(new ExperienceInfo
                        {
                            ID = m_ExperienceID,
                            Name = m_ExperienceName,
                            Creator = m_ScriptOwner,
                            Owner = m_ScriptOwner,
                            Properties = ExperiencePropertyFlags.Grid /* make this grid-wide since otherwise we have to configure a lot more */
                        });
                    }
                    else
                    {
                        m_ExperienceID = UUID.Zero;
                    }
                }
            }
            catch(Exception e)
            {
                m_Log.Error("Creating experience failed", e);
                return false;
            }

            if(!string.IsNullOrEmpty(m_LoadOarFileName))
            {
                try
                {
                    using (var s = new FileStream(m_LoadOarFileName, FileMode.Open))
                    {
                        OAR.Load(m_Scenes, scene, OAR.LoadOptions.PersistUuids, s);
                    }
                }
                catch(Exception e)
                {
                    m_Log.Error("Loading oar failed", e);
                    return false;
                }
            }

            if(success)
            {
                foreach(string additionalObject in m_AdditionalObjectConfigs)
                {
                    m_Log.InfoFormat("Adding object from section {0}", additionalObject);
                    if(!TryAddAdditionalObject(scene, additionalObject))
                    {
                        success = false;
                        break;
                    }
                }
            }
            try
            {
                if (success)
                {
                    {
                        var grp = new ObjectGroup();
                        var part = new ObjectPart(m_ObjectID);
                        grp.Add(1, part.ID, part);
                        part.ObjectGroup = grp;
                        grp.Owner = m_ObjectOwner;
                        grp.LastOwner = m_ObjectLastOwner;
                        part.Creator = m_ObjectCreator;
                        part.Name = m_ObjectName;
                        part.Description = m_ObjectDescription;
                        part.GlobalPosition = m_Position;
                        part.GlobalRotation = m_Rotation;
                        part.BaseMask = m_ObjectPermissionsBase;
                        part.OwnerMask = m_ObjectPermissionsOwner;
                        part.NextOwnerMask = m_ObjectPermissionsNext;
                        part.EveryoneMask = m_ObjectPermissionsEveryone;
                        part.GroupMask = m_ObjectPermissionsGroup;

                        var item = new ObjectPartInventoryItem
                        {
                            AssetType = AssetType.LSLText,
                            AssetID = UUID.Random,
                            InventoryType = InventoryType.LSL,
                            LastOwner = m_ScriptLastOwner,
                            Creator = m_ScriptCreator,
                            Owner = m_ScriptOwner,
                            Name = m_ScriptName,
                            Description = m_ScriptDescription
                        };
                        item.Permissions.Base = m_ScriptPermissionsBase;
                        item.Permissions.Current = m_ScriptPermissionsOwner;
                        item.Permissions.EveryOne = m_ScriptPermissionsEveryone;
                        item.Permissions.Group = m_ScriptPermissionsGroup;
                        item.Permissions.NextOwner = m_ScriptPermissionsNext;
                        item.ExperienceID = m_ExperienceID;

                        scene.Add(grp);
                        ChatServiceInterface chatService = scene.GetService<ChatServiceInterface>();
                        if (chatService != null)
                        {
                            chatService.AddRegionListener(PUBLIC_CHANNEL, string.Empty, UUID.Zero, "", GetUUID, PublicChannelLog);
                            chatService.AddRegionListener(DEBUG_CHANNEL, string.Empty, UUID.Zero, "", GetUUID, DebugChannelLog);
                        }
                        ScriptInstance scriptInstance = scriptAssembly.Instantiate(part, item);
                        part.Inventory.Add(item);
                        item.ScriptInstance = scriptInstance;
                        item.ScriptInstance.Start(m_StartParameter);
                    }
                    m_KillTimer = new Timer(KillTimerCbk, null, m_TimeoutMs + 5000, Timeout.Infinite);
                    m_RunTimeoutEvent.WaitOne(m_TimeoutMs);
                    return m_Runner.OtherThreadResult;
                }
            }
            catch(Exception e)
            {
                m_Log.Error("Starting script failed", e);
                return false;
            }
            return success;
        }

        private void KillTimerCbk(object o)
        {
            Environment.Exit(3);
        }

        private UUID GetUUID() => UUID.Zero;

        private const int PUBLIC_CHANNEL = 0;
        private const int DEBUG_CHANNEL = 0x7FFFFFFF;

        private void DebugChannelLog(ListenEvent ev)
        {
            m_DebugChatLog.InfoFormat("{0} ({1}, {2} at {5}): {3}: {4}", ev.Name, ev.ID, ev.SourceType.ToString(), ev.Type.ToString(), ev.Message, ev.GlobalPosition.ToString());
        }

        private void PublicChannelLog(ListenEvent ev)
        {
            m_PublicChatLog.InfoFormat("{0} ({1}, {2} at {5}): {3}: {4}", ev.Name, ev.ID, ev.SourceType.ToString(), ev.Type.ToString(), ev.Message, ev.GlobalPosition.ToString());
        }

        public void Shutdown()
        {
            m_RunTimeoutEvent.Set();
        }

        public ShutdownOrder ShutdownOrder => ShutdownOrder.LogoutAgents;
    }
}

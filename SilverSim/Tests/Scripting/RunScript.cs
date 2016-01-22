// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using log4net;
using Nini.Config;
using SilverSim.Main.Common;
using SilverSim.Scene.Management.Scene;
using SilverSim.Scene.ServiceInterfaces.Chat;
using SilverSim.Scene.ServiceInterfaces.Scene;
using SilverSim.Scene.Types.Object;
using SilverSim.Scene.Types.Scene;
using SilverSim.Scene.Types.Script;
using SilverSim.Scene.Types.Script.Events;
using SilverSim.Scripting.Common;
using SilverSim.ServiceInterfaces.Estate;
using SilverSim.ServiceInterfaces.Grid;
using SilverSim.Tests.Extensions;
using SilverSim.Types;
using SilverSim.Types.Asset;
using SilverSim.Types.Estate;
using SilverSim.Types.Grid;
using SilverSim.Types.Inventory;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Timers;

namespace SilverSim.Tests.Scripting
{
    public class RunScript : ITest, IPluginShutdown
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly ILog m_PublicChatLog = LogManager.GetLogger("PUBLIC_CHANNEL");
        private static readonly ILog m_DebugChatLog = LogManager.GetLogger("DEBUG_CHANNEL");

        UUID m_AssetID;
        string m_ScriptFile;
        int m_TimeoutMs;
        UUID m_RegionID;
        TestRunner m_Runner;
        UUI m_RegionOwner;
        UUI m_ObjectOwner;
        UUI m_ObjectLastOwner;
        UUI m_ObjectCreator;
        UUI m_ScriptOwner;
        UUI m_ScriptLastOwner;
        UUI m_ScriptCreator;
        UUI m_EstateOwner;
        string m_RegionName;
        string m_ObjectName;
        string m_ObjectDescription;
        string m_ScriptName;
        string m_ScriptDescription;
        string m_EstateName;
        uint m_EstateID;
        Vector3 m_Position;
        Quaternion m_Rotation;
        int m_RegionPort;
        GridServiceInterface m_RegionStorage;
        SceneFactoryInterface m_SceneFactory;
        EstateServiceInterface m_EstateService;
        ConfigurationLoader m_Loader;
        
        InventoryPermissionsMask m_ObjectPermissionsBase = InventoryPermissionsMask.All;
        InventoryPermissionsMask m_ObjectPermissionsOwner = InventoryPermissionsMask.All;
        InventoryPermissionsMask m_ObjectPermissionsGroup = InventoryPermissionsMask.All;
        InventoryPermissionsMask m_ObjectPermissionsNext = InventoryPermissionsMask.All;
        InventoryPermissionsMask m_ObjectPermissionsEveryone = InventoryPermissionsMask.All;

        InventoryPermissionsMask m_ScriptPermissionsBase = InventoryPermissionsMask.All;
        InventoryPermissionsMask m_ScriptPermissionsOwner = InventoryPermissionsMask.All;
        InventoryPermissionsMask m_ScriptPermissionsGroup = InventoryPermissionsMask.All;
        InventoryPermissionsMask m_ScriptPermissionsNext = InventoryPermissionsMask.All;
        InventoryPermissionsMask m_ScriptPermissionsEveryone = InventoryPermissionsMask.All;

        public void Startup(ConfigurationLoader loader)
        {
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

            m_TimeoutMs = config.GetInt("RunTimeout", 1000);
            m_RegionID = UUID.Parse(config.GetString("RegionID"));
            m_RegionOwner = new UUI(config.GetString("RegionOwner"));
            m_EstateOwner = new UUI(config.GetString("EstateOwner", m_RegionOwner.ToString()));
            m_EstateID = (uint)config.GetInt("EstateID", 100);
            m_EstateName = config.GetString("EstateName", "My Estate");

            m_RegionName = config.GetString("RegionName", "Testing Region");
            m_RegionPort = config.GetInt("RegionPort", 9300);
            m_Runner = loader.GetServicesByValue<TestRunner>()[0];
            m_Position = Vector3.Parse(config.GetString("Position", "<128, 128, 23>"));
            m_Rotation = Quaternion.Parse(config.GetString("Rotation", "<0,0,0,1>"));

            m_ObjectName = config.GetString("ObjectName", "Object");
            m_ScriptName = config.GetString("ScriptName", "Script");

            m_ObjectDescription = config.GetString("ObjectDescription", "");
            m_ScriptDescription = config.GetString("ScriptDescription", "");

            m_RegionStorage = loader.GetService<GridServiceInterface>("RegionStorage");
            m_SceneFactory = loader.GetService<SceneFactoryInterface>("DefaultSceneImplementation");
            m_EstateService = loader.GetService<EstateServiceInterface>("EstateService");

            m_ObjectOwner = new UUI(config.GetString("ObjectOwner"));
            if (config.Contains("ObjectCreator"))
            {
                m_ObjectCreator = new UUI(config.GetString("ObjectCreator"));
            }
            else
            {
                m_ObjectCreator = m_ObjectOwner;
            }
            if (config.Contains("ObjectLastOwner"))
            {
                m_ObjectLastOwner = new UUI(config.GetString("ObjectLastOwner"));
            }
            else
            {
                m_ObjectLastOwner = m_ObjectOwner;
            }

            m_ScriptOwner = new UUI(config.GetString("ScriptOwner"));
            if(config.Contains("ScriptCreator"))
            {
                m_ScriptCreator = new UUI(config.GetString("ScriptCreator"));
            }
            else
            {
                m_ScriptCreator = m_ScriptOwner;
            }
            if (config.Contains("ScriptLastOwner"))
            {
                m_ScriptLastOwner = new UUI(config.GetString("ScriptLastOwner"));
            }
            else
            {
                m_ScriptLastOwner = m_ScriptOwner;
            }

            if(string.IsNullOrEmpty(m_ScriptFile))
            {
                throw new ArgumentException("Script filename and UUID missing");
            }

            CompilerRegistry.ScriptCompilers.DefaultCompilerName = config.GetString("DefaultCompiler");
        }

        public void Setup()
        {

        }

        public void Cleanup()
        {
        }

        public bool Run()
        {
            bool success = true;
            int successcnt = 0;

            m_Log.InfoFormat("Testing Execution of {1} ({0})", m_AssetID, m_ScriptFile);
            IScriptAssembly scriptAssembly = null;
            try
            {
                using (TextReader reader = new StreamReader(m_ScriptFile, new UTF8Encoding(false)))
                {
                    scriptAssembly = CompilerRegistry.ScriptCompilers.Compile(AppDomain.CurrentDomain, UUI.Unknown, m_AssetID, reader);
                }
                m_Log.InfoFormat("Compilation of {1} ({0}) successful", m_AssetID, m_ScriptFile);
                ++successcnt;
            }
            catch (CompilerException e)
            {
                m_Log.ErrorFormat("Compilation of {1} ({0}) failed: {2}", m_AssetID, m_ScriptFile, e.Message);
                m_Log.WarnFormat("Stack Trace:\n{0}", e.StackTrace.ToString());
                success = false;
            }
            catch (Exception e)
            {
                m_Log.ErrorFormat("Compilation of {1} ({0}) failed: {2}", m_AssetID, m_ScriptFile, e.Message);
                m_Log.WarnFormat("Stack Trace:\n{0}", e.StackTrace.ToString());
                success = false;
            }

            EstateInfo estate = new EstateInfo();
            estate.ParentEstateID = 1;
            estate.ID = m_EstateID;
            estate.Owner = m_EstateOwner;
            estate.Name = m_EstateName;
            m_EstateService.Add(estate);
            m_EstateService.RegionMap[m_RegionID] = m_EstateID;

            RegionInfo rInfo = new RegionInfo();
            rInfo.Name = m_RegionName;
            rInfo.ID = m_RegionID;
            rInfo.Location.GridX = 10000;
            rInfo.Location.GridY = 10000;
            rInfo.ProductName = "Mainland";
            rInfo.ServerPort = (uint)m_RegionPort;
            rInfo.Owner = m_RegionOwner;
            rInfo.Flags = RegionFlags.RegionOnline;
            m_RegionStorage.RegisterRegion(rInfo);

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

            if (success)
            {
                SceneManager.Scenes.Add(scene);
                scene.LoadSceneSync();
            }

            if(success)
            {
                using (System.Timers.Timer runTimeoutTimer = new System.Timers.Timer(m_TimeoutMs))
                {
                    runTimeoutTimer.Elapsed += delegate (object o, ElapsedEventArgs args) { m_Loader.TriggerShutdown(); };
                    runTimeoutTimer.AutoReset = false;
                    runTimeoutTimer.Enabled = true;
                    ObjectGroup grp = new ObjectGroup();
                    ObjectPart part = new ObjectPart();
                    part.ID = UUID.Random;
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

                    ObjectPartInventoryItem item = new ObjectPartInventoryItem();
                    item.AssetType = AssetType.LSLText;
                    item.AssetID = UUID.Random;
                    item.InventoryType = InventoryType.LSLText;
                    item.LastOwner = m_ScriptLastOwner;
                    item.Creator = m_ScriptCreator;
                    item.Owner = m_ScriptOwner;
                    item.Name = m_ScriptName;
                    item.Description = m_ScriptDescription;
                    item.Permissions.Base = m_ScriptPermissionsBase;
                    item.Permissions.Current = m_ScriptPermissionsOwner;
                    item.Permissions.EveryOne = m_ScriptPermissionsEveryone;
                    item.Permissions.Group = m_ScriptPermissionsGroup;
                    item.Permissions.NextOwner = m_ScriptPermissionsNext;

                    scene.Add(grp);
                    ChatServiceInterface chatService = scene.GetService<ChatServiceInterface>();
                    if (null != chatService)
                    {
                        chatService.AddRegionListener(PUBLIC_CHANNEL, string.Empty, UUID.Zero, "", GetUUID, PublicChannelLog);
                        chatService.AddRegionListener(DEBUG_CHANNEL, string.Empty, UUID.Zero, "", GetUUID, DebugChannelLog);
                    }
                    ScriptInstance scriptInstance = scriptAssembly.Instantiate(part, item);
                    item.ScriptInstance = scriptInstance;
                    item.ScriptInstance.IsRunning = true;
                    item.ScriptInstance.Reset();
                }
                return m_Runner.OtherThreadResult;
            }
            return success;
        }

        UUID GetUUID()
        {
            return UUID.Zero;
        }

        const int PUBLIC_CHANNEL = 0;
        const int DEBUG_CHANNEL = 0x7FFFFFFF;

        void DebugChannelLog(ListenEvent ev)
        {
            m_DebugChatLog.InfoFormat("{0} ({1}, {2} at {5}): {3}: {4}", ev.Name, ev.ID, ev.SourceType.ToString(), ev.Type.ToString(), ev.Message, ev.GlobalPosition.ToString());
        }

        void PublicChannelLog(ListenEvent ev)
        {
            m_PublicChatLog.InfoFormat("{0} ({1}, {2} at {5}): {3}: {4}", ev.Name, ev.ID, ev.SourceType.ToString(), ev.Type.ToString(), ev.Message, ev.GlobalPosition.ToString());
        }

        public void Shutdown()
        {
        }

        public ShutdownOrder ShutdownOrder
        {
            get 
            {
                return ShutdownOrder.LogoutAgents;
            }
        }
    }
}

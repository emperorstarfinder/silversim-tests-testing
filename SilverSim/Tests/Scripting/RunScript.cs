// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using log4net;
using Nini.Config;
using SilverSim.Main.Common;
using SilverSim.Scene.Management.Scene;
using SilverSim.Scene.Types.Object;
using SilverSim.Scene.Types.Scene;
using SilverSim.Scene.Types.Script;
using SilverSim.Scene.Types.Script.Events;
using SilverSim.Scripting.Common;
using SilverSim.Tests.Extensions;
using SilverSim.Types;
using SilverSim.Types.Asset;
using SilverSim.Types.Inventory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Timers;

namespace SilverSim.Tests.Scripting
{
    public class RunScript : ITest, IPluginShutdown
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        UUID m_AssetID;
        string m_ScriptFile;
        int m_TimeoutMs;
        UUID m_RegionID;
        ManualResetEvent m_RunTimeoutEvent = new ManualResetEvent(false);
        TestRunner m_Runner;
        UUI m_ObjectOwner;
        UUI m_ObjectLastOwner;
        UUI m_ObjectCreator;
        UUI m_ScriptOwner;
        UUI m_ScriptLastOwner;
        UUI m_ScriptCreator;
        string m_ObjectName;
        string m_ObjectDescription;
        string m_ScriptName;
        string m_ScriptDescription;
        Vector3 m_Position;
        Quaternion m_Rotation;
        
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
            m_Runner = loader.GetServicesByValue<TestRunner>()[0];
            m_Position = Vector3.Parse(config.GetString("Position", "<128, 128, 23>"));
            m_Rotation = Quaternion.Parse(config.GetString("Rotation", "<0,0,0,1>"));

            m_ObjectName = config.GetString("ObjectName", "Object");
            m_ScriptName = config.GetString("ScriptName", "Script");

            m_ObjectDescription = config.GetString("ObjectDescription", "");
            m_ScriptDescription = config.GetString("ScriptDescription", "");

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
                using (TextReader reader = new StreamReader(m_ScriptFile))
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

            SceneInterface scene = null;
            if(success && !SceneManager.Scenes.TryGetValue(m_RegionID, out scene))
            {
                m_Log.ErrorFormat("Running of {1} ({0}) failed: Region ID {2} not found", m_AssetID, m_ScriptFile, m_RegionID);
                success = false;
            }

            if(success)
            {
                {
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
                    ScriptInstance scriptInstance = scriptAssembly.Instantiate(part, item);
                    item.ScriptInstance = scriptInstance;
                    item.ScriptInstance.ThreadPool = scene.ScriptThreadPool;
                    item.ScriptInstance.PostEvent(new ResetScriptEvent());
                }
                m_RunTimeoutEvent.WaitOne(m_TimeoutMs);
                return m_Runner.OtherThreadResult;
            }
            return success;
        }

        public void Shutdown()
        {
            m_RunTimeoutEvent.Set();
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

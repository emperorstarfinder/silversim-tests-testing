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
using SilverSim.ServiceInterfaces.Account;
using SilverSim.ServiceInterfaces.Asset;
using SilverSim.ServiceInterfaces.AvatarName;
using SilverSim.ServiceInterfaces.Estate;
using SilverSim.ServiceInterfaces.Experience;
using SilverSim.ServiceInterfaces.Grid;
using SilverSim.Tests.Extensions;
using SilverSim.Types;
using SilverSim.Types.Account;
using SilverSim.Types.Asset;
using SilverSim.Types.Estate;
using SilverSim.Types.Experience;
using SilverSim.Types.Grid;
using SilverSim.Types.Inventory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private UGUIWithName m_RegionOwner;
        private UUID m_ObjectID;
        private UGUIWithName m_ObjectOwner;
        private UGUIWithName m_ObjectLastOwner;
        private UGUIWithName m_ObjectCreator;
        private UGUIWithName m_ScriptOwner;
        private UGUIWithName m_ScriptLastOwner;
        private UGUIWithName m_ScriptCreator;
        private UGUIWithName m_EstateOwner;
        private RegionAccess m_RegionAccess;
        private string m_RegionName;
        private string m_ProductName;
        private GridVector m_RegionLocation;
        private GridVector m_RegionSize;
        private string m_ObjectName;
        private string m_ObjectDescription;
        private UUID m_ItemID;
        private UUID m_RezzingObjID;
        private string m_ScriptName;
        private string m_ScriptDescription;
        private string m_EstateName;
        private uint m_EstateID;
        private Vector3 m_Position;
        private Quaternion m_Rotation;
        private int m_RegionPort;
        private UEI m_ExperienceID;
        private GridServiceInterface m_RegionStorage;
        private SceneFactoryInterface m_SceneFactory;
        private EstateServiceInterface m_EstateService;
        private AvatarNameServiceInterface m_AvatarNameService;
        private UserAccountServiceInterface m_UserAccountService;
        private SceneList m_Scenes;
        private Timer m_KillTimer;
        private int m_StartParameter;
        private string m_LoadOarFileName;
        private string[] m_AdditionalObjectConfigs = new string[0];
        private string[] m_AdditionalInventoryConfigs = new string[0];
        private string m_AssetSourcesConfig = string.Empty;
        private string m_ScriptStatesConfig = string.Empty;
        private readonly Dictionary<UUID, byte[]> m_ScriptStates = new Dictionary<UUID, byte[]>();

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

        private string m_GatekeeperURI;
        public void Startup(ConfigurationLoader loader)
        {
            m_Scenes = loader.Scenes;
            m_Loader = loader;
            m_GatekeeperURI = loader.GatekeeperURI;
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
            m_RegionOwner = new UGUIWithName(config.GetString("RegionOwner")) { IsAuthoritative = true };
            m_EstateOwner = new UGUIWithName(config.GetString("EstateOwner", m_RegionOwner.ToString())) { IsAuthoritative = true };
            m_EstateID = (uint)config.GetInt("EstateID", 100);
            m_EstateName = config.GetString("EstateName", "My Estate");

            m_ObjectID = UUID.Parse(config.GetString("ID", UUID.Random.ToString()));
            m_RegionName = config.GetString("RegionName", "Testing Region");
            m_ProductName = config.GetString("RegionProductName", "Mainland");
            m_RegionLocation = new GridVector(config.GetString("RegionLocation", "10000,10000"), 256);
            m_RegionSize = new GridVector(config.GetString("RegionSize", "1,1"), 256);
            m_RegionAccess = (RegionAccess)Enum.Parse(typeof(RegionAccess), config.GetString("RegionAccess", "PG"));
            m_RegionPort = config.GetInt("RegionPort", 9300);
            m_Runner = loader.GetServicesByValue<TestRunner>()[0];
            m_Position = Vector3.Parse(config.GetString("Position", "<128, 128, 23>"));
            m_Rotation = Quaternion.Parse(config.GetString("Rotation", "<0,0,0,1>"));

            m_ItemID = UUID.Parse(config.GetString("ScriptItemID", UUID.Random.ToString()));
            m_RezzingObjID = UUID.Parse(config.GetString("RezzingObjectID", UUID.Zero.ToString()));
            m_ObjectName = config.GetString("ObjectName", "Object");
            m_ScriptName = config.GetString("ScriptName", "Script");
            string experienceName = config.GetString("ExperienceName", "My Experience");
            UUID experienceID;
            UUID.TryParse(config.GetString("ExperienceID", UUID.Zero.ToString()), out experienceID);
            m_ExperienceID = new UEI(experienceID, experienceName, null);

            m_ObjectDescription = config.GetString("ObjectDescription", "");
            m_ScriptDescription = config.GetString("ScriptDescription", "");

            m_RegionStorage = loader.GetService<GridServiceInterface>("RegionStorage");
            m_SceneFactory = loader.GetService<SceneFactoryInterface>("DefaultSceneImplementation");
            m_EstateService = loader.GetService<EstateServiceInterface>("EstateService");
            m_AvatarNameService = loader.GetService<AvatarNameServiceInterface>("AvatarNameStorage");
            m_UserAccountService = loader.GetService<UserAccountServiceInterface>("UserAccountService");

            m_AvatarNameService.Store(m_RegionOwner);
            m_AvatarNameService.Store(m_EstateOwner);
            try
            {
                m_UserAccountService.Add(new UserAccount
                {
                    Principal = m_RegionOwner,
                    IsLocalToGrid = true,
                });
            }
            catch
            {
                m_Log.Info("UserAccount creation failed for RegionOwner");
            }

            if(!m_EstateOwner.EqualsGrid(m_RegionOwner))
            {
                try
                {
                    m_UserAccountService.Add(new UserAccount
                    {
                        Principal = m_RegionOwner,
                        IsLocalToGrid = true,
                    });
                }
                catch
                {
                    m_Log.Info("UserAccount creation failed for EstateOwner");
                }
            }

            m_ObjectOwner = new UGUIWithName(config.GetString("ObjectOwner")) { IsAuthoritative = true };
            m_AvatarNameService.Store(m_ObjectOwner);
            if (config.Contains("ObjectCreator"))
            {
                m_ObjectCreator = new UGUIWithName(config.GetString("ObjectCreator")) { IsAuthoritative = true };
                m_AvatarNameService.Store(m_ObjectCreator);
                try
                {
                    m_UserAccountService.Add(new UserAccount
                    {
                        Principal = m_ObjectCreator,
                        IsLocalToGrid = true,
                    });
                }
                catch
                {
                    /* intentionally ignored */
                }
            }
            else
            {
                m_ObjectCreator = m_ObjectOwner;
            }
            if (config.Contains("ObjectLastOwner"))
            {
                m_ObjectLastOwner = new UGUIWithName(config.GetString("ObjectLastOwner")) { IsAuthoritative = true };
                m_AvatarNameService.Store(m_ObjectLastOwner);
                try
                {
                    m_UserAccountService.Add(new UserAccount
                    {
                        Principal = m_ObjectLastOwner,
                        IsLocalToGrid = true,
                    });
                }
                catch
                {
                    /* intentionally ignored */
                }
            }
            else
            {
                m_ObjectLastOwner = m_ObjectOwner;
            }

            m_ScriptOwner = new UGUIWithName(config.GetString("ScriptOwner")) { IsAuthoritative = true };
            m_AvatarNameService.Store(m_ScriptOwner);
            if (config.Contains("ScriptCreator"))
            {
                m_ScriptCreator = new UGUIWithName(config.GetString("ScriptCreator")) { IsAuthoritative = true };
                m_AvatarNameService.Store(m_ScriptCreator);
                try
                {
                    m_UserAccountService.Add(new UserAccount
                    {
                        Principal = m_ScriptCreator,
                        IsLocalToGrid = true,
                    });
                }
                catch
                {
                    /* intentionally ignored */
                }
            }
            else
            {
                m_ScriptCreator = m_ScriptOwner;
            }
            if (config.Contains("ScriptLastOwner"))
            {
                m_ScriptLastOwner = new UGUIWithName(config.GetString("ScriptLastOwner")) { IsAuthoritative = true };
                m_AvatarNameService.Store(m_ScriptLastOwner);
                try
                {
                    m_UserAccountService.Add(new UserAccount
                    {
                        Principal = m_ScriptLastOwner,
                        IsLocalToGrid = true,
                    });
                }
                catch
                {
                    /* intentionally ignored */
                }
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

            m_AdditionalInventoryConfigs = config.GetString("AdditionalInventories", string.Empty).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if(m_AdditionalInventoryConfigs.Length == 1 && m_AdditionalInventoryConfigs[0] == string.Empty)
            {
                m_AdditionalInventoryConfigs = new string[0];
            }

            m_AssetSourcesConfig = config.GetString("AssetSources", string.Empty);

            m_ScriptStatesConfig = config.GetString("ScriptStates", string.Empty);

            CompilerRegistry.ScriptCompilers.DefaultCompilerName = config.GetString("DefaultCompiler");
        }

        public void Setup()
        {

        }

        public void Cleanup()
        {
            /* intentionally left empty */
        }

        private void AddAssets(AssetServiceInterface assetService)
        {
            IConfig config = m_Loader.Config.Configs[m_AssetSourcesConfig];
            foreach(string k in config.GetKeys())
            {
                string fname = config.GetString(k);
                using (FileStream fs = new FileStream(fname, FileMode.Open))
                {
                    assetService.Store(new AssetData
                    {
                        FileName = fname,
                        Data = fs.ReadToStreamEnd()
                    });
                }
            }
        }

        private void AddScriptStates()
        {
            IConfig config = m_Loader.Config.Configs[m_ScriptStatesConfig];
            foreach (string k in config.GetKeys())
            {
                UUID stateId;
                if (UUID.TryParse(k, out stateId))
                {
                    using (FileStream fs = new FileStream(config.GetString(k), FileMode.Open))
                    {
                        m_ScriptStates.Add(stateId, fs.ReadToStreamEnd());
                    }
                }
            }
        }

        private void AddAdditionalInventory(ObjectPart part, string sectionName)
        {
            IConfig config = m_Loader.Config.Configs[sectionName];
            var creator = new UGUIWithName(config.GetString("Creator", m_ObjectCreator.ToString())) { IsAuthoritative = true };
            var owner = new UGUIWithName(config.GetString("Owner", m_ObjectOwner.ToString())) { IsAuthoritative = true };
            var lastOwner = new UGUIWithName(config.GetString("LastOwner", m_ObjectLastOwner.ToString())) { IsAuthoritative = true };
            m_AvatarNameService.Store(creator);
            m_AvatarNameService.Store(owner);
            m_AvatarNameService.Store(lastOwner);
            var item = new ObjectPartInventoryItem
            {
                Name = config.GetString("Name"),
                Description = config.GetString("Description", string.Empty),
                AssetID = new UUID(config.GetString("AssetID", UUID.Random.ToString())),
                AssetTypeName = config.GetString("AssetType"),
                Creator = creator,
                Owner = owner,
                LastOwner = lastOwner,
                InventoryTypeName = config.GetString("InventoryType"),
                Flags = (InventoryFlags)config.GetInt("Flags", 0),
                IsGroupOwned = config.GetBoolean("IsGroupOwned", false)
            };
            item.Permissions.Base = (InventoryPermissionsMask)config.GetInt("BasePermissions", (int)InventoryPermissionsMask.Every);
            item.Permissions.Current = (InventoryPermissionsMask)config.GetInt("CurrentPermissions", (int)InventoryPermissionsMask.Every);
            item.Permissions.EveryOne = (InventoryPermissionsMask)config.GetInt("EveryOnePermissions", (int)InventoryPermissionsMask.Every);
            item.Permissions.Group = (InventoryPermissionsMask)config.GetInt("GroupPermissions", (int)InventoryPermissionsMask.Every);
            item.Permissions.NextOwner = (InventoryPermissionsMask)config.GetInt("NextOwnerPermissions", (int)InventoryPermissionsMask.Every);

            part.Inventory.Add(item);
        }

        private bool TryAddAdditionalObject(SceneInterface scene, string sectionName)
        {
            IConfig config = m_Loader.Config.Configs[sectionName];
            Vector3 position = Vector3.Parse(config.GetString("Position", m_Position.ToString()));
            Quaternion rotation = Quaternion.Parse(config.GetString("Rotation", m_Rotation.ToString()));
            UUID objectid = UUID.Parse(config.GetString("ID", UUID.Random.ToString()));
            int scriptPin = config.GetInt("ScriptAccessPin", 0);
            string objectName = config.GetString("ObjectName", sectionName);
            string scriptName = config.GetString("ScriptName", "Script");
            string experienceName = config.GetString("ExperienceName", "My Experience");
            UUID itemID = UUID.Parse(config.GetString("ScriptItemID", UUID.Zero.ToString()));
            UUID rezzingObjID = UUID.Parse(config.GetString("RezzingObjectID", UUID.Zero.ToString()));
            UEI experienceID;
            UUID expID;
            UUID.TryParse(config.GetString("ExperienceID", m_ExperienceID.ToString()), out expID);
            experienceID = new UEI(expID, experienceName, null);

            string objectDescription = config.GetString("ObjectDescription", "");
            string scriptDescription = config.GetString("ScriptDescription", "");

            var objectOwner = new UGUIWithName(config.GetString("ObjectOwner", m_ObjectOwner.ToString())) { IsAuthoritative = true };
            m_AvatarNameService.Store(objectOwner);
            try
            {
                m_UserAccountService.Add(new UserAccount
                {
                    Principal = objectOwner,
                    IsLocalToGrid = true,
                });
            }
            catch
            {
                /* intentionally ignored */
            }
            var objectCreator = new UGUIWithName(config.GetString("ObjectCreator", m_ObjectCreator.ToString())) { IsAuthoritative = true };
            m_AvatarNameService.Store(objectCreator);
            try
            {
                m_UserAccountService.Add(new UserAccount
                {
                    Principal = objectCreator,
                    IsLocalToGrid = true,
                });
            }
            catch
            {
                /* intentionally ignored */
            }
            var objectLastOwner = new UGUIWithName(config.GetString("ObjectLastOwner", m_ObjectLastOwner.ToString())) { IsAuthoritative = true };
            m_AvatarNameService.Store(objectLastOwner);
            try
            {
                m_UserAccountService.Add(new UserAccount
                {
                    Principal = objectLastOwner,
                    IsLocalToGrid = true,
                });
            }
            catch
            {
                /* intentionally ignored */
            }
            var scriptOwner = new UGUIWithName(config.GetString("ScriptOwner", m_ScriptOwner.ToString())) { IsAuthoritative = true };
            m_AvatarNameService.Store(scriptOwner);
            try
            {
                m_UserAccountService.Add(new UserAccount
                {
                    Principal = scriptOwner,
                    IsLocalToGrid = true,
                });
            }
            catch
            {
                /* intentionally ignored */
            }
            var scriptCreator = new UGUIWithName(config.GetString("ScriptCreator", m_ScriptCreator.ToString())) { IsAuthoritative = true };
            m_AvatarNameService.Store(scriptCreator);
            try
            {
                m_UserAccountService.Add(new UserAccount
                {
                    Principal = scriptCreator,
                    IsLocalToGrid = true,
                });
            }
            catch
            {
                /* intentionally ignored */
            }
            var scriptLastOwner = new UGUIWithName(config.GetString("ScriptLastOwner", m_ScriptLastOwner.ToString())) { IsAuthoritative = true };
            m_AvatarNameService.Store(scriptLastOwner);
            try
            {
                m_UserAccountService.Add(new UserAccount
                {
                    Principal = scriptLastOwner,
                    IsLocalToGrid = true,
                });
            }
            catch
            {
                /* intentionally ignored */
            }
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
                if(itemID == UUID.Zero)
                {
                    itemID = UUID.Random;
                }
                try
                {
                    using (var reader = new StreamReader(scriptFile, new UTF8Encoding(false)))
                    {
                        scriptAssembly = CompilerRegistry.ScriptCompilers.Compile(AppDomain.CurrentDomain, UGUI.Unknown, m_AssetID, reader);
                    }
                    m_Log.InfoFormat("Compilation of {1} ({0}) successful", m_AssetID, scriptFile);
                }
                catch (CompilerException e)
                {
                    m_Log.ErrorFormat("Compilation of {1} ({0}) failed: {2}", m_AssetID, scriptFile, e.Message);
                    m_Log.WarnFormat("Stack Trace:\n{0}", e.StackTrace.ToString());
                    return false;
                }
                catch (Exception e)
                {
                    m_Log.ErrorFormat("Compilation of {1} ({0}) failed: {2}", m_AssetID, scriptFile, e.Message);
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
                            Creator = scriptOwner,
                            Owner = scriptOwner,
                            Properties = ExperiencePropertyFlags.Grid /* make this grid-wide since otherwise we have to configure a lot more */
                        });
                    }
                }
                else
                {
                    experienceID = UEI.Unknown;
                }
            }
            catch (Exception e)
            {
                m_Log.Error("Creating experience failed", e);
                return false;
            }

            string[] additionalInventoryConfigs = config.GetString("AdditionalInventories", string.Empty).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (additionalInventoryConfigs.Length == 1 && additionalInventoryConfigs[0] == string.Empty)
            {
                additionalInventoryConfigs = new string[0];
            }

            try
            {
                var grp = new ObjectGroup
                {
                    RezzingObjectID = rezzingObjID
                };
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
                part.ScriptAccessPin = scriptPin;

                var item = new ObjectPartInventoryItem(itemID)
                {
                    AssetType = AssetType.LSLText,
                    AssetID = assetID,
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

                foreach (string invconfig in additionalInventoryConfigs)
                {
                    AddAdditionalInventory(part, invconfig);
                }

                if (scriptAssembly != null)
                {
                    byte[] serializedState;
                    m_ScriptStates.TryGetValue(item.ID, out serializedState);
                    ScriptInstance scriptInstance = scriptAssembly.Instantiate(part, item, serializedState);
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
            m_Log.InfoFormat("Testing Execution of {1} ({0})", m_AssetID, m_ScriptFile);
            IScriptAssembly scriptAssembly = null;
            try
            {
                using (var reader = new StreamReader(m_ScriptFile, new UTF8Encoding(false)))
                {
                    scriptAssembly = CompilerRegistry.ScriptCompilers.Compile(AppDomain.CurrentDomain, UGUI.Unknown, m_AssetID, reader, includeOpen: OpenFile);
                }
                m_Log.InfoFormat("Compilation of {1} ({0}) successful", m_AssetID, m_ScriptFile);
            }
            catch (CompilerException e)
            {
                m_Log.ErrorFormat("Compilation of {1} ({0}) failed: {2}", m_AssetID, m_ScriptFile, e.Message);
                m_Log.WarnFormat("Stack Trace:\n{0}", e.StackTrace);
                return false;
            }
            catch (Exception e)
            {
                m_Log.ErrorFormat("Compilation of {1} ({0}) failed: {2}", m_AssetID, m_ScriptFile, e.Message);
                m_Log.WarnFormat("Stack Trace:\n{0}", e.StackTrace);
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
                    Location = m_RegionLocation,
                    Size = m_RegionSize,
                    ProductName = m_ProductName,
                    ServerPort = (uint)m_RegionPort,
                    Owner = m_RegionOwner,
                    Flags = RegionFlags.RegionOnline,
                    Access = m_RegionAccess,
                    GridURI = m_GatekeeperURI
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
                return false;
            }

            try
            {
                m_Scenes.Add(scene);
                scene.LoadSceneSync();
            }
            catch(Exception e)
            {
                m_Log.Error("Starting region failed", e);
                return false;
            }

            try
            {
                ExperienceServiceInterface experienceService = scene.ExperienceService;
                if(experienceService != null)
                {
                    experienceService.Add(new ExperienceInfo
                    {
                        ID = m_ExperienceID,
                        Creator = m_ScriptOwner,
                        Owner = m_ScriptOwner,
                        Properties = ExperiencePropertyFlags.Grid /* make this grid-wide since otherwise we have to configure a lot more */
                    });
                }
                else
                {
                    m_ExperienceID = UEI.Unknown;
                }
            }
            catch(Exception e)
            {
                m_Log.Error("Creating experience failed", e);
                return false;
            }

            if(!string.IsNullOrEmpty(m_AssetSourcesConfig))
            {
                AddAssets(scene.AssetService);
            }

            if(!string.IsNullOrEmpty(m_ScriptStatesConfig))
            {
                AddScriptStates();
            }

            if (!string.IsNullOrEmpty(m_LoadOarFileName))
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

            m_Runner.OtherThreadResult = false;

            foreach (string additionalObject in m_AdditionalObjectConfigs)
            {
                m_Log.InfoFormat("Adding object from section {0}", additionalObject);
                if(!TryAddAdditionalObject(scene, additionalObject))
                {
                    m_Log.Info("Failed to add object");
                    return false;
                }
            }

            try
            {
                var grp = new ObjectGroup
                {
                    RezzingObjectID = m_RezzingObjID
                };
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

                var item = new ObjectPartInventoryItem(m_ItemID)
                {
                    AssetType = AssetType.LSLText,
                    AssetID = m_AssetID,
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

                foreach (string invconfig in m_AdditionalInventoryConfigs)
                {
                    AddAdditionalInventory(part, invconfig);
                }
                ChatServiceInterface chatService = scene.GetService<ChatServiceInterface>();
                if (chatService != null)
                {
                    chatService.AddRegionListener(PUBLIC_CHANNEL, string.Empty, UUID.Zero, "", GetUUID, null, PublicChannelLog);
                    chatService.AddRegionListener(DEBUG_CHANNEL, string.Empty, UUID.Zero, "", GetUUID, null, DebugChannelLog);
                }
                byte[] serializedState;
                m_ScriptStates.TryGetValue(item.ID, out serializedState);
                ScriptInstance scriptInstance = scriptAssembly.Instantiate(part, item, serializedState);
                part.Inventory.Add(item);
                item.ScriptInstance = scriptInstance;
                item.ScriptInstance.Start(m_StartParameter);
                m_Log.Info("Script started");

                if (Debugger.IsAttached)
                {
                    m_RunTimeoutEvent.WaitOne();
                }
                else
                {
                    m_KillTimer = new Timer(KillTimerCbk, null, m_TimeoutMs + 5000, Timeout.Infinite);
                    m_RunTimeoutEvent.WaitOne(m_TimeoutMs);
                }
                return m_Runner.OtherThreadResult;
            }
            catch (Exception e)
            {
                m_Log.Error("Starting script failed", e);
                return false;
            }
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
            if (ev.TargetID != UUID.Zero)
            {
                m_DebugChatLog.InfoFormat("{0} ({1}, {2} at {5}) to {6}: {3}: {4}", ev.Name, ev.ID, ev.SourceType.ToString(), ev.Type.ToString(), ev.Message, ev.GlobalPosition.ToString(), ev.TargetID);
            }
            else
            {
                m_DebugChatLog.InfoFormat("{0} ({1}, {2} at {5}): {3}: {4}", ev.Name, ev.ID, ev.SourceType.ToString(), ev.Type.ToString(), ev.Message, ev.GlobalPosition.ToString());
            }
        }

        private void PublicChannelLog(ListenEvent ev)
        {
            if (ev.TargetID != UUID.Zero)
            {
                m_PublicChatLog.InfoFormat("{0} ({1}, {2} at {5}) to {6}: {3}: {4}", ev.Name, ev.ID, ev.SourceType.ToString(), ev.Type.ToString(), ev.Message, ev.GlobalPosition.ToString(), ev.TargetID);
            }
            else
            {
                m_PublicChatLog.InfoFormat("{0} ({1}, {2} at {5}): {3}: {4}", ev.Name, ev.ID, ev.SourceType.ToString(), ev.Type.ToString(), ev.Message, ev.GlobalPosition.ToString());
            }
        }

        public void Shutdown()
        {
            m_RunTimeoutEvent.Set();
        }

        public ShutdownOrder ShutdownOrder => ShutdownOrder.LogoutAgents;
    }
}

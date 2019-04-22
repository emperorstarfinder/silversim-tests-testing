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
using SilverSim.Main.Common.Caps;
using SilverSim.Main.Common.CmdIO;
using SilverSim.Scene.Management.Scene;
using SilverSim.Scene.Types.Object;
using SilverSim.Scene.Types.Scene;
using SilverSim.Scene.Types.Script;
using SilverSim.Scene.Types.Script.Events;
using SilverSim.Scripting.Lsl;
using SilverSim.ServiceInterfaces.Account;
using SilverSim.ServiceInterfaces.Asset;
using SilverSim.ServiceInterfaces.Experience;
using SilverSim.ServiceInterfaces.Friends;
using SilverSim.ServiceInterfaces.Grid;
using SilverSim.ServiceInterfaces.Groups;
using SilverSim.ServiceInterfaces.IM;
using SilverSim.ServiceInterfaces.Inventory;
using SilverSim.ServiceInterfaces.PortControl;
using SilverSim.ServiceInterfaces.Profile;
using SilverSim.ServiceInterfaces.UserAgents;
using SilverSim.ServiceInterfaces.UserSession;
using SilverSim.Tests.Viewer.UDP;
using SilverSim.Threading;
using SilverSim.Types;
using SilverSim.Types.Account;
using SilverSim.Types.Agent;
using SilverSim.Types.Grid;
using SilverSim.Types.IM;
using SilverSim.Types.ServerURIs;
using SilverSim.Viewer.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SilverSim.Tests.Viewer
{
    [LSLImplementation]
    [ScriptApiName("ViewerControl")]
    [PluginName("ViewerControl")]
    [Description("LSL Viewer Control API")]
    public partial class ViewerControlApi : IScriptApi, IPlugin, IPluginShutdown, IUserAgentServicePlugin, IInventoryServicePlugin, IAssetServicePlugin
    {
        static readonly ILog m_Log = LogManager.GetLogger("VIEWER CONTROL");
        public const string ExtensionName = "ViewerControl";

        public class ViewerConnection
        {
            public readonly RwLockedDictionary<uint, ViewerCircuit> ViewerCircuits = new RwLockedDictionary<uint, ViewerCircuit>();
            public readonly UDPCircuitsManager ClientUDP;
            private readonly SceneList m_Scenes;
            public readonly UUID AgentID;
            private readonly UUID m_SceneID;
            private readonly UUID m_PartID;
            private readonly UUID m_ItemID;

            public ViewerConnection(SceneList scenes, UUID agentID, UUID sceneID, UUID partID, UUID itemID)
            {
                m_Scenes = scenes;
                AgentID = agentID;
                m_SceneID = sceneID;
                m_PartID = partID;
                m_ItemID = itemID;

                ClientUDP = new UDPCircuitsManager(new System.Net.IPAddress(0), 0, null, null, null, new List<IPortControlServiceInterface>());
                ClientUDP.Start();
            }

            public void Shutdown()
            {
                foreach (ViewerCircuit circuit in ViewerCircuits.Values)
                {
                    ClientUDP.RemoveCircuit(circuit);
                    circuit.Stop();
                }
                ClientUDP.Shutdown();
            }

            public void PostEvent(IScriptEvent ev)
            {
                SceneInterface scene;
                ObjectPart part;
                ObjectPartInventoryItem item;
                ScriptInstance instance;
                if(m_Scenes.TryGetValue(m_SceneID, out scene) &&
                    scene.Primitives.TryGetValue(m_PartID, out part) &&
                    part.Inventory.TryGetValue(m_ItemID, out item))
                {
                    instance = item.ScriptInstance;
                    if(null != instance)
                    {
                        instance.PostEvent(ev);
                    }
                }
            }
        }

        private ViewerConnection AddAgent(ScriptInstance instance, UUID agentId)
        {
            ViewerConnection vc;
            ObjectPart part = instance.Part;
            SceneInterface scene = part.ObjectGroup.Scene;
            if(!m_Clients.TryGetValue(agentId, out vc))
            {
                vc = new ViewerConnection(m_Scenes, agentId, scene.ID, part.ID, instance.Item.ID);
                m_Clients.Add(agentId, vc);
            }
            return vc;
        }

        private RwLockedDictionary<UUID, ViewerConnection> m_Clients = new RwLockedDictionary<UUID, ViewerConnection>();

        private readonly string m_AgentInventoryServiceName;
        private readonly string m_AgentAssetServiceName;
        private readonly string m_AgentProfileServiceName;
        private readonly string m_AgentFriendsServiceName;
        private readonly string m_UserSessionServiceName;
        private readonly string m_GridServiceName;
        private readonly string m_OfflineIMServiceName;
        private readonly string m_UserAccountServiceName;
        private readonly string m_AgentExperienceServiceName;
        private readonly string m_AgentGroupsServiceName;
        private readonly bool m_EnableServicePlugins;

        private InventoryServiceInterface m_AgentInventoryService;
        private AssetServiceInterface m_AgentAssetService;
        private ProfileServiceInterface m_AgentProfileService;
        private FriendsServiceInterface m_AgentFriendsService;
        private UserAgentServiceInterface m_AgentUserAgentService;
        private UserSessionServiceInterface m_UserSessionService;
        private GridServiceInterface m_GridService;
        private OfflineIMServiceInterface m_OfflineIMService;
        private UserAccountServiceInterface m_UserAccountService;
        private ExperienceServiceInterface m_AgentExperienceService;
        private GroupsServiceInterface m_AgentGroupsService;
        private SceneList m_Scenes;
        private CommandRegistry m_Commands;
        private CapsHttpRedirector m_CapsRedirector;
        private List<IProtocolExtender> m_PacketHandlerPlugins = new List<IProtocolExtender>();
        private readonly bool m_RequiresInventoryIDAsIMSessionID;

        public ShutdownOrder ShutdownOrder => ShutdownOrder.BeforeLogoutAgents;

        public string Name => "ViewerControl";

        UserAgentServiceInterface IUserAgentServicePlugin.Instantiate(string url) => m_AgentUserAgentService;
        InventoryServiceInterface IInventoryServicePlugin.Instantiate(string url) => m_AgentInventoryService;
        AssetServiceInterface IAssetServicePlugin.Instantiate(string url) => m_AgentAssetService;
        public bool IsProtocolSupported(string url) => m_EnableServicePlugins;
        public bool IsProtocolSupported(string url, Dictionary<string, string> cachedheaders) => m_EnableServicePlugins;

        public void Shutdown()
        {
            foreach (ViewerConnection circuitmgr in m_Clients.Values)
            {
                circuitmgr.Shutdown();
            }
        }

        public ViewerControlApi(IConfig ownSection)
        {
            m_RequiresInventoryIDAsIMSessionID = ownSection.GetBoolean("RequiresInventoryIDAsIMSessionID", false);
            m_AgentInventoryServiceName = ownSection.GetString("InventoryService");
            m_AgentAssetServiceName = ownSection.GetString("AssetService");
            m_AgentProfileServiceName = ownSection.GetString("ProfileService", string.Empty);
            m_AgentFriendsServiceName = ownSection.GetString("FriendsService");
            m_UserSessionServiceName = ownSection.GetString("UserSessionService");
            m_GridServiceName = ownSection.GetString("GridService");
            m_OfflineIMServiceName = ownSection.GetString("OfflineIMService", string.Empty);
            m_UserAccountServiceName = ownSection.GetString("UserAccountService");
            m_AgentExperienceServiceName = ownSection.GetString("ExperienceService", string.Empty);
            m_AgentGroupsServiceName = ownSection.GetString("GroupsService", string.Empty);
            m_EnableServicePlugins = ownSection.GetBoolean("EnableServicePlugins", true);
        }

        private sealed class LocalUserIMService : IMServiceInterface
        {
            public override void Send(GridInstantMessage im)
            {
                /* intentionally left empty */
            }
        }

        private sealed class LocalUserAgentService : UserAgentServiceInterface, IDisplayNameAccessor
        {
            private readonly UserSessionServiceInterface m_UserSessionService;
            private readonly UserAccountServiceInterface m_UserAccountService;
            private readonly string m_BaseURI;

            public LocalUserAgentService(
                UserSessionServiceInterface userSessionService, 
                UserAccountServiceInterface userAccountService,
                bool requiresInventoryIDAsIMSessionID,
                string baseuri)
            {
                m_BaseURI = baseuri;
                m_UserSessionService = userSessionService;
                m_UserAccountService = userAccountService;
                RequiresInventoryIDAsIMSessionID = requiresInventoryIDAsIMSessionID;
            }

            bool IDisplayNameAccessor.TryGetValue(UGUI agent, out string displayname)
            {
                displayname = string.Empty;
                return false;
            }

            bool IDisplayNameAccessor.ContainsKey(UGUI agent) => false;

            public override bool RequiresInventoryIDAsIMSessionID { get; }

            string IDisplayNameAccessor.this[UGUI agent]
            {
                get
                {
                    throw new KeyNotFoundException();
                }

                set
                {
                    throw new NotSupportedException();
                }
            }


            public override IDisplayNameAccessor DisplayName
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override DestinationInfo GetHomeRegion(UGUI user)
            {
                throw new NotImplementedException();
            }

            public override void SetHomeRegion(UGUI user, UserRegionData info)
            {
                throw new NotImplementedException();
            }

            public override ServerURIs GetServerURLs(UGUI user) => new ServerURIs(new Dictionary<string, string>
            {
                ["AssetServerURI"] = m_BaseURI,
                ["InventoryServerURI"] = m_BaseURI,
                ["IMServerURI"] = m_BaseURI,
                ["HomeURI"] = m_BaseURI
            });

            public override UserInfo GetUserInfo(UGUI user)
            {
                UserAccount account;
                if (!m_UserAccountService.TryGetValue(user.ID, out account))
                {
                    throw new KeyNotFoundException();
                }
                return new UserInfo
                {
                    FirstName = account.Principal.FirstName,
                    LastName = account.Principal.LastName,
                    UserCreated = account.Created,
                    UserFlags = account.UserFlags,
                    UserTitle = account.UserTitle
                };
            }

            public override UGUIWithName GetUUI(UGUI user, UGUI targetUserID)
            {
                UserAccount account;
                if(!m_UserAccountService.TryGetValue(targetUserID.ID, out account))
                {
                    throw new KeyNotFoundException();
                }
                return account.Principal;
            }

            public override bool IsOnline(UGUI user)
            {
                return m_UserSessionService[user].Count != 0;
            }

            public override string LocateUser(UGUI user)
            {
                throw new KeyNotFoundException();
            }

            public override List<UUID> NotifyStatus(List<KeyValuePair<UGUI, string>> friends, UGUI user, bool online)
            {
                return new List<UUID>();
            }

            private static readonly LocalUserIMService m_IMService = new LocalUserIMService();

            public override IMServiceInterface GetIMService(UUID agentid) => m_IMService;
        }

        private string HomeURI;

        public void Startup(ConfigurationLoader loader)
        {
            HomeURI = loader.HomeURI;

            m_AgentInventoryService = loader.GetService<InventoryServiceInterface>(m_AgentInventoryServiceName);
            m_AgentAssetService = loader.GetService<AssetServiceInterface>(m_AgentAssetServiceName);
            if (m_AgentProfileServiceName?.Length != 0)
            {
                m_AgentProfileService = loader.GetService<ProfileServiceInterface>(m_AgentProfileServiceName);
            }
            m_AgentFriendsService = loader.GetService<FriendsServiceInterface>(m_AgentFriendsServiceName);
            m_UserSessionService = loader.GetService<UserSessionServiceInterface>(m_UserSessionServiceName);
            m_GridService = loader.GetService<GridServiceInterface>(m_GridServiceName);
            if (m_OfflineIMServiceName?.Length != 0)
            {
                m_OfflineIMService = loader.GetService<OfflineIMServiceInterface>(m_OfflineIMServiceName);
            }
            if(m_AgentExperienceServiceName?.Length != 0)
            {
                loader.GetService(m_AgentExperienceServiceName, out m_AgentExperienceService);
            }
            if (m_AgentGroupsServiceName?.Length != 0)
            {
                loader.GetService(m_AgentGroupsServiceName, out m_AgentGroupsService);
            }
            m_UserAccountService = loader.GetService<UserAccountServiceInterface>(m_UserAccountServiceName);
            m_AgentUserAgentService = new LocalUserAgentService(m_UserSessionService, m_UserAccountService, m_RequiresInventoryIDAsIMSessionID, HomeURI);

            m_Scenes = loader.Scenes;
            m_Commands = loader.CommandRegistry;
            m_CapsRedirector = loader.CapsRedirector;
            m_PacketHandlerPlugins = loader.GetServicesByValue<IProtocolExtender>();
        }
    }
}

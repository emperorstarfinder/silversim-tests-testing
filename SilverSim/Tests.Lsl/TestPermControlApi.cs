using Nini.Config;
using SilverSim.Main.Common;
using SilverSim.Scene.Types.Object;
using SilverSim.Scene.Types.Script;
using SilverSim.Scripting.Lsl;
using SilverSim.Types;
using SilverSim.Types.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilverSim.Tests.Lsl
{
    [LSLImplementation]
    [ScriptApiName("PermControlExtensions")]
    [PluginName("PermControlExtensions")]
    public sealed class TestPermControlApi : IScriptApi, IPlugin
    {
        public UGUI m_PermGranter;

        public TestPermControlApi(IConfig ownConfig)
        {
            m_PermGranter = new UGUI(ownConfig.GetString("PermGranter"));
        }

        public void Startup(ConfigurationLoader loader)
        {
            /* intentionally left empty */
        }

        [APIExtension("PermGranter", "_test_GrantScriptPerm")]
        public void GrantScriptPerm(ScriptInstance instance, int perms)
        {
            lock(instance)
            {
                instance.Item.PermsGranter = new ObjectPartInventoryItem.PermsGranterInfo
                {
                    PermsGranter = m_PermGranter,
                    PermsMask = (ScriptPermissions)perms
                };
            }
        }
    }
}

// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using Nini.Config;
using SilverSim.Main.Common;
using SilverSim.ServiceInterfaces.Neighbor;
using SilverSim.Types.Grid;

namespace SilverSim.Tests.Extensions
{
    public class NeighborDummyService : NeighborServiceInterface, IPlugin
    {
        public NeighborDummyService()
        {

        }

        public void Startup(ConfigurationLoader loader)
        {
        }

        public override void NotifyNeighborStatus(RegionInfo fromRegion)
        {
        }
    }

    [PluginName("DummyNeighbor")]
    public class NeighborDummyServiceFactory : IPluginFactory
    {
        public NeighborDummyServiceFactory()
        {

        }

        public IPlugin Initialize(ConfigurationLoader loader, IConfig ownSection)
        {
            return new NeighborDummyService();
        }
    }
}

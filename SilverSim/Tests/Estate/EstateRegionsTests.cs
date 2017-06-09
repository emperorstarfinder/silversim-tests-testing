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
using SilverSim.ServiceInterfaces.Estate;
using SilverSim.Tests.Extensions;
using SilverSim.Types;
using SilverSim.Types.Estate;
using System.Collections.Generic;
using System.Reflection;

namespace SilverSim.Tests.Estate
{
    public sealed class EstateRegionsTests : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        EstateServiceInterface m_EstateService;
        UUI m_EstateOwner1;
        UUI m_EstateOwner2;
        UUID m_EstateRegion1;
        UUID m_EstateRegion2;

        public void Startup(ConfigurationLoader loader)
        {
            IConfig config = loader.Config.Configs[GetType().FullName];
            m_EstateService = loader.GetService<EstateServiceInterface>(config.GetString("EstateService"));
            m_EstateOwner1 = new UUI(config.GetString("EstateOwner1"));
            m_EstateOwner2 = new UUI(config.GetString("EstateOwner2"));
            m_EstateRegion1 = new UUID(config.GetString("EstateRegion1"));
            m_EstateRegion2 = new UUID(config.GetString("EstateRegion2"));
        }

        public void Setup()
        {

        }

        public void Cleanup()
        {

        }

        public bool Run()
        {
            m_Log.Info("Creating estates");
            EstateInfo info1 = new EstateInfo()
            {
                Name = "Test Estate 1",
                ID = 101,
                Owner = m_EstateOwner1
            };
            m_EstateService.Add(info1);

            EstateInfo info2 = new EstateInfo()
            {
                Name = "Test Estate 2",
                ID = 102,
                Owner = m_EstateOwner2
            };
            m_EstateService.Add(info2);

            m_Log.Info("Adding regions");
            m_EstateService.RegionMap[m_EstateRegion1] = 101;
            m_EstateService.RegionMap[m_EstateRegion2] = 102;

            m_Log.Info("Test that estate 101 has region 1 on list");
            List<UUID> infoList1 = m_EstateService.RegionMap[101];
            if (infoList1.Count != 1 || !infoList1.Contains(m_EstateRegion1))
            {
                return false;
            }

            m_Log.Info("Test that estate 101 has region 1 on explicit");
            if (m_EstateService.RegionMap[m_EstateRegion1] != 101)
            {
                return false;
            }

            m_Log.Info("Test that estate 102 has region 2");
            List<UUID> infoList2 = m_EstateService.RegionMap[102];
            if (infoList2.Count != 1 || !infoList2.Contains(m_EstateRegion2))
            {
                return false;
            }

            m_Log.Info("Test that estate 102 has region 2 on explicit");
            if (m_EstateService.RegionMap[m_EstateRegion2] != 102)
            {
                return false;
            }

            m_Log.Info("exchange regions between estates");
            m_EstateService.RegionMap[m_EstateRegion1] = 102;
            m_EstateService.RegionMap[m_EstateRegion2] = 101;

            m_Log.Info("Test that estate 101 has region 2 on list");
            infoList1 = m_EstateService.RegionMap[101];
            if (infoList1.Count != 1 || !infoList1.Contains(m_EstateRegion2))
            {
                return false;
            }

            m_Log.Info("Test that estate 101 has region 2 on explicit");
            if (m_EstateService.RegionMap[m_EstateRegion2] != 101)
            {
                return false;
            }

            m_Log.Info("Test that estate 102 has region 1");
            infoList2 = m_EstateService.RegionMap[102];
            if (infoList2.Count != 1 || !infoList2.Contains(m_EstateRegion1))
            {
                return false;
            }

            m_Log.Info("Test that estate 102 has region 1 on explicit");
            if (m_EstateService.RegionMap[m_EstateRegion2] != 101)
            {
                return false;
            }

            m_Log.Info("Testing deletion");
            if (!m_EstateService.Remove(info1.ID))
            {
                return false;
            }
            if (!m_EstateService.Remove(info2.ID))
            {
                return false;
            }
            return true;
        }
    }
}

using log4net;
using Nini.Config;
using SilverSim.Main.Common;
using SilverSim.ServiceInterfaces.Estate;
using SilverSim.Tests.Extensions;
using SilverSim.Types;
using SilverSim.Types.Estate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SilverSim.Tests.Estate
{
    public sealed class EstateOwnerTests : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        EstateServiceInterface m_EstateService;
        UUI m_EstateOwner1;
        UUI m_EstateOwner2;

        public void Startup(ConfigurationLoader loader)
        {
            IConfig config = loader.Config.Configs[GetType().FullName];
            m_EstateService = loader.GetService<EstateServiceInterface>(config.GetString("EstateService"));
            m_EstateOwner1 = new UUI(config.GetString("EstateOwner1"));
            m_EstateOwner2 = new UUI(config.GetString("EstateOwner2"));
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

            m_Log.Info("Test that owner 1 has estate 101 on list");
            List<uint> infoList1 = m_EstateService.EstateOwner[m_EstateOwner1];
            if (infoList1.Count != 1 || !infoList1.Contains(101))
            {
                return false;
            }

            m_Log.Info("Test that owner 1 has estate 101 on explicit");
            if (!m_EstateService.EstateOwner[info1.ID].EqualsGrid(m_EstateOwner1))
            {
                return false;
            }

            m_Log.Info("Test that owner 2 has estate 102");
            List<uint> infoList2 = m_EstateService.EstateOwner[m_EstateOwner2];
            if (infoList2.Count != 1 || !infoList2.Contains(102))
            {
                return false;
            }

            m_Log.Info("Test that owner 2 has estate 102 on explicit");
            if (!m_EstateService.EstateOwner[info2.ID].EqualsGrid(m_EstateOwner2))
            {
                return false;
            }

            m_Log.Info("exchange estates between owners");
            m_EstateService.EstateOwner[101] = m_EstateOwner2;
            m_EstateService.EstateOwner[102] = m_EstateOwner1;

            m_Log.Info("check that owner changes on EstateInfo");
            if(!m_EstateService[101].Owner.EqualsGrid(m_EstateOwner2))
            {
                return false;
            }
            if (!m_EstateService[102].Owner.EqualsGrid(m_EstateOwner1))
            {
                return false;
            }

            m_Log.Info("Test that owner 1 has estate 102 on list");
            infoList1 = m_EstateService.EstateOwner[m_EstateOwner1];
            if (infoList1.Count != 1 || !infoList1.Contains(102))
            {
                return false;
            }

            m_Log.Info("Test that owner 1 has estate 102 on explicit");
            if (!m_EstateService.EstateOwner[info2.ID].EqualsGrid(m_EstateOwner1))
            {
                return false;
            }

            m_Log.Info("Test that owner 2 has estate 101");
            infoList2 = m_EstateService.EstateOwner[m_EstateOwner2];
            if (infoList2.Count != 1 || !infoList2.Contains(101))
            {
                return false;
            }

            m_Log.Info("Test that owner 2 has estate 101 on explicit");
            if (!m_EstateService.EstateOwner[info1.ID].EqualsGrid(m_EstateOwner2))
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

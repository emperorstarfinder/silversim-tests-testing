using SilverSim.Tests.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SilverSim.Main.Common;
using SilverSim.Scene.Types.Object;
using log4net;
using System.Reflection;
using SilverSim.Types;

namespace SilverSim.Tests.SimulationData.Consistency
{
    public sealed class ObjectPartInventoryRefTest : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Startup(ConfigurationLoader loader)
        {
            /* intentionally left empty */
        }

        public void Setup()
        {
            /* intentionally left empty */
        }

        public bool Run()
        {
            m_Log.Info("Create test object");
            var grp = new ObjectGroup();
            var part = new ObjectPart();
            grp.Add(1, part.ID, part);
            var item = new ObjectPartInventoryItem
            {
                Name = "Test Item"
            };
            if(item.ParentFolderID != UUID.Zero)
            {
                m_Log.Info("Test item cannot have valid PartID");
                return false;
            }
            part.Inventory.Add(item.ID, item.Name, item);
            if(part.Inventory.PartID != part.ID)
            {
                m_Log.Error("part.Inventory.PartID != part.ParentFolderID (A)");
                return false;
            }
            if (part.Inventory.PartID != item.ParentFolderID)
            {
                m_Log.Error("part.Inventory.PartID != item.ParentFolderID (A)");
                return false;
            }
            return true;
        }

        public void Cleanup()
        {
            /* intentionally left empty */
        }
    }
}

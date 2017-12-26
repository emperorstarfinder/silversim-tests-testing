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
using SilverSim.Main.Common;
using SilverSim.Scene.Types.Object;
using SilverSim.Tests.Extensions;
using SilverSim.Types;
using System.Reflection;

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
                m_Log.Info("Test item cannot have valid PartID yet");
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

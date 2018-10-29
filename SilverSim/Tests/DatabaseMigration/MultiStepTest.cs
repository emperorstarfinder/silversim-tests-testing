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
using SilverSim.ServiceInterfaces.Database;
using SilverSim.Tests.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SilverSim.Tests.DatabaseMigration
{
    public sealed class MultiStepTest : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private DBMigratorTestInterface m_MigratorTestControl;
        private List<IDBServiceInterface> m_MigratableServices;

        public void Cleanup()
        {
            m_MigratorTestControl.StopAtMigrationRevision = uint.MaxValue;
            m_MigratorTestControl.DeleteTablesBefore = false;
        }

        public bool Run()
        {
            foreach (IDBServiceInterface service in m_MigratableServices)
            {
                uint targettingMigration = 1;
                do
                {
                    m_MigratorTestControl.DeleteTablesBefore = true;
                    m_MigratorTestControl.StopAtMigrationRevision = targettingMigration;
                    m_Log.Info($"Test creating revision at {m_MigratorTestControl.StopAtMigrationRevision}");
                    service.ProcessMigrations();
                    m_MigratorTestControl.DeleteTablesBefore = false;
                    m_MigratorTestControl.StopAtMigrationRevision = uint.MaxValue;
                    m_Log.Info("Test migration to final revision");
                    service.ProcessMigrations();
                } while (++targettingMigration < m_MigratorTestControl.MaxAvailableMigrationRevision);
            }

            return true;
        }

        public void Setup()
        {
            /* intentionally left empty */
        }

        public void Startup(ConfigurationLoader loader)
        {
            IConfig config = loader.Config.Configs[GetType().FullName];
            if (config == null)
            {
                m_MigratorTestControl = loader.GetService<DBMigratorTestInterface>("MigratorTestControl");
            }
            else
            {
                m_MigratorTestControl = loader.GetService<DBMigratorTestInterface>(config.GetString("MigratorTestControl", "MigratorTestControl"));
            }
            m_MigratableServices = loader.GetServicesByValue<IDBServiceInterface>();
        }
    }
}

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
using SilverSim.ServiceInterfaces.Maptile;
using SilverSim.Tests.Extensions;
using SilverSim.Types;
using SilverSim.Types.Maptile;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SilverSim.Tests.Maptile
{
    public sealed class ServiceTests : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private MaptileServiceInterface m_MaptileService;

        public void Startup(ConfigurationLoader loader)
        {
            IConfig config = loader.Config.Configs[GetType().FullName];
            m_MaptileService = loader.GetService<MaptileServiceInterface>(config.GetString("MaptileService"));
        }

        public void Setup()
        {

        }

        public void Cleanup()
        {

        }

        public bool Run()
        {
            var location = new GridVector(256000, 256000);
            MaptileData result;
            m_Log.Info("Testing non-existence 1");
            if(m_MaptileService.TryGetValue(UUID.Zero, location, 1, out result))
            {
                return false;
            }

            m_Log.Info("Tesing non-existence 2");
            if(m_MaptileService.GetUpdateTimes(UUID.Zero, location, location, 1).Count != 0)
            {
                return false;
            }

            var testData = new MaptileData
            {
                ContentType = "application/octet-stream",
                Data = new byte[] { 1, 2, 3, 4 },
                Location = location,
                ScopeID = UUID.Zero,
                ZoomLevel = 1,
                LastUpdate = Date.Now
            };

            m_Log.Info("Store maptile");
            m_MaptileService.Store(testData);

            m_Log.Info("Testing existence 1");
            if (!m_MaptileService.TryGetValue(UUID.Zero, location, 1, out result))
            {
                return false;
            }

            m_Log.Info("Testing equality");
            if(!IsEqual(testData, result))
            {
                return false;
            }

            m_Log.Info("Tesing existence 2");
            List<MaptileInfo> reslist;
            reslist = m_MaptileService.GetUpdateTimes(UUID.Zero, location, location, 1);
            if(reslist.Count != 1)
            {
                return false;
            }
            m_Log.Info("Testing equality");
            if (!IsEqual(testData, reslist[0]))
            {
                return false;
            }

            m_Log.Info("Remove maptile");
            m_MaptileService.Remove(UUID.Zero, location, 1);

            m_Log.Info("Testing non-existence 1");
            if (m_MaptileService.TryGetValue(UUID.Zero, location, 1, out result))
            {
                return false;
            }

            m_Log.Info("Tesing non-existence 2");
            if (m_MaptileService.GetUpdateTimes(UUID.Zero, location, location, 1).Count != 0)
            {
                return false;
            }

            return true;
        }

        private bool IsEqual(MaptileData a, MaptileData b)
        {
            var mismatches = new List<string>();

            if (a.Location != b.Location)
            {
                mismatches.Add("Location");
            }

            if (a.LastUpdate.AsULong != b.LastUpdate.AsULong)
            {
                m_Log.InfoFormat("LastUpdate mismatch {0} != {1}", a.LastUpdate.AsULong, b.LastUpdate.AsULong);
                mismatches.Add("LastUpdate");
            }

            if (a.ZoomLevel != b.ZoomLevel)
            {
                mismatches.Add("ZoomLevel");
            }

            if (a.ScopeID != b.ScopeID)
            {
                mismatches.Add("ScopeID");
            }

            if(!a.Data.SequenceEqual(b.Data))
            {
                mismatches.Add("Data");
            }

            if (mismatches.Count != 0)
            {
                m_Log.InfoFormat("Mismatches: {0}", string.Join(" ", mismatches));
            }

            return mismatches.Count == 0;
        }

        private bool IsEqual(MaptileInfo a, MaptileInfo b)
        {
            var mismatches = new List<string>();

            if(a.Location != b.Location)
            {
                mismatches.Add("Location");
            }

            if(a.LastUpdate.AsULong != b.LastUpdate.AsULong)
            {
                m_Log.InfoFormat("LastUpdate mismatch {0} != {1}", a.LastUpdate.AsULong, b.LastUpdate.AsULong);
                mismatches.Add("LastUpdate");
            }

            if(a.ZoomLevel != b.ZoomLevel)
            {
                mismatches.Add("ZoomLevel");
            }

            if(a.ScopeID != b.ScopeID)
            {
                mismatches.Add("ScopeID");
            }

            if(mismatches.Count != 0)
            {
                m_Log.InfoFormat("Mismatches: {0}", string.Join(" ", mismatches));
            }

            return mismatches.Count == 0;
        }
    }
}

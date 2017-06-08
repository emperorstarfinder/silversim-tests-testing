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
    public class EstateTests : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        EstateServiceInterface m_EstateService;
        UUI m_EstateOwner;

        public void Startup(ConfigurationLoader loader)
        {
            IConfig config = loader.Config.Configs[GetType().FullName];
            m_EstateService = loader.GetService<EstateServiceInterface>(config.GetString("EstateService"));
            m_EstateOwner = new UUI(config.GetString("EstateOwner"));
        }

        public void Setup()
        {

        }

        public void Cleanup()
        {

        }

        private bool CompareEstates(EstateInfo a, EstateInfo b)
        {
            List<string> mismatches = new List<string>();
            if(a.Name != b.Name)
            {
                mismatches.Add("Name");
            }
            if(a.ID != b.ID)
            {
                mismatches.Add("ID");
            }
            if(a.AbuseEmail != b.AbuseEmail)
            {
                mismatches.Add("AbuseEmail");
            }
            if(a.CovenantID != b.CovenantID)
            {
                mismatches.Add("CovenantID");
            }
            if(a.CovenantTimestamp.AsULong != b.CovenantTimestamp.AsULong)
            {
                mismatches.Add("CovenantTimestamp");
            }
            if(a.Flags != b.Flags)
            {
                mismatches.Add("Flags");
            }
            if(a.ParentEstateID != b.ParentEstateID)
            {
                mismatches.Add("ParentEstateID");
            }
            if(a.PricePerMeter != b.PricePerMeter)
            {
                mismatches.Add("PricePerMeter");
            }
            if(a.SunPosition != b.SunPosition)
            {
                mismatches.Add("SunPosition");
            }
            if(a.UseGlobalTime != b.UseGlobalTime)
            {
                mismatches.Add("UseGlobalTime");
            }
            if (mismatches.Count > 0)
            {
                m_Log.InfoFormat("Mismatch detected: {0}", string.Join(" ", mismatches));
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool Run()
        {
            string estateName = "Test Estate";
            uint estateId = 100;
            UUID covenantId = UUID.Random;
            m_Log.Info("Testing non-existence of estate via name");
            if(m_EstateService.ContainsKey(estateName))
            {
                return false;
            }
            m_Log.Info("Testing non-existence of estate via id");
            if (m_EstateService.ContainsKey(estateId))
            {
                return false;
            }
            m_Log.Info("Creating estate");
            EstateInfo info = new EstateInfo()
            {
                ID = estateId,
                Name = estateName,
                Owner = m_EstateOwner,
                CovenantID = covenantId,
                AbuseEmail = "abuse@example.com",
                Flags = RegionOptionFlags.AllowDirectTeleport,
                BillableFactor = 5,
                PricePerMeter = 2,
                ParentEstateID = 2
            };

            m_EstateService.Add(info);
            m_Log.Info("Testing existence of estate via name");
            if (!m_EstateService.ContainsKey(estateName))
            {
                return false;
            }
            m_Log.Info("Testing existence of estate via id");
            if (!m_EstateService.ContainsKey(estateId))
            {
                return false;
            }

            EstateInfo retrievedInfo;

            m_Log.Info("Testing retrieval via name");
            if(!m_EstateService.TryGetValue(estateName, out retrievedInfo))
            {
                return false;
            }
            if(!CompareEstates(info, retrievedInfo))
            {
                return false;
            }

            m_Log.Info("Testing retrieval via id");
            if(!m_EstateService.TryGetValue(estateId, out retrievedInfo))
            {
                return false;
            }
            if(!CompareEstates(info, retrievedInfo))
            {
                return false;
            }

            m_Log.Info("Testing update");
            info.Name = "New Test Estate";
            info.ParentEstateID = 3;
            estateName = info.Name;
            m_EstateService.Update(info);

            m_Log.Info("Testing retrieval via old name");
            if (m_EstateService.TryGetValue("Test Estate", out retrievedInfo))
            {
                return false;
            }

            m_Log.Info("Testing retrieval via name");
            if (!m_EstateService.TryGetValue(estateName, out retrievedInfo))
            {
                return false;
            }
            if (!CompareEstates(info, retrievedInfo))
            {
                return false;
            }

            m_Log.Info("Testing retrieval via id");
            if (!m_EstateService.TryGetValue(estateId, out retrievedInfo))
            {
                return false;
            }
            if (!CompareEstates(info, retrievedInfo))
            {
                return false;
            }

            m_Log.Info("Testing deletion");
            if(!m_EstateService.Remove(estateId))
            {
                return false;
            }
            m_Log.Info("Testing non-existence of estate via name");
            if (m_EstateService.ContainsKey(estateName))
            {
                return false;
            }
            m_Log.Info("Testing non-existence of estate via id");
            if (m_EstateService.ContainsKey(estateId))
            {
                return false;
            }

            return true;
        }
    }
}

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
using SilverSim.ServiceInterfaces.Presence;
using SilverSim.Tests.Extensions;
using SilverSim.Types;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SilverSim.Tests.Presence
{
    public sealed class NpcServiceTests : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private NpcPresenceServiceInterface m_PresenceService;

        public void Startup(ConfigurationLoader loader)
        {
            IConfig config = loader.Config.Configs[GetType().FullName];
            m_PresenceService = loader.GetService<NpcPresenceServiceInterface>(config.GetString("NpcPresenceService"));
        }

        public void Setup()
        {

        }

        public void Cleanup()
        {

        }

        public bool Run()
        {
            var pInfo = new NpcPresenceInfo
            {
                Npc = new UUI(UUID.Random, "Npc", "Test"),
                Owner = new UUI(UUID.Random, "Test", "User", new Uri("http://example.com/")),
                Group = new UGI(UUID.Random, "NpcGroup", new Uri("http://example.com/")),
                RegionID = UUID.Random,
                Position = new Vector3(128, 128, 23),
                LookAt = new Vector3(1, -1, 0.5),
                SittingOnObjectID = UUID.Random
            };

            m_Log.Info("---- Check precondition ----");
            if (!CheckNonExistence(pInfo))
            {
                return false;
            }

            m_Log.Info("---- Presence Login ----");
            m_PresenceService.Store(pInfo);

            m_Log.Info("---- Verify existence ----");
            if (!CheckExistence(pInfo))
            {
                return false;
            }

            UUID newRegionID = UUID.Random;
            /* ensure that we get a different id */
            while (newRegionID == pInfo.RegionID)
            {
                newRegionID = UUID.Random;
            }

            m_Log.Info("---- Verify entry to be removed ----");
            m_PresenceService.Remove(UUID.Zero, pInfo.Npc.ID);
            if (!CheckNonExistence(pInfo))
            {
                return false;
            }

            return true;
        }

        private bool IsEqual(NpcPresenceInfo p1, NpcPresenceInfo p2)
        {
            var mismatches = new List<string>();

            if (p1.Npc.ID != p2.Npc.ID || p1.Npc.FirstName != p2.Npc.FirstName || p1.Npc.LastName != p2.Npc.LastName)
            {
                mismatches.Add("Npc");
            }

            bool ownerUriEqual = p1.Owner.HomeURI?.ToString() == p2.Owner.HomeURI?.ToString();
            if(p1.Owner.ID != p2.Owner.ID || p1.Owner.FirstName != p2.Owner.FirstName || p1.Owner.LastName != p2.Owner.LastName || !ownerUriEqual)
            {
                mismatches.Add("Owner");
            }

            if(p1.Group.ID != p2.Group.ID || p1.Group.GroupName != p2.Group.GroupName || p1.Group.HomeURI.ToString() != p2.Group.HomeURI.ToString())
            {
                mismatches.Add("Group");
            }

            if(p1.Options != p2.Options)
            {
                mismatches.Add("Options");
            }

            if (p1.RegionID != p2.RegionID)
            {
                mismatches.Add("RegionID");
            }

            if (!p1.Position.ApproxEquals(p2.Position, double.Epsilon))
            {
                mismatches.Add("Position");
            }

            if (!p1.LookAt.ApproxEquals(p2.LookAt, double.Epsilon))
            {
                mismatches.Add("LookAt");
            }

            if(p1.SittingOnObjectID != p2.SittingOnObjectID)
            {
                mismatches.Add("SittingOnObjectID");
            }

            if (mismatches.Count != 0)
            {
                m_Log.InfoFormat("Mismatches: {0}", string.Join(" ", mismatches));
            }

            return mismatches.Count == 0;
        }

        private bool CheckExistence(NpcPresenceInfo pInfo)
        {
            NpcPresenceInfo result;
            List<NpcPresenceInfo> reslist;

            m_Log.Info("Check that entry exists by npcid 1");
            if(!m_PresenceService.ContainsKey(pInfo.Npc.ID))
            {
                return false;
            }

            m_Log.Info("Check that entry exists by npcid 2");
            if (!m_PresenceService.TryGetValue(pInfo.Npc.ID, out result))
            {
                return false;
            }

            m_Log.Info("Verify equality");
            if (!IsEqual(result, pInfo))
            {
                return false;
            }

            m_Log.Info("Check that entry exists by regionid");
            reslist = m_PresenceService[pInfo.RegionID];
            if (reslist.Count != 1)
            {
                return false;
            }

            m_Log.Info("Verify equality");
            if (!IsEqual(reslist[0], pInfo))
            {
                return false;
            }

            m_Log.Info("Check that entry exists by firstname and lastname");
            if(!m_PresenceService.TryGetValue(pInfo.RegionID, pInfo.Npc.FirstName, pInfo.Npc.LastName, out result))
            {
                return false;
            }

            m_Log.Info("Verify equality");
            if (!IsEqual(result, pInfo))
            {
                return false;
            }

            m_Log.Info("Check that entry does not exist by firstname and lastname and other regionid");
            if (m_PresenceService.TryGetValue(UUID.RandomFixedFirst(0xFFFFFFFF), pInfo.Npc.FirstName, pInfo.Npc.LastName, out result))
            {
                return false;
            }

            return true;
        }

        private bool CheckNonExistence(NpcPresenceInfo pInfo)
        {
            NpcPresenceInfo result;

            m_Log.Info("Check that entry does not exist by userid 1");
            if (m_PresenceService.ContainsKey(pInfo.Npc.ID))
            {
                return false;
            }

            m_Log.Info("Check that entry does not exist by userid 2");
            if(m_PresenceService.TryGetValue(pInfo.Npc.ID, out result))
            {
                return false;
            }

            m_Log.Info("Check that entry does not exist by regionid");
            if (m_PresenceService[pInfo.RegionID].Count != 0)
            {
                return false;
            }

            m_Log.Info("Check that entry does not exist by firstname and lastname");
            if(m_PresenceService.TryGetValue(pInfo.RegionID, pInfo.Npc.FirstName, pInfo.Npc.LastName, out result))
            {
                return false;
            }

            return true;
        }
    }
}

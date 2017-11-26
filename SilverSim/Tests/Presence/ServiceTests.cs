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
using SilverSim.Types.Presence;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SilverSim.Tests.Presence
{
    public sealed class ServiceTests : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private PresenceServiceInterface m_PresenceService;

        public void Startup(ConfigurationLoader loader)
        {
            IConfig config = loader.Config.Configs[GetType().FullName];
            m_PresenceService = loader.GetService<PresenceServiceInterface>(config.GetString("PresenceService"));
        }

        public void Setup()
        {

        }

        public void Cleanup()
        {

        }

        public bool Run()
        {
            var pInfo = new PresenceInfo
            {
                UserID = new UUI(UUID.Random, "Test", "User", new Uri("http://example.com/")),
                RegionID = UUID.Random,
                SessionID = UUID.Random,
                SecureSessionID = UUID.Random
            };

            m_Log.Info("---- Check precondition ----");
            if(!CheckNonExistence(pInfo))
            {
                return false;
            }

            m_Log.Info("---- Check that report does not create an entry ----");
            m_PresenceService.Report(pInfo);
            if (!CheckNonExistence(pInfo))
            {
                return false;
            }

            m_Log.Info("---- Check that logout does not create an entry ----");
            m_PresenceService.Logout(pInfo.SessionID, pInfo.UserID.ID);
            if (!CheckNonExistence(pInfo))
            {
                return false;
            }

            m_Log.Info("---- Check that logoutregion does not create an entry ----");
            m_PresenceService.LogoutRegion(pInfo.RegionID);
            if (!CheckNonExistence(pInfo))
            {
                return false;
            }

            m_Log.Info("---- Presence Login ----");
            m_PresenceService.Login(pInfo);

            m_Log.Info("---- Verify existence ----");
            if(CheckExistence(pInfo))
            {
                return false;
            }

            UUID newRegionID = UUID.Random;
            /* ensure that we get a different id */
            while(newRegionID == pInfo.RegionID)
            {
                newRegionID = UUID.Random;
            }

            m_Log.Info("---- Verify that new regionid on logoutregion does not remove presence ----");
            m_PresenceService.LogoutRegion(newRegionID);

            m_Log.Info("---- Verify existence ----");
            if (CheckExistence(pInfo))
            {
                return false;
            }

            m_Log.Info("---- Verify that old regionid on logoutregion removes presence ----");
            m_PresenceService.LogoutRegion(pInfo.RegionID);
            if(!CheckNonExistence(pInfo))
            {
                return false;
            }

            m_Log.Info("---- Verify that logout agent removes presence ----");
            m_PresenceService.Login(pInfo);
            if (!CheckExistence(pInfo))
            {
                return false;
            }
            m_PresenceService.Logout(pInfo.SessionID, pInfo.UserID.ID);
            if (!CheckNonExistence(pInfo))
            {
                return false;
            }

            m_Log.Info("---- Verify that report changes region ----");
            m_PresenceService.Login(pInfo);
            if (!CheckExistence(pInfo))
            {
                return false;
            }
            UUID oldRegionID = pInfo.RegionID;
            pInfo.RegionID = newRegionID;
            m_PresenceService.Report(pInfo);
            if (!CheckExistence(pInfo))
            {
                return false;
            }

            m_Log.Info("---- Verify that old regionid on logoutregion does not remove presence ----");
            m_PresenceService.LogoutRegion(oldRegionID);

            if (!CheckExistence(pInfo))
            {
                return false;
            }

            m_Log.Info("---- Verify that new regionid on logoutregion removes presence ----");
            m_PresenceService.LogoutRegion(newRegionID);

            if(!CheckNonExistence(pInfo))
            {
                return false;
            }

            return true;
        }

        private bool IsEqual(PresenceInfo p1, PresenceInfo p2)
        {
            var mismatches = new List<string>();
            bool uriEqual = p1.UserID.HomeURI?.ToString() == p2.UserID.HomeURI?.ToString();
            if(p1.UserID.ID != p2.UserID.ID || p1.UserID.FirstName != p2.UserID.LastName || p1.UserID.LastName != p2.UserID.LastName || ! uriEqual)
            {
                mismatches.Add("UserID");
            }

            if(p1.RegionID != p2.RegionID)
            {
                mismatches.Add("RegionID");
            }

            if(p1.SessionID != p2.SessionID)
            {
                mismatches.Add("SessionID");
            }

            if(p1.SecureSessionID != p2.SecureSessionID)
            {
                mismatches.Add("SecureSessionID");
            }

            if(mismatches.Count != 0)
            {
                m_Log.InfoFormat("Mismatches: {0}", string.Join(" ", mismatches));
            }

            return mismatches.Count == 0;
        }

        private bool CheckExistence(PresenceInfo pInfo)
        {
            PresenceInfo result;
            List<PresenceInfo> reslist;

            m_Log.Info("Check that entry exists by userid");
            reslist = m_PresenceService[pInfo.UserID.ID];
            if (reslist.Count != 1)
            {
                return false;
            }

            m_Log.Info("Verify equality");
            if(!IsEqual(reslist[0], pInfo))
            {
                return false;
            }

            m_Log.Info("Check that entry exists by regionid");
            reslist = m_PresenceService.GetPresencesInRegion(pInfo.RegionID);
            if (reslist.Count != 1)
            {
                return false;
            }

            m_Log.Info("Verify equality");
            if (!IsEqual(reslist[0], pInfo))
            {
                return false;
            }

            m_Log.Info("Check that entry exists by sessionid and userid");
            try
            {
                result = m_PresenceService[pInfo.SessionID, pInfo.UserID.ID];
            }
            catch
            {
                return false;
            }

            m_Log.Info("Verify equality");
            if (!IsEqual(result, pInfo))
            {
                return false;
            }

            return true;
        }

        private bool CheckNonExistence(PresenceInfo pInfo)
        {
            PresenceInfo result;

            m_Log.Info("Check that entry does not exist by userid");
            if (m_PresenceService[pInfo.UserID.ID].Count != 0)
            {
                return false;
            }
            m_Log.Info("Check that entry does not exist by regionid");
            if (m_PresenceService.GetPresencesInRegion(pInfo.RegionID).Count != 0)
            {
                return false;
            }

            m_Log.Info("Check that entry does not exist by sessionid and userid");
            try
            {
                result = m_PresenceService[pInfo.SessionID, pInfo.UserID.ID];
                return false;
            }
            catch (KeyNotFoundException)
            {
                /* expected exception */
            }

            return true;
        }
    }
}

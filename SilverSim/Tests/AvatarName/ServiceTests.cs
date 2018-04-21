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
using SilverSim.ServiceInterfaces.AvatarName;
using SilverSim.Tests.Extensions;
using SilverSim.Types;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SilverSim.Tests.AvatarName
{
    public sealed class ServiceTests : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        AvatarNameServiceInterface m_AvatarNameService;

        public void Startup(ConfigurationLoader loader)
        {
            IConfig config = loader.Config.Configs[GetType().FullName];
            m_AvatarNameService = loader.GetService<AvatarNameServiceInterface>(config.GetString("AvatarNameService"));
        }

        public void Setup()
        {

        }

        public void Cleanup()
        {

        }

        public bool Run()
        {
            var uui1 = new UGUIWithName(UUID.Random, "First", "User", new Uri("http://example.com/")) { IsAuthoritative = true };
            var uui2 = new UGUIWithName(UUID.Random, "Second", "User", new Uri("http://example.com/")) { IsAuthoritative = true };
            List<UGUIWithName> reslist;

            m_Log.Info("---- Step 1 ----");
            if(!CheckNonExistance(1, uui1))
            {
                return false;
            }

            if(!CheckNonExistance(2, uui2))
            {
                return false;
            }

            m_Log.Info("Testing that entries are not searchable by last name");
            reslist = m_AvatarNameService.Search(new string[] { "User" });
            if (reslist.Count != 0)
            {
                m_Log.Info("Result list is not empty");
                return false;
            }

            m_Log.Info("---- Create entry 1 ----");
            m_AvatarNameService.Store(uui1);

            if(!CheckExistance(1, uui1))
            {
                return false;
            }

            if(!CheckNonExistance(2, uui2))
            {
                return false;
            }

            m_Log.Info("Testing that one entry is searchable by last name");
            reslist = m_AvatarNameService.Search(new string[] { "User" });
            if (reslist.Count != 1)
            {
                m_Log.Info("Result list does not contain exactly one entry");
                return false;
            }

            m_Log.Info("Testing equality");
            if (!IsEqual(reslist[0], uui1))
            {
                return false;
            }

            m_Log.Info("---- Create entry 2 ----");
            m_AvatarNameService.Store(uui2);

            if (!CheckExistance(1, uui1))
            {
                return false;
            }

            if (!CheckExistance(2, uui2))
            {
                return false;
            }

            m_Log.Info("Testing that both entries is searchable by last name");
            reslist = m_AvatarNameService.Search(new string[] { "User" });
            if (reslist.Count != 2)
            {
                m_Log.Info("Result list does not contain exactly two entries");
                return false;
            }

            m_Log.Info("Testing equality");
            if (!((IsEqual(reslist[0], uui1) && IsEqual(reslist[1], uui2)) ||
                (IsEqual(reslist[0], uui2) && IsEqual(reslist[1], uui1))))
            {
                return false;
            }


            m_Log.Info("---- Remove entry 1 ----");
            m_AvatarNameService.Remove(uui1.ID);

            if (!CheckNonExistance(1, uui1))
            {
                return false;
            }

            if (!CheckExistance(2, uui2))
            {
                return false;
            }

            m_Log.Info("Testing that one entry is searchable by last name");
            reslist = m_AvatarNameService.Search(new string[] { "User" });
            if (reslist.Count != 1)
            {
                m_Log.Info("Result list does not contain exactly one entry");
                return false;
            }

            m_Log.Info("Testing equality");
            if (!IsEqual(reslist[0], uui2))
            {
                return false;
            }

            m_Log.Info("---- Remove entry 2 ----");
            m_AvatarNameService.Remove(uui2.ID);

            if (!CheckNonExistance(1, uui1))
            {
                return false;
            }

            if (!CheckNonExistance(2, uui2))
            {
                return false;
            }

            m_Log.Info("Testing that entries are not searchable by last name");
            reslist = m_AvatarNameService.Search(new string[] { "User" });
            if (reslist.Count != 0)
            {
                m_Log.Info("Result list is not empty");
                return false;
            }

            return true;
        }

        private bool CheckExistance(int number, UGUIWithName uui)
        {
            UGUIWithName result;
            m_Log.InfoFormat("Testing that entry {0} exists by name 1", number);
            if (!m_AvatarNameService.TryGetValue(uui.FirstName, uui.LastName, out result))
            {
                return false;
            }

            m_Log.Info("Testing equality");
            if (!IsEqual(result, uui))
            {
                return false;
            }

            m_Log.InfoFormat("Testing that entry {0} exists by name 2", number);
            result = m_AvatarNameService[uui.FirstName, uui.LastName];

            m_Log.Info("Testing equality");
            if (!IsEqual(result, uui))
            {
                return false;
            }

            m_Log.InfoFormat("Testing that entry {0} exists by id 1", number);
            if (!m_AvatarNameService.TryGetValue(uui.ID, out result))
            {
                return false;
            }

            m_Log.Info("Testing equality");
            if (!IsEqual(result, uui))
            {
                return false;
            }

            m_Log.InfoFormat("Testing that entry {0} exists by name 2", number);
            result = m_AvatarNameService[uui.ID];

            m_Log.Info("Testing equality");
            if (!IsEqual(result, uui))
            {
                return false;
            }

            m_Log.InfoFormat("Testing that entry {0} is searchable by full name", number);
            List<UGUIWithName> reslist = m_AvatarNameService.Search(new string[] { uui.FirstName, uui.LastName });
            if(reslist.Count != 1)
            {
                m_Log.Info("Result list is not containing exactly one entry");
                return false;
            }
            m_Log.Info("Testing equality");
            if(!IsEqual(reslist[0], uui))
            {
                return false;
            }

            m_Log.InfoFormat("Testing that entry {0} is searchable by first name", number);
            reslist = m_AvatarNameService.Search(new string[] { uui.FirstName, uui.LastName });
            if (reslist.Count != 1)
            {
                m_Log.Info("Result list is not containing exactly one entry");
                return false;
            }
            m_Log.Info("Testing equality");
            if (!IsEqual(reslist[0], uui))
            {
                return false;
            }
            return true;
        }

        private bool CheckNonExistance(int number, UGUIWithName uui)
        {
            UGUIWithName result;
            m_Log.InfoFormat("Testing that entry {0} does not exist by name 1", number);
            if (m_AvatarNameService.TryGetValue(uui.FirstName, uui.LastName, out result))
            {
                return false;
            }

            m_Log.InfoFormat("Testing that entry {0} does not exist by name 2", number);
            try
            {
                result = m_AvatarNameService[uui.FirstName, uui.LastName];
                return false;
            }
            catch (KeyNotFoundException)
            {
                /* this is the exception that should happen */
            }

            m_Log.InfoFormat("Testing that entry {0} does not exist by id 1", number);
            if (m_AvatarNameService.TryGetValue(uui.ID, out result))
            {
                return false;
            }

            m_Log.InfoFormat("Testing that entry {0} does not exist by name 2", number);
            try
            {
                result = m_AvatarNameService[uui.ID];
                return false;
            }
            catch (KeyNotFoundException)
            {
                /* this is the exception that should happen */
            }

            m_Log.InfoFormat("Testing that entry {0} is not searchable by full name", number);
            List<UGUIWithName> reslist = m_AvatarNameService.Search(new string[] { uui.FirstName, uui.LastName });
            if (reslist.Count != 0)
            {
                m_Log.Info("Result list is not empty");
                return false;
            }

            m_Log.InfoFormat("Testing that entry {0} is not searchable by first name", number);
            reslist = m_AvatarNameService.Search(new string[] { uui.FirstName });
            if (reslist.Count != 0)
            {
                m_Log.Info("Result list is not empty");
                return false;
            }

            return true;
        }

        private static bool IsEqual(UGUIWithName uui1, UGUIWithName uui2)
        {
            var mismatches = new List<string>();
            if(uui1.ID != uui2.ID)
            {
                mismatches.Add("ID");
            }
            if(uui1.FirstName != uui2.FirstName)
            {
                mismatches.Add("FirstName");
            }
            if(uui1.LastName != uui2.LastName)
            {
                mismatches.Add("LastName");
            }
            bool uriEqual = uui1.HomeURI?.ToString() == uui2.HomeURI?.ToString();
            if(!uriEqual)
            {
                mismatches.Add("HomeURI");
            }
            if(mismatches.Count != 0)
            {
                m_Log.InfoFormat("Mismatches: {0}", string.Join(" ", mismatches));
            }
            return mismatches.Count == 0;
        }
    }
}

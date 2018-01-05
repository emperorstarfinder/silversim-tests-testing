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
using SilverSim.ServiceInterfaces.MuteList;
using SilverSim.Tests.Extensions;
using SilverSim.Types;
using SilverSim.Types.MuteList;
using System.Collections.Generic;
using System.Reflection;

namespace SilverSim.Tests.MuteList
{
    public sealed class MuteListTests : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private MuteListServiceInterface m_MuteListService;

        public void Cleanup()
        {
        }

        private bool CheckEqual(MuteListEntry e1, MuteListEntry e2, bool bequiet = false)
        {
            var mismatch = new List<string>();

            if(e1.MuteName != e2.MuteName)
            {
                mismatch.Add("MuteName");
            }

            if(e1.MuteID != e2.MuteID)
            {
                mismatch.Add("MuteID");
            }

            if(e1.Type != e2.Type)
            {
                mismatch.Add("Type");
            }

            if(e1.Flags != e2.Flags)
            {
                mismatch.Add("Flags");
            }

            if(mismatch.Count > 0 && !bequiet)
            {
                m_Log.InfoFormat("Mismatches: {0}", string.Join(" ", mismatch));
            }

            return mismatch.Count == 0;
        }

        public bool Run()
        {
            var muteowner = new UUID("11111111-2222-3333-4444-112233445566");
            var mute1id = new UUID("11223344-1122-1122-1122-112233445566");
            var mute2id = new UUID("11223344-1122-1122-1122-112233445577");
            var mute1entry = new MuteListEntry
            {
                MuteName = "Mute1 name",
                MuteID = mute1id,
                Type = MuteType.ByName,
                Flags = MuteFlags.ObjectSoundsMuted
            };
            var mute2entry = new MuteListEntry
            {
                MuteName = "Mute2 name",
                MuteID = mute2id,
                Type = MuteType.ByName,
                Flags = MuteFlags.ObjectSoundsMuted
            };

            m_Log.InfoFormat("Check that mute list is empty");
            List<MuteListEntry> list = m_MuteListService.GetList(muteowner, 0);
            if(list.Count != 0)
            {
                m_Log.Error("Mute list is not empty");
                return false;
            }

            m_MuteListService.Store(muteowner, mute1entry);

            m_Log.InfoFormat("Check that mute list has 1 entry");
            list = m_MuteListService.GetList(muteowner, 0);
            if (list.Count != 1)
            {
                m_Log.Error("Mute list does not match");
                return false;
            }

            if(!CheckEqual(list[0], mute1entry))
            {
                m_Log.Error("Mute entry content does not match");
                return false;
            }


            m_MuteListService.Store(muteowner, mute2entry);

            m_Log.InfoFormat("Check that mute list has 2 entries");
            list = m_MuteListService.GetList(muteowner, 0);
            if (list.Count != 2)
            {
                m_Log.Error("Mute list does not match");
                return false;
            }

            bool found1 = false;
            bool found2 = false;
            foreach(MuteListEntry e in list)
            {
                if(CheckEqual(e, mute1entry, true))
                {
                    found1 = true;
                }
                if(CheckEqual(e, mute2entry, true))
                {
                    found2 = true;
                }
            }
            if (!found1 || !found2)
            {
                m_Log.Error("Mute entries content does not match");
                return false;
            }

            m_Log.Info("Removing second entry");
            if(!m_MuteListService.Remove(muteowner, mute2entry.MuteID, mute2entry.MuteName))
            {
                m_Log.Error("Failed to remove it");
                return false;
            }

            m_Log.InfoFormat("Check that mute list has 1 entry");
            list = m_MuteListService.GetList(muteowner, 0);
            if (list.Count != 1)
            {
                m_Log.Error("Mute list does not match");
                return false;
            }

            if (!CheckEqual(list[0], mute1entry))
            {
                m_Log.Error("Mute entry content does not match");
                return false;
            }

            m_Log.Info("Change mute");
            mute1entry.Flags |= MuteFlags.ParticlesNotMuted;
            m_MuteListService.Store(muteowner, mute1entry);

            m_Log.InfoFormat("Check that mute list has 1 entry");
            list = m_MuteListService.GetList(muteowner, 0);
            if (list.Count != 1)
            {
                m_Log.Error("Mute list does not match");
                return false;
            }

            if (!CheckEqual(list[0], mute1entry))
            {
                m_Log.Error("Mute entry content does not match");
                return false;
            }

            m_Log.Info("Removing first entry");
            if (!m_MuteListService.Remove(muteowner, mute1entry.MuteID, mute1entry.MuteName))
            {
                m_Log.Error("Failed to remove it");
                return false;
            }

            m_Log.InfoFormat("Check that mute list is empty");
            list = m_MuteListService.GetList(muteowner, 0);
            if (list.Count != 0)
            {
                m_Log.Error("Mute list is not empty");
                return false;
            }

            return true;
        }

        public void Setup()
        {
        }

        public void Startup(ConfigurationLoader loader)
        {
            IConfig config = loader.Config.Configs[GetType().FullName];
            m_MuteListService = loader.GetService<MuteListServiceInterface>(config.GetString("MuteListService", "MuteListService"));
        }
    }
}

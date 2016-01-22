// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using SilverSim.ServiceInterfaces.Estate;
using SilverSim.Threading;
using SilverSim.Types;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace SilverSim.Database.Memory.Estate
{
    public sealed class MemoryEstateManagerService : EstateManagerServiceInterface
    {
        readonly RwLockedDictionaryAutoAdd<uint, RwLockedDictionary<UUI, bool>> m_Data = new RwLockedDictionaryAutoAdd<uint, RwLockedDictionary<UUI, bool>>(delegate () { return new RwLockedDictionary<UUI, bool>(); });

        public sealed class MemoryListAccess : IListAccess
        {
            readonly RwLockedDictionaryAutoAdd<uint, RwLockedDictionary<UUI, bool>> m_Data;

            public MemoryListAccess(RwLockedDictionaryAutoAdd<uint, RwLockedDictionary<UUI, bool>> data)
            {
                m_Data = data;
            }

            public List<UUI> this[uint estateID]
            {
                get 
                {
                    RwLockedDictionary<UUI, bool> res;
                    if (m_Data.TryGetValue(estateID, out res))
                    {
                        return new List<UUI>(from uui in res.Keys where true select new UUI(uui));
                    }
                    else
                    {
                        return new List<UUI>();
                    }
                }
            }
        }

        readonly MemoryListAccess m_ListAccess;

        public MemoryEstateManagerService()
        {
            m_ListAccess = new MemoryListAccess(m_Data);
        }

        [SuppressMessage("Gendarme.Rules.Design", "AvoidMultidimensionalIndexerRule")]
        public override bool this[uint estateID, UUI agent]
        {
            get
            {
                RwLockedDictionary<UUI, bool> res;
                return m_Data.TryGetValue(estateID, out res) && res.ContainsKey(agent);
            }
            set
            {
                if (value)
                {
                    m_Data[estateID][agent] = true;
                }
                else
                {
                    m_Data[estateID].Remove(agent);
                }
            }
        }

        public override EstateManagerServiceInterface.IListAccess All
        {
            get
            {
                return m_ListAccess;
            }
        }
    }
}

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
    public sealed class MemoryEstateGroupsService : EstateGroupsServiceInterface
    {
        readonly RwLockedDictionaryAutoAdd<uint, RwLockedDictionary<UGI, bool>> m_Data = new RwLockedDictionaryAutoAdd<uint, RwLockedDictionary<UGI, bool>>(delegate () { return new RwLockedDictionary<UGI, bool>(); });
        readonly MemoryListAccess m_ListAccess;

        public sealed class MemoryListAccess : IListAccess
        {
            readonly RwLockedDictionaryAutoAdd<uint, RwLockedDictionary<UGI, bool>> m_Data;

            public MemoryListAccess(RwLockedDictionaryAutoAdd<uint, RwLockedDictionary<UGI, bool>> data)
            {
                m_Data = data;
            }

            public List<UGI> this[uint estateID]
            {
                get
                {
                    RwLockedDictionary<UGI, bool> res;
                    if (m_Data.TryGetValue(estateID, out res))
                    {
                        return new List<UGI>(from ugi in res.Keys where true select new UGI(ugi));
                    }
                    else
                    {
                        return new List<UGI>();
                    }
                }
            }
        }

        public MemoryEstateGroupsService()
        {
            m_ListAccess = new MemoryListAccess(m_Data);
        }

        [SuppressMessage("Gendarme.Rules.Design", "AvoidMultidimensionalIndexerRule")]
        public override bool this[uint estateID, UGI group]
        {
            get
            {
                RwLockedDictionary<UGI, bool> res;
                return m_Data.TryGetValue(estateID, out res) && res.ContainsKey(group);
            }
            set
            {
                if (value)
                {
                    m_Data[estateID][group] = true;
                }
                else
                {
                    m_Data[estateID].Remove(group);
                }
            }
        }

        public override IListAccess All
        {
            get 
            {
                return m_ListAccess;
            }
        }
    }
}

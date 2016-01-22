// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using SilverSim.ServiceInterfaces.Estate;
using SilverSim.Threading;
using SilverSim.Types;
using System.Collections.Generic;
using System.Linq;

namespace SilverSim.Database.Memory.Estate
{
    public sealed class MemoryEstateOwnerService : IEstateOwnerServiceInterface
    {
        readonly RwLockedDictionary<uint, UUI> m_Data = new RwLockedDictionary<uint, UUI>();

        public MemoryEstateOwnerService()
        {
        }

        public bool TryGetValue(uint estateID, out UUI uui)
        {
            if(m_Data.TryGetValue(estateID, out uui))
            {
                uui = new UUI(uui);
                return true;
            }
            uui = default(UUI);
            return false;
        }

        public List<uint> this[UUI owner]
        {
            get
            {
                return new List<uint>(from data in m_Data where data.Value.EqualsGrid(owner) select data.Key);
            }
        }

        public UUI this[uint estateID]
        {
            get
            {
                UUI uui;
                if(!TryGetValue(estateID, out uui))
                {
                    throw new KeyNotFoundException();
                }
                return uui;
            }
            set
            {
                m_Data[estateID] = new UUI(value);
            }
        }
    }
}

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

using SilverSim.ServiceInterfaces.Inventory;
using SilverSim.Types;
using SilverSim.Types.Inventory;
using SilverSim.Viewer.Messages;
using System;
using System.Collections.Generic;

namespace SilverSim.Tests.Viewer.UDP
{
    public partial class LLUDPInventoryClient : IInventoryItemServiceInterface
    {
        private void HandleUpdateInventoryItem(Message m)
        {

        }

        private void HandleUpdateCreateInventoryItem(Message m)
        {

        }

        InventoryItem IInventoryItemServiceInterface.this[UUID key] => throw new NotImplementedException();

        InventoryItem IInventoryItemServiceInterface.this[UUID principalID, UUID key] => throw new NotImplementedException();

        List<InventoryItem> IInventoryItemServiceInterface.this[UUID principalID, List<UUID> itemids] => throw new NotImplementedException();

        void IInventoryItemServiceInterface.Add(InventoryItem item)
        {
            throw new NotImplementedException();
        }

        bool IInventoryItemServiceInterface.ContainsKey(UUID key)
        {
            throw new NotImplementedException();
        }

        bool IInventoryItemServiceInterface.ContainsKey(UUID principalID, UUID key)
        {
            throw new NotImplementedException();
        }

        void IInventoryItemServiceInterface.Delete(UUID principalID, UUID id)
        {
            throw new NotImplementedException();
        }

        List<UUID> IInventoryItemServiceInterface.Delete(UUID principalID, List<UUID> ids)
        {
            throw new NotImplementedException();
        }

        void IInventoryItemServiceInterface.Move(UUID principalID, UUID id, UUID newFolder)
        {
            throw new NotImplementedException();
        }

        bool IInventoryItemServiceInterface.TryGetValue(UUID key, out InventoryItem item)
        {
            throw new NotImplementedException();
        }

        bool IInventoryItemServiceInterface.TryGetValue(UUID principalID, UUID key, out InventoryItem item)
        {
            throw new NotImplementedException();
        }

        void IInventoryItemServiceInterface.Update(InventoryItem item)
        {
            throw new NotImplementedException();
        }
    }
}

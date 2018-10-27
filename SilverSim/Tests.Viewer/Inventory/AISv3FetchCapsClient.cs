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

using SilverSim.AISv3.Client;
using SilverSim.ServiceInterfaces.Inventory;
using SilverSim.Tests.Viewer.Caps;
using SilverSim.Types;
using SilverSim.Types.Inventory;
using System.Collections.Generic;

namespace SilverSim.Tests.Viewer.Inventory
{
    public sealed class AISv3FetchCapsClient : InventoryServiceInterface
    {
        IInventoryItemServiceInterface m_FetchInventory2;
        IInventoryFolderServiceInterface m_FetchInventoryDescendents2;
        private readonly AISv3ClientConnector m_AISv3;

        public class InvAPI2Item : FetchInventory2Client
        {
            private readonly IInventoryItemServiceInterface m_UdpClient;

            internal InvAPI2Item(string uri, IInventoryItemServiceInterface udpClient)
                : base(uri)
            {
                m_UdpClient = udpClient;
            }

            public override void Add(InventoryItem item) =>
                m_UdpClient.Add(item);

            public override UUID Copy(UUID principalID, UUID id, UUID newFolder) =>
                m_UdpClient.Copy(principalID, id, newFolder);

            public override void Delete(UUID principalID, UUID id) =>
                m_UdpClient.Delete(principalID, id);

            public override List<UUID> Delete(UUID principalID, List<UUID> ids) =>
                m_UdpClient.Delete(principalID, ids);

            public override void Move(UUID principalID, UUID id, UUID newFolder) =>
                m_UdpClient.Move(principalID, id, newFolder);

            public override void Update(InventoryItem item) =>
                m_UdpClient.Update(item);
        }

        public class InvAPI2Folder : FetchInventoryDescendents2Client
        {
            private readonly IInventoryFolderServiceInterface m_UdpClient;

            public InvAPI2Folder(UUID rootfolderid, string uri, IInventoryFolderServiceInterface udpClient)
                : base(rootfolderid, uri)
            {
                m_UdpClient = udpClient;
            }

            public override void Add(InventoryFolder folder) =>
                m_UdpClient.Add(folder);

            public override void Delete(UUID principalID, UUID folderID) =>
                m_UdpClient.Delete(principalID, folderID);

            public override List<UUID> Delete(UUID principalID, List<UUID> folderIDs) =>
                m_UdpClient.Delete(principalID, folderIDs);

            public override InventoryTree Copy(UUID principalID, UUID folderID, UUID toFolderID) =>
                m_UdpClient.Copy(principalID, folderID, toFolderID);

            public override void Move(UUID principalID, UUID folderID, UUID toFolderID) =>
                m_UdpClient.Move(principalID, folderID, toFolderID);

            public override void Purge(UUID folderID) =>
                m_UdpClient.Purge(folderID);

            public override void Purge(UUID principalID, UUID folderID) =>
                m_UdpClient.Purge(principalID, folderID);

            public override void Update(InventoryFolder folder) =>
                m_UdpClient.Update(folder);
        }

        public AISv3FetchCapsClient(
            string aisUri,
            string fetchInventoryDescendentsUri,
            string fetchInventoryUri,
            UUID rootFolderID)
        {
            m_AISv3 = new AISv3ClientConnector(aisUri);
            m_FetchInventory2 = new InvAPI2Item(fetchInventoryUri, m_AISv3);
            m_FetchInventoryDescendents2 = new InvAPI2Folder(rootFolderID, fetchInventoryDescendentsUri, m_AISv3);
        }

        public override IInventoryItemServiceInterface Item => m_FetchInventory2;
        public override IInventoryFolderServiceInterface Folder => m_FetchInventoryDescendents2;

        public override void Remove(UUID accountID) =>
            m_AISv3.Remove(accountID);

        public override List<InventoryItem> GetActiveGestures(UUID principalID) =>
            m_AISv3.GetActiveGestures(principalID);
    }
}

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
using SilverSim.ServiceInterfaces.Inventory.This;
using SilverSim.Threading;
using SilverSim.Types;
using SilverSim.Types.Asset;
using SilverSim.Types.Inventory;
using SilverSim.Viewer.Messages;
using SilverSim.Viewer.Messages.Inventory;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SilverSim.Tests.Viewer.UDP
{
    public partial class LLUDPInventoryClient : IInventoryFolderServiceInterface
    {
        private readonly RwLockedDictionary<UUID, AutoResetEvent> m_WaitCreateFolder = new RwLockedDictionary<UUID, AutoResetEvent>();
        private readonly RwLockedDictionary<UUID, AutoResetEvent> m_WaitMoveFolder = new RwLockedDictionary<UUID, AutoResetEvent>();
        private readonly RwLockedDictionary<UUID, AutoResetEvent> m_WaitUpdateFolder = new RwLockedDictionary<UUID, AutoResetEvent>();

        InventoryFolder IInventoryFolderServiceThisInterface.this[UUID key]
        {
            get
            {
                InventoryFolder folder;
                if (!Folder.TryGetValue(key, out folder))
                {
                    throw new InventoryFolderNotFoundException(key);
                }
                return folder;
            }
        }

        InventoryFolder IInventoryFolderServiceThisInterface.this[UUID principalID, UUID key]
        {
            get
            {
                InventoryFolder folder;
                if (!Folder.TryGetValue(principalID, key, out folder))
                {
                    throw new InventoryFolderNotFoundException(key);
                }
                return folder;
            }
        }

        InventoryFolder IInventoryFolderServiceThisInterface.this[UUID principalID, AssetType type]
        {
            get
            {
                InventoryFolder folder;
                if (!Folder.TryGetValue(principalID, type, out folder))
                {
                    throw new InventoryFolderNotFoundException();
                }
                return folder;
            }
        }

        IInventoryFolderContentServiceInterface IInventoryFolderServiceInterface.Content => this;

        private void HandleUpdateInventoryFolder(Message m)
        {
            var res = (UpdateInventoryFolder)m;
            AutoResetEvent autoEvent;
            foreach(UpdateInventoryFolder.InventoryDataEntry e in res.InventoryData)
            {
                if(m_WaitCreateFolder.TryGetValue(e.FolderID, out autoEvent))
                {
                    autoEvent.Set();
                }
                if(m_WaitMoveFolder.TryGetValue(e.FolderID, out autoEvent))
                {
                    autoEvent.Set();
                }
                if (m_WaitUpdateFolder.TryGetValue(e.FolderID, out autoEvent))
                {
                    autoEvent.Set();
                }
            }
        }

        void IInventoryFolderServiceInterface.Add(InventoryFolder folder)
        {
            var req = new CreateInventoryFolder
            {
                AgentID = m_ViewerCircuit.AgentID,
                SessionID = m_ViewerCircuit.SessionID,
                FolderID = folder.ID,
                FolderName = folder.Name,
                DefaultType = folder.DefaultType,
                ParentFolderID = folder.ParentFolderID,
            };
            var autoEvent = new AutoResetEvent(false);
            try
            {
                m_WaitCreateFolder.Add(folder.ID, autoEvent);
                m_ViewerCircuit.SendMessage(req);
                if (!autoEvent.WaitOne(5000))
                {
                    throw new InventoryFolderNotStoredException();
                }
            }
            finally
            {
                m_WaitCreateFolder.Remove(folder.ID);
            }
        }

        bool IInventoryFolderServiceInterface.ContainsKey(UUID key)
        {
            InventoryFolder folder;
            return Folder.TryGetValue(key, out folder);
        }

        bool IInventoryFolderServiceInterface.ContainsKey(UUID principalID, UUID key)
        {
            InventoryFolder folder;
            return Folder.TryGetValue(principalID, key, out folder);
        }

        bool IInventoryFolderServiceInterface.ContainsKey(UUID principalID, AssetType type)
        {
            InventoryFolder folder;
            return Folder.TryGetValue(principalID, type, out folder);
        }

        void IInventoryFolderServiceInterface.Delete(UUID principalID, UUID folderID)
        {
            var req = new RemoveInventoryFolder
            {
                AgentID = m_ViewerCircuit.AgentID,
                SessionID = m_ViewerCircuit.SessionID,
            };
            req.FolderData.Add(folderID);
            m_ViewerCircuit.SendMessage(req);
        }

        List<UUID> IInventoryFolderServiceInterface.Delete(UUID principalID, List<UUID> folderIDs)
        {
            var req = new RemoveInventoryFolder
            {
                AgentID = m_ViewerCircuit.AgentID,
                SessionID = m_ViewerCircuit.SessionID,
            };
            req.FolderData.AddRange(folderIDs);
            m_ViewerCircuit.SendMessage(req);
            return new List<UUID>(folderIDs);
        }

        List<InventoryFolder> IInventoryFolderServiceInterface.GetFolders(UUID principalID, UUID key)
        {
            InventoryFolderContent content;
            return (Folder.Content.TryGetValue(principalID, key, out content)) ? content.Folders : new List<InventoryFolder>();
        }

        List<InventoryItem> IInventoryFolderServiceInterface.GetItems(UUID principalID, UUID key)
        {
            InventoryFolderContent content;
            return (Folder.Content.TryGetValue(principalID, key, out content)) ? content.Items : new List<InventoryItem>();
        }

        void IInventoryFolderServiceInterface.IncrementVersion(UUID principalID, UUID folderID)
        {
            /* intentionally left empty */
        }

        InventoryTree IInventoryFolderServiceInterface.Copy(UUID principalID, UUID folderID, UUID toFolderID) =>
            CopyFolder(principalID, folderID, toFolderID);

        void IInventoryFolderServiceInterface.Move(UUID principalID, UUID folderID, UUID toFolderID)
        {
            var req = new MoveInventoryFolder
            {
                AgentID = m_ViewerCircuit.AgentID,
                SessionID = m_ViewerCircuit.SessionID
            };
            req.InventoryData.Add(new MoveInventoryFolder.InventoryDataEntry
            {
                FolderID = folderID,
                ParentID = toFolderID
            });
            var autoEvent = new AutoResetEvent(false);
            try
            {
                m_WaitMoveFolder.Add(folderID, autoEvent);
                m_ViewerCircuit.SendMessage(req);
                if (!autoEvent.WaitOne(5000))
                {
                    throw new InventoryFolderNotStoredException();
                }
            }
            finally
            {
                m_WaitMoveFolder.Remove(folderID);
            }
        }

        void IInventoryFolderServiceInterface.Purge(UUID folderID)
        {
            var req = new PurgeInventoryDescendents
            {
                AgentID = m_ViewerCircuit.AgentID,
                SessionID = m_ViewerCircuit.SessionID,
                FolderID = folderID
            };
            m_ViewerCircuit.SendMessage(req);
        }

        void IInventoryFolderServiceInterface.Purge(UUID principalID, UUID folderID)
        {
            var req = new PurgeInventoryDescendents
            {
                AgentID = m_ViewerCircuit.AgentID,
                SessionID = m_ViewerCircuit.SessionID,
                FolderID = folderID
            };
            m_ViewerCircuit.SendMessage(req);
        }

        bool IInventoryFolderServiceInterface.TryGetValue(UUID key, out InventoryFolder folder) =>
            Folder.TryGetValue(m_ViewerCircuit.AgentID, key, out folder);

        bool IInventoryFolderServiceInterface.TryGetValue(UUID principalID, UUID key, out InventoryFolder folder)
        {
            throw new NotImplementedException();
        }

        bool IInventoryFolderServiceInterface.TryGetValue(UUID principalID, AssetType type, out InventoryFolder folder)
        {
            foreach(InventoryFolder f in Folder.GetFolders(m_ViewerCircuit.AgentID, m_RootFolderID))
            {
                if(f.DefaultType == type)
                {
                    folder = f;
                    return true;
                }
            }
            folder = default(InventoryFolder);
            return false;
        }

        void IInventoryFolderServiceInterface.Update(InventoryFolder folder)
        {
            var req = new UpdateInventoryFolder
            {
                AgentID = m_ViewerCircuit.AgentID,
                SessionID = m_ViewerCircuit.SessionID,
            };
            req.InventoryData.Add(new UpdateInventoryFolder.InventoryDataEntry
            {
                FolderID = folder.ID,
                Name = folder.Name,
                ParentID = folder.ParentFolderID,
                DefaultType = folder.DefaultType
            });
            var autoEvent = new AutoResetEvent(false);
            try
            {
                m_WaitUpdateFolder.Add(folder.ID, autoEvent);
                m_ViewerCircuit.SendMessage(req);
                if (!autoEvent.WaitOne(5000))
                {
                    throw new InventoryFolderNotStoredException();
                }
            }
            finally
            {
                m_WaitUpdateFolder.Remove(folder.ID);
            }
        }
    }
}

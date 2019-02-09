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
using SilverSim.ServiceInterfaces.Inventory;
using SilverSim.Tests.Extensions;
using SilverSim.Types;
using SilverSim.Types.Inventory;
using System.Collections.Generic;
using System.Reflection;

namespace SilverSim.Tests.Inventory
{
    public sealed class InventoryTransferTransactionTests : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        InventoryTransferTransactionServiceInterface m_InventoryTransferTransactionService;
        UGUI m_SrcAgent;
        UGUI m_DstAgent;

        public void Startup(ConfigurationLoader loader)
        {
            IConfig config = loader.Config.Configs[GetType().FullName];
            string inventoryTransferTransactionServiceName = config.GetString("InventoryTransferTransactionService");
            m_InventoryTransferTransactionService = loader.GetService<InventoryTransferTransactionServiceInterface>(inventoryTransferTransactionServiceName);
            m_SrcAgent = new UGUI(config.GetString("SrcAgent"));
            m_DstAgent = new UGUI(config.GetString("DstAgent"));
            m_Log.Info("Src " + m_SrcAgent.ToString());
        }

        public void Setup()
        {

        }

        public void Cleanup()
        {

        }

        private bool CheckForEquality(InventoryTransferInfo info1, InventoryTransferInfo info2)
        {
            var list = new List<string>();
            if(info1.AssetType != info2.AssetType)
            {
                list.Add("AssetType");
            }
            if (info1.InventoryID != info2.InventoryID)
            {
                list.Add("InventoryID");
            }
            if (info1.SrcTransactionID != info2.SrcTransactionID)
            {
                list.Add("SrcTransactionID");
            }
            if (info1.DstTransactionID != info2.DstTransactionID)
            {
                list.Add("DstTransactionID");
            }
            if(!info1.SrcAgent.EqualsGrid(info2.SrcAgent))
            {
                list.Add("SrcAgent");
            }
            if (!info1.DstAgent.EqualsGrid(info2.DstAgent))
            {
                list.Add("DstAgent");
            }
            if(list.Count != 0)
            {
                m_Log.ErrorFormat("Test for equality failed: {0}", string.Join(",", list));
            }
            return list.Count == 0;
        }

        public bool Run()
        {
            InventoryTransferInfo info;
            InventoryTransferInfo testInfo = new InventoryTransferInfo
            {
                AssetType = Types.Asset.AssetType.Animation,
                InventoryID = UUID.Random,
                SrcTransactionID = UUID.Random,
                DstTransactionID = UUID.Random,
                SrcAgent = m_SrcAgent,
                DstAgent = m_DstAgent
            };

            m_Log.Info("Testing non-existence 1");
            if(m_InventoryTransferTransactionService.ContainsKey(m_DstAgent.ID, testInfo.DstTransactionID))
            {
                return false;
            }
            m_Log.Info("Testing non-existence 2");
            if (m_InventoryTransferTransactionService.TryGetValue(m_DstAgent.ID, testInfo.DstTransactionID, out info))
            {
                return false;
            }
            m_Log.Info("Testing non-existence 3");
            try
            {
                info = m_InventoryTransferTransactionService[m_DstAgent.ID, testInfo.DstTransactionID];
                return false;
            }
            catch (KeyNotFoundException)
            {
            }

            m_Log.Info("Creating transaction data");
            m_InventoryTransferTransactionService.Store(testInfo);

            m_Log.Info("Testing existence 1");
            if (!m_InventoryTransferTransactionService.ContainsKey(m_DstAgent.ID, testInfo.DstTransactionID))
            {
                return false;
            }
            m_Log.Info("Testing existence 2");
            if (!m_InventoryTransferTransactionService.TryGetValue(m_DstAgent.ID, testInfo.DstTransactionID, out info))
            {
                return false;
            }
            if(!CheckForEquality(info, testInfo))
            {
                return false;
            }
            m_Log.Info("Testing existence 3");
            info = m_InventoryTransferTransactionService[m_DstAgent.ID, testInfo.DstTransactionID];
            if (!CheckForEquality(info, testInfo))
            {
                return false;
            }

            m_Log.Info("Testing non-existence 1 by SrcAgent");
            if (m_InventoryTransferTransactionService.ContainsKey(m_SrcAgent.ID, testInfo.DstTransactionID))
            {
                return false;
            }
            m_Log.Info("Testing non-existence 2 by SrcAgent");
            if (m_InventoryTransferTransactionService.TryGetValue(m_SrcAgent.ID, testInfo.DstTransactionID, out info))
            {
                return false;
            }
            m_Log.Info("Testing non-existence 3 by SrcAgent");
            try
            {
                info = m_InventoryTransferTransactionService[m_SrcAgent.ID, testInfo.DstTransactionID];
                return false;
            }
            catch (KeyNotFoundException)
            {
            }

            m_Log.Info("Testing non-existence 1 by SrcTransactionID");
            if (m_InventoryTransferTransactionService.ContainsKey(m_DstAgent.ID, testInfo.SrcTransactionID))
            {
                return false;
            }
            m_Log.Info("Testing non-existence 2 by SrcTransactionID");
            if (m_InventoryTransferTransactionService.TryGetValue(m_DstAgent.ID, testInfo.SrcTransactionID, out info))
            {
                return false;
            }
            m_Log.Info("Testing non-existence 3 by SrcTransactionID");
            try
            {
                info = m_InventoryTransferTransactionService[m_DstAgent.ID, testInfo.SrcTransactionID];
                return false;
            }
            catch (KeyNotFoundException)
            {
            }

            m_Log.Info("Testing non-existence 1 by SrcTransactionID+SrcAgent");
            if (m_InventoryTransferTransactionService.ContainsKey(m_SrcAgent.ID, testInfo.SrcTransactionID))
            {
                return false;
            }
            m_Log.Info("Testing non-existence 2 by SrcTransactionID+SrcAgent");
            if (m_InventoryTransferTransactionService.TryGetValue(m_SrcAgent.ID, testInfo.SrcTransactionID, out info))
            {
                return false;
            }
            m_Log.Info("Testing non-existence 3 by SrcTransactionID+SrcAgent");
            try
            {
                info = m_InventoryTransferTransactionService[m_SrcAgent.ID, testInfo.SrcTransactionID];
                return false;
            }
            catch (KeyNotFoundException)
            {
            }

            m_Log.Info("Removing entry");
            if(!m_InventoryTransferTransactionService.Remove(m_DstAgent.ID, testInfo.DstTransactionID))
            {
                return false;
            }

            m_Log.Info("Testing non-existence 1");
            if (m_InventoryTransferTransactionService.ContainsKey(m_DstAgent.ID, testInfo.DstTransactionID))
            {
                return false;
            }
            m_Log.Info("Testing non-existence 2");
            if (m_InventoryTransferTransactionService.TryGetValue(m_DstAgent.ID, testInfo.DstTransactionID, out info))
            {
                return false;
            }
            m_Log.Info("Testing non-existence 3");
            try
            {
                info = m_InventoryTransferTransactionService[m_DstAgent.ID, testInfo.DstTransactionID];
                return false;
            }
            catch (KeyNotFoundException)
            {
            }

            m_Log.Info("Non-existence test on remove");
            if (m_InventoryTransferTransactionService.Remove(m_DstAgent.ID, testInfo.DstTransactionID))
            {
                return false;
            }

            return true;
        }
    }
}

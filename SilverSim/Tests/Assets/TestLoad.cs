﻿// SilverSim is distributed under the terms of the
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
using SilverSim.ServiceInterfaces.Asset;
using SilverSim.Tests.Extensions;
using SilverSim.Types;
using SilverSim.Types.Asset;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SilverSim.Tests.Assets
{
    public class TestLoad : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public List<UUID> AssetIDs = new List<UUID>();

        AssetServiceInterface m_AssetService;

        public void Startup(ConfigurationLoader loader)
        {
            IConfig config = loader.Config.Configs[GetType().FullName];
            m_AssetService = loader.GetService<AssetServiceInterface>(config.GetString("AssetService"));
            foreach(string key in config.GetKeys())
            {
                if(key.StartsWith("AssetID"))
                {
                    AssetIDs.Add(config.GetString(key));
                }
            }
        }

        public void Setup()
        {

        }

        public void Cleanup()
        {

        }

        public bool Run()
        {
            bool success = true;
            foreach(UUID assetID in AssetIDs)
            {
                m_Log.InfoFormat("Testing load of AssetID {0}", assetID);
                AssetData test;
                try
                {
                    test = m_AssetService[assetID];
                }
                catch
                {
                    success = false;
                }
            }

            return success;
        }
    }
}

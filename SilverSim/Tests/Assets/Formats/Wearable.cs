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
using SilverSim.Main.Common;
using SilverSim.Tests.Extensions;
using SilverSim.Types;
using SilverSim.Types.Agent;
using SilverSim.Types.Asset;
using SilverSim.Types.Asset.Format;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SilverSim.Tests.Assets.Formats
{
    class WearableFormat : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Setup()
        {

        }

        public void Cleanup()
        {

        }

        public bool Run()
        {
            Wearable wearable;
            Wearable wearableserialized;
            AssetData assetdata;
            var theCreator = new UUI()
            {
                ID = UUID.Random,
                HomeURI = new Uri("http://example.com/"),
                FirstName = "The",
                LastName = "Creator"
            };
            wearable = new Wearable()
            {
                Type = WearableType.Shape,
                Creator = theCreator,
                Description = "Wearable Description",
                Name = "Wearable Name",
                Group = UGI.Unknown,
                LastOwner = theCreator,
                Owner = theCreator
            };
            wearable.Params.Add(1, 1);
            wearable.Params.Add(2, 2);
            wearable.Params.Add(4, 4);
            wearable.Params.Add(8, 8);
            wearable.Params.Add(16, 16);
            wearable.Textures.Add(AvatarTextureIndex.UpperShirt, UUID.Random);
            wearable.Textures.Add(AvatarTextureIndex.LowerPants, UUID.Random);
            wearable.Textures.Add(AvatarTextureIndex.EyesIris, UUID.Random);

            m_Log.Info("Testing Serialization");
            assetdata = wearable.Asset();
            wearableserialized = new Wearable(assetdata);

            if(wearableserialized.Type != wearable.Type)
            {
                m_Log.Fatal("Wearable Type not identical");
                return false;
            }
            if (wearableserialized.Creator.ID != wearable.Creator.ID)
            {
                m_Log.Fatal("Wearable CreatorID not identical");
                return false;
            }
            if (wearableserialized.Description != wearable.Description)
            {
                m_Log.Fatal("Wearable Description not identical");
                return false;
            }
            if (wearableserialized.Name != wearable.Name)
            {
                m_Log.Fatal("Wearable Name not identical");
                return false;
            }
            if (wearableserialized.Group.ID != wearable.Group.ID)
            {
                m_Log.Fatal("Wearable GroupID not identical");
                return false;
            }
            if (wearableserialized.LastOwner.ID != wearable.LastOwner.ID)
            {
                m_Log.Fatal("Wearable LastOwnerID not identical");
                return false;
            }
            if (wearableserialized.Owner.ID != wearable.Owner.ID)
            {
                m_Log.Fatal("Wearable OwnerID not identical");
                return false;
            }

            if(wearable.Params.Count != wearableserialized.Params.Count)
            {
                m_Log.Fatal("Wearable Param Count not identical");
                return false;
            }
            if (wearable.Textures.Count != wearableserialized.Textures.Count)
            {
                m_Log.Fatal("Wearable Texture Count not identical");
                return false;
            }

            foreach(KeyValuePair<uint, double> kvp in wearable.Params)
            {
                if(kvp.Value != wearableserialized.Params[kvp.Key])
                {
                    m_Log.Fatal("Wearable Param not identical");
                    return false;
                }
            }
            foreach (KeyValuePair<AvatarTextureIndex, UUID> kvp in wearable.Textures)
            {
                if (kvp.Value != wearableserialized.Textures[kvp.Key])
                {
                    m_Log.Fatal("Wearable Texture not identical");
                    return false;
                }
            }

            m_Log.Info("Testing References");
            List<UUID> refs = wearableserialized.References;
            if(refs.Count != 3)
            {
                m_Log.Fatal("Wearable reference count is not 3");
                return false;
            }

            if(!refs.Contains(wearable.Textures[AvatarTextureIndex.UpperShirt]))
            {
                m_Log.Fatal("Wearable references miss texture UpperShirt");
                return false;
            }

            if (!refs.Contains(wearable.Textures[AvatarTextureIndex.LowerPants]))
            {
                m_Log.Fatal("Wearable references miss texture LowerPants");
                return false;
            }

            if (!refs.Contains(wearable.Textures[AvatarTextureIndex.EyesIris]))
            {
                m_Log.Fatal("Wearable references miss texture EyesIris");
                return false;
            }
            return true;
        }

        public void Startup(ConfigurationLoader loader)
        {
        }
    }
}

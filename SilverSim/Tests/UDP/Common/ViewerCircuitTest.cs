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

using SilverSim.Main.Common;
using SilverSim.ServiceInterfaces.PortControl;
using SilverSim.Tests.Extensions;
using SilverSim.Viewer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SilverSim.Tests.UDP.Common
{
    public abstract class ViewerCircuitTest : ITest
    {
        protected UDPCircuitsManager ClientUDP;

        public ViewerCircuitTest()
        {

        }

        public abstract bool Run();

        public void Setup()
        {
            ClientUDP = new UDPCircuitsManager(new System.Net.IPAddress(0), 0, null, null, null, new List<IPortControlServiceInterface>());
        }

        public void Cleanup()
        {
            ClientUDP.Shutdown();
        }

        public virtual void Startup(ConfigurationLoader loader)
        {
        }
    }
}

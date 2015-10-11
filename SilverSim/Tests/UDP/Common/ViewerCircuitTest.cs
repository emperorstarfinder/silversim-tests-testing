// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using SilverSim.Main.Common;
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
            ClientUDP = new UDPCircuitsManager(new System.Net.IPAddress(0), 0, null, null, null);
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

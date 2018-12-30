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

using SilverSim.Scene.Types.Script;
using SilverSim.Scripting.Lsl;
using SilverSim.Tests.Viewer.UDP;
using SilverSim.Viewer.Messages.Parcel;

namespace SilverSim.Tests.Viewer
{
    public partial class ViewerControlApi
    {
        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendParcelInfoRequest")]
        public void SendParcelInfoRequest(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey parcelID)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit))
                {
                    viewerCircuit.SendMessage(new ParcelInfoRequest
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        ParcelID = parcelID.AsUUID
                    });
                }
            }
        }
    }
}

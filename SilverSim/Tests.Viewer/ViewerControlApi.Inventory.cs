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
using SilverSim.Scene.Types.Script;
using SilverSim.Scripting.Lsl;
using SilverSim.Scripting.Lsl.Api.Hashtable;
using SilverSim.Scripting.Lsl.Api.Properties.AgentInventory;
using SilverSim.Tests.Viewer.Inventory;
using SilverSim.Tests.Viewer.UDP;
using SilverSim.Types;

namespace SilverSim.Tests.Viewer
{
    public partial class ViewerControlApi
    {
        [APIExtension("ViewerControl")]
        public const int VC_AGENT_INVENTORY_UDP_ONLY = 0;
        [APIExtension("ViewerControl")]
        public const int VC_AGENT_INVENTORY_FETCH_CAPS = 1;
        [APIExtension("ViewerControl")]
        public const int VC_AGENT_INVENTORY_MIXED_AISV3 = 2;
        [APIExtension("ViewerControl")]
        public const int VC_AGENT_INVENTORY_FULL_AISV3 = 3;

        [APIExtension("ViewerControl", "vcGetAgentInventory")]
        public AgentInventoryApi.AgentInventory GetAgentInventory(
            ScriptInstance instance,
            LSLKey agentId,
            int circuitCode,
            LSLKey rootFolderID,
            HashtableApi.Hashtable seedResponse) =>
            GetAgentInventory(instance, agentId, circuitCode, rootFolderID, seedResponse, VC_AGENT_INVENTORY_FULL_AISV3);

        [APIExtension("ViewerControl", "vcGetAgentInventory")]
        public AgentInventoryApi.AgentInventory GetAgentInventory(
            ScriptInstance instance,
            LSLKey agentId,
            int circuitCode,
            LSLKey rootFolderID,
            HashtableApi.Hashtable seedResponse,
            int inventoryOption)
        {
            lock(instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agentId.AsUUID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)circuitCode, out viewerCircuit))
                {
                    IValue value;
                    string aisv3_agent_uri = seedResponse.TryGetValue("InventoryAPIv3", out value) ? value.ToString() : string.Empty;
                    string fetchinventory2_agent_uri = seedResponse.TryGetValue("FetchInventory2", out value) ? value.ToString() : string.Empty;
                    string fetchinventorydescendents2_agent_uri = seedResponse.TryGetValue("FetchInventoryDescendents2", out value) ? value.ToString() : string.Empty;

                    if (inventoryOption >= VC_AGENT_INVENTORY_FULL_AISV3 && !string.IsNullOrEmpty(aisv3_agent_uri))
                    {
                        return new AgentInventoryApi.AgentInventory(instance, new AISv3ClientConnector(aisv3_agent_uri), new UUI(viewerCircuit.AgentID), false);
                    }

                    if(inventoryOption >= VC_AGENT_INVENTORY_FETCH_CAPS && !string.IsNullOrEmpty(fetchinventory2_agent_uri) &&
                        !string.IsNullOrEmpty(fetchinventorydescendents2_agent_uri))
                    {
                        return new AgentInventoryApi.AgentInventory(instance, new InventoryV2Client(viewerCircuit, fetchinventorydescendents2_agent_uri, fetchinventory2_agent_uri, rootFolderID), new UUI(viewerCircuit.AgentID), false);
                    }

                    return new AgentInventoryApi.AgentInventory(instance, new LLUDPInventoryClient(viewerCircuit, rootFolderID), new UUI(viewerCircuit.AgentID), false);
                }

                return new AgentInventoryApi.AgentInventory();
            }
        }

        [APIExtension("ViewerControl", "vcGetLibraryInventory")]
        public AgentInventoryApi.AgentInventory GetLibraryInventory(
            ScriptInstance instance,
            LSLKey agentId,
            int circuitCode,
            LSLKey libraryAgentId,
            LSLKey rootFolderID,
            HashtableApi.Hashtable seedResponse) =>
            GetLibraryInventory(instance, agentId, circuitCode, libraryAgentId, rootFolderID, seedResponse, VC_AGENT_INVENTORY_FULL_AISV3);

        [APIExtension("ViewerControl", "vcGetLibraryInventory")]
        public AgentInventoryApi.AgentInventory GetLibraryInventory(
            ScriptInstance instance,
            LSLKey agentId,
            int circuitCode,
            LSLKey libraryAgentId,
            LSLKey rootFolderID,
            HashtableApi.Hashtable seedResponse,
            int inventoryOption)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agentId.AsUUID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)circuitCode, out viewerCircuit))
                {
                    IValue value;
                    string aisv3_agent_uri = seedResponse.TryGetValue("LibraryAPIv3", out value) ? value.ToString() : string.Empty;
                    string fetchlib2_agent_uri = seedResponse.TryGetValue("FetchLib2", out value) ? value.ToString() : string.Empty;
                    string fetchlibdescendents2_agent_uri = seedResponse.TryGetValue("FetchLibDescendents2", out value) ? value.ToString() : string.Empty;

                    if (inventoryOption >= VC_AGENT_INVENTORY_FULL_AISV3 && !string.IsNullOrEmpty(aisv3_agent_uri))
                    {
                        return new AgentInventoryApi.AgentInventory(instance, new AISv3ClientConnector(aisv3_agent_uri), new UUI(libraryAgentId), false);
                    }

                    if (inventoryOption >= VC_AGENT_INVENTORY_FETCH_CAPS && !string.IsNullOrEmpty(fetchlib2_agent_uri) &&
                        !string.IsNullOrEmpty(fetchlibdescendents2_agent_uri))
                    {
                        return new AgentInventoryApi.AgentInventory(instance, new InventoryV2Client(viewerCircuit, fetchlibdescendents2_agent_uri, fetchlib2_agent_uri, rootFolderID), new UUI(libraryAgentId), false);
                    }

                    return new AgentInventoryApi.AgentInventory(instance, new LLUDPInventoryClient(viewerCircuit, rootFolderID), new UUI(libraryAgentId), false);
                }

                return new AgentInventoryApi.AgentInventory();
            }
        }
    }
}

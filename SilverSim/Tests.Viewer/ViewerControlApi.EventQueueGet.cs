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

using SilverSim.Http.Client;
using SilverSim.Scene.Management.Scene;
using SilverSim.Scene.Types.Script;
using SilverSim.Scene.Types.Script.Events;
using SilverSim.Scripting.Lsl;
using SilverSim.Tests.Viewer.UDP;
using SilverSim.Types;
using SilverSim.Types.StructuredData.Llsd;
using SilverSim.Viewer.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace SilverSim.Tests.Viewer
{
    public partial class ViewerControlApi
    {
        private static EQGDecoder m_EQGDecoder = new EQGDecoder(true);

        private class EventQueueGet
        {
            private readonly UUID m_ReqID;
            private readonly string m_Url;
            private readonly int m_TimeoutMs;
            private readonly SceneList m_Scenes;
            private readonly UUID m_SceneID;
            private readonly UUID m_PartID;
            private readonly UUID m_ItemID;
            private readonly uint m_CircuitCode;
            private readonly ViewerConnection m_ViewerConnection;
            private Dictionary<MessageType, Action<Message>> m_MessageRouting;

            public EventQueueGet(
                UUID reqid,
                string url, 
                uint circuitCode, 
                int timeoutms, 
                SceneList sceneList, 
                UUID sceneID, 
                UUID partID, 
                UUID itemID, 
                ViewerConnection vc,
                Dictionary<MessageType, Action<Message>> messageRouting)
            {
                m_ReqID = reqid;
                m_Url = url;
                m_CircuitCode = circuitCode;
                m_TimeoutMs = timeoutms;
                m_Scenes = sceneList;
                m_SceneID = sceneID;
                m_PartID = partID;
                m_ItemID = itemID;
                m_ViewerConnection = vc;
                m_MessageRouting = messageRouting;
            }

            public void HandleRequest(object unused)
            {
                try
                {
                    var reqmap = new Map
                {
                    { "ack", new Undef() },
                    { "done", false }
                };
                    HttpStatusCode statusCode;

                    using (Stream s = new HttpClient.Post(m_Url, "application/llsd+xml", (Stream o) => LlsdXml.Serialize(reqmap, o))
                    {
                        TimeoutMs = m_TimeoutMs,
                        DisableExceptions = HttpClient.Request.DisableExceptionFlags.Disable5XX
                    }.ExecuteStreamRequest(out statusCode))
                    {
                        if (statusCode == HttpStatusCode.OK)
                        {
                            var resmap = (Map)LlsdXml.Deserialize(s);
                            foreach(Map evmap in ((AnArray)resmap["events"]).OfType<Map>())
                            {
                                IValue body;
                                string msgtype;
                                Func<IValue, Message> del;
                                if(evmap.TryGetValue("message", out msgtype) &&
                                    evmap.TryGetValue("body", out body) &&
                                    m_EQGDecoder.EQGDecoders.TryGetValue(msgtype, out del))
                                {
                                    Message m;
                                    try
                                    {
                                        m = del(body);
                                    }
                                    catch
                                    {
                                        continue;
                                    }

                                    Action<Message> handle;
                                    if(m_MessageRouting.TryGetValue(m.Number, out handle))
                                    {
                                        handle(m);
                                    }
                                }
                            }
                        }
                        m_Log.Info("EQG finished");
                        m_ViewerConnection.PostEvent(new EventQueueGetFinishedEvent(
                            new AgentInfo(m_ViewerConnection.AgentID, m_SceneID, m_CircuitCode),
                            m_ReqID,
                            (int)statusCode));
                    }
                }
                catch
                {
                    m_Log.Info("EQG abort");
                    m_ViewerConnection.PostEvent(new EventQueueGetFinishedEvent(
                        new AgentInfo(m_ViewerConnection.AgentID, m_SceneID, m_CircuitCode),
                        m_ReqID,
                        499));
                }
            }
        }

        [TranslatedScriptEvent("eventqueueget_finished")]
        public class EventQueueGetFinishedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public AgentInfo Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey RequestID;
            [TranslatedScriptEventParameter(2)]
            public int StatusCode;

            public EventQueueGetFinishedEvent(AgentInfo agentInfo, UUID requestID, int statusCode)
            {
                Agent = agentInfo;
                RequestID = requestID;
                StatusCode = statusCode;
            }
        }

        [APIExtension(ExtensionName, "eventqueueget_finished")]
        [StateEventDelegate]
        public delegate void EventQueueGetFinished(
            AgentInfo agent,
            LSLKey requestID,
            int httpStatusCode);

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction, "RequestEventQueueGet")]
        public LSLKey SendEventQueueGet(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            string url,
            int timeoutms)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                UUID reqID = UUID.Zero;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    reqID = UUID.Random;
                    ThreadPool.QueueUserWorkItem(new EventQueueGet(
                        reqID,
                        url,
                        viewerCircuit.CircuitCode,
                        timeoutms,
                        m_Scenes,
                        instance.Part.ObjectGroup.Scene.ID,
                        instance.Part.ID,
                        instance.Item.ID,
                        vc,
                        viewerCircuit.MessageRouting).HandleRequest);
                }
                return reqID;
            }
        }
    }
}

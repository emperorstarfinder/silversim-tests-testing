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
using SilverSim.Types;
using SilverSim.Viewer.Messages.Object;
using System;
using System.Collections.Generic;

namespace SilverSim.Tests.Viewer
{
    public partial class ViewerControlApi
    {
        [APIExtension(ExtensionName, "vcextraparams")]
        [APIDisplayName("vcextraparams")]
        [APIAccessibleMembers]
        [APIIsVariableType]
        [Serializable]
        [APICloneOnAssignment]
        public class VcExtraParams
        {
            private const ushort FlexiEP = 0x10;
            private const ushort LightEP = 0x20;
            private const ushort SculptEP = 0x30;
            private const ushort ProjectionEP = 0x40;
            private const ushort MeshEP = 0x60;
            private const ushort ExtendedMeshEP = 0x70;

            public int HasFlexible;
            public double FlexiGravity;
            public double FlexiFriction;
            public int FlexiSoftness;
            public double FlexiWind;
            public double FlexiTension;
            public Vector3 FlexiForce;

            public int HasSculpt;
            public LSLKey SculptMap = new LSLKey(UUID.Zero);
            public int SculptType;

            public int HasLight;
            public Vector3 LightColor;
            public double LightIntensity;
            public double LightRadius;
            public double LightCutoff;
            public double LightFalloff;

            public int HasProjection;
            public LSLKey ProjectionTexture = new LSLKey(UUID.Zero);
            public double ProjectionFOV;
            public double ProjectionFocus;
            public double ProjectionAmbience;

            public int MeshFlags;

            public VcExtraParams()
            {
            }

            public VcExtraParams(VcExtraParams src)
            {
                HasFlexible = src.HasFlexible;
                FlexiGravity = src.FlexiGravity;
                FlexiFriction = src.FlexiFriction;
                FlexiSoftness = src.FlexiSoftness;
                FlexiWind = src.FlexiWind;
                FlexiTension = src.FlexiTension;
                FlexiForce = src.FlexiForce;

                HasSculpt = src.HasSculpt;
                SculptMap = src.SculptMap;
                SculptType = src.SculptType;

                HasLight = src.HasLight;
                LightColor = src.LightColor;
                LightIntensity = src.LightIntensity;
                LightRadius = src.LightRadius;
                LightCutoff = src.LightCutoff;
                LightFalloff = src.LightFalloff;

                HasProjection = src.HasProjection;
                ProjectionTexture = src.ProjectionTexture;
                ProjectionFOV = src.ProjectionFOV;
                ProjectionFocus = src.ProjectionFocus;
                ProjectionAmbience = src.ProjectionAmbience;

                MeshFlags = src.MeshFlags;
            }

            public VcExtraParams(byte[] extraparams)
            {
                int offset = 0;
                if(extraparams.Length == 0)
                {
                    return;
                }
                int numparams = extraparams[offset++];
                while(numparams > 0 && offset + 6 <= extraparams.Length)
                {
                    uint ep = extraparams[offset++];
                    ep = ((uint)extraparams[offset++] << 8) | ep;
                    uint eplength = extraparams[offset + 3];
                    eplength = (eplength << 8) | extraparams[offset + 2];
                    eplength = (eplength << 8) | extraparams[offset + 1];
                    eplength = (eplength << 8) | extraparams[offset + 0];
                    offset += 4;
                    if(offset+ eplength > extraparams.Length)
                    {
                        break;
                    }
                    switch(ep)
                    {
                        case FlexiEP:
                            if(eplength < 16)
                            {
                                break;
                            }
                            HasFlexible = true.ToLSLBoolean();
                            FlexiSoftness = 0;
                            if((extraparams[1] & 0x80) != 0)
                            {
                                FlexiSoftness |= 1;
                            }
                            if((extraparams[0] & 0x80) != 0)
                            {
                                FlexiSoftness |= 2;
                            }
                            FlexiTension = (extraparams[0] & 0x7F) / 10.01f;
                            FlexiFriction = (extraparams[1] & 0x7f) / 10.01f;
                            FlexiGravity = extraparams[2] / 10.01f - 10.0f;
                            FlexiWind = extraparams[3] / 10.01f;
                            FlexiForce = new Vector3(extraparams, 4);
                            break;

                        case SculptEP:
                            if (eplength < 17)
                            {
                                break;
                            }
                            HasSculpt = true.ToLSLBoolean();
                            SculptMap = new UUID(extraparams, offset);
                            SculptType = extraparams[offset + 16];
                            break;

                        case LightEP:
                            if (eplength < 16)
                            {
                                break;
                            }
                            HasLight = true.ToLSLBoolean();
                            LightColor = new Vector3(extraparams[offset] / 255.0, extraparams[offset + 1] / 255.0, extraparams[offset + 2] / 255.0);
                            LightIntensity = extraparams[offset + 3] / 255.0;
                            LightRadius = LEBytes2Float(extraparams, offset + 4);
                            LightCutoff = LEBytes2Float(extraparams, offset + 8);
                            LightFalloff = LEBytes2Float(extraparams, offset + 12);
                            break;

                        case ProjectionEP:
                            if (eplength < 28)
                            {
                                break;
                            }
                            HasProjection = true.ToLSLBoolean();
                            ProjectionTexture = new UUID(extraparams, offset);
                            ProjectionFOV = LEBytes2Float(extraparams, 16);
                            ProjectionFocus = LEBytes2Float(extraparams, 20);
                            ProjectionAmbience = LEBytes2Float(extraparams, 24);
                            break;

                        case ExtendedMeshEP:
                            if (eplength < 4)
                            {
                                break;
                            }
                            MeshFlags = extraparams[offset + 3];
                            MeshFlags = (MeshFlags << 8) | extraparams[offset + 3];
                            MeshFlags = (MeshFlags << 8) | extraparams[offset + 3];
                            MeshFlags = (MeshFlags << 8) | extraparams[offset + 3];
                            break;
                    }

                    offset += (int)eplength;
                }
            }

            private static void Float2LEBytes(float v, byte[] b, int offset)
            {
                var i = BitConverter.GetBytes(v);
                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(i);
                }
                Buffer.BlockCopy(i, 0, b, offset, 4);
            }

            private static float LEBytes2Float(byte[] b, int offset)
            {
                if (!BitConverter.IsLittleEndian)
                {
                    var i = new byte[4];
                    Buffer.BlockCopy(b, offset, i, 0, 4);
                    Array.Reverse(i);
                    return BitConverter.ToSingle(i, 0);
                }
                else
                {
                    return BitConverter.ToSingle(b, offset);
                }
            }

            public byte[] GetFlexibleData()
            {
                byte[] updatebytes = null;
                if (HasFlexible != 0)
                {
                    updatebytes = new byte[16];
                    updatebytes[0] = (byte)((byte)((byte)(FlexiTension * 10.01f) & 0x7F) | (byte)((FlexiSoftness & 2) << 6));
                    updatebytes[1] = (byte)((byte)((byte)(FlexiFriction * 10.01f) & 0x7F) | (byte)((FlexiSoftness & 1) << 7));
                    updatebytes[2] = (byte)((FlexiGravity + 10.0f) * 10.01f);
                    updatebytes[3] = (byte)(FlexiWind * 10.01f);
                    FlexiForce.ToBytes(updatebytes, 4);
                }
                return updatebytes;
            }

            public byte[] GetSculptData()
            {
                byte[] updatebytes = null;
                if (HasSculpt != 0)
                {
                    updatebytes = new byte[17];
                    SculptMap.AsUUID.ToBytes(updatebytes, 0);
                    updatebytes[16] = (byte)SculptType;
                }
                return updatebytes;
            }

            public byte[] GetLightData()
            {
                byte[] updatebytes = null;
                if (HasLight != 0)
                {
                    updatebytes = new byte[16];
                    Color color = new Color(LightColor);
                    Buffer.BlockCopy(color.AsByte, 0, updatebytes, 0, 3);

                    updatebytes[3] = (byte)(LightIntensity * 255f);
                    Float2LEBytes((float)LightRadius, updatebytes, 4);
                    Float2LEBytes((float)LightCutoff, updatebytes, 8);
                    Float2LEBytes((float)LightFalloff, updatebytes, 12);
                }
                return updatebytes;
            }

            public byte[] GetProjectData()
            {
                byte[] updatebytes = null;
                if (HasProjection != 0)
                {
                    updatebytes = new byte[28];
                    ProjectionTexture.AsUUID.ToBytes(updatebytes, 0);
                    Float2LEBytes((float)ProjectionFOV, updatebytes, 16);
                    Float2LEBytes((float)ProjectionFocus, updatebytes, 20);
                    Float2LEBytes((float)ProjectionAmbience, updatebytes, 24);
                }
                return updatebytes;
            }

            public byte[] GetEMeshData()
            {
                byte[] updatebytes = null;
                if (MeshFlags != 0)
                {
                    updatebytes = new byte[4];
                    updatebytes[0] = (byte)(((uint)MeshFlags) & 0xFF);
                    updatebytes[1] = (byte)((((uint)MeshFlags) >> 8) & 0xFF);
                    updatebytes[2] = (byte)((((uint)MeshFlags) >> 16) & 0xFF);
                    updatebytes[3] = (byte)((((uint)MeshFlags) >> 24) & 0xFF);
                }
                return updatebytes;
            }
        }

        [APIExtension(ExtensionName, "vcextraparamsdata")]
        [APIDisplayName("vcextraparamsdata")]
        [APIAccessibleMembers]
        [APIIsVariableType]
        [Serializable]
        [APICloneOnAssignment]
        public class VcExtraParamsData
        {
            public int LocalID;
            public int Flags;
            public VcExtraParams ExtraParams = new VcExtraParams();

            public VcExtraParamsData()
            {
            }

            public VcExtraParamsData(VcExtraParamsData src)
            {
                LocalID = src.LocalID;
                Flags = src.Flags;
                ExtraParams = new VcExtraParams(src.ExtraParams);
            }
        }

        [APIExtension(ExtensionName)]
        public const int VC_EXTRA_PARAMS_DATA_SET_FLEXI_EP = 0x0001;
        [APIExtension(ExtensionName)]
        public const int VC_EXTRA_PARAMS_DATA_SET_LIGHT_EP = 0x0002;
        [APIExtension(ExtensionName)]
        public const int VC_EXTRA_PARAMS_DATA_SET_SCULPT_EP = 0x0004;
        [APIExtension(ExtensionName)]
        public const int VC_EXTRA_PARAMS_DATA_SET_PROJECTION_EP = 0x0008;
        [APIExtension(ExtensionName)]
        public const int VC_EXTRA_PARAMS_DATA_SET_EXTENDEDMESH_EP = 0x0010;

        [APIExtension(ExtensionName, "vcextraparamsdatalist")]
        [APIDisplayName("vcextraparamsdatalist")]
        [APIAccessibleMembers]
        [APICloneOnAssignment]
        [APIIsVariableType]
        [Serializable]
        public class VcExtraParamsDataList : List<VcExtraParamsData>
        {
            public int Length => Count;
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void Add(VcExtraParamsDataList list, VcExtraParamsData data)
        {
            list.Add(new VcExtraParamsData(data));
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction, "SendObjectExtraParams")]
        public void SendObjectExtraParams(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            VcExtraParamsDataList objectData)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    var m = new ObjectExtraParams
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                    };
                    foreach (VcExtraParamsData d in objectData)
                    {
                        if((d.Flags & VC_EXTRA_PARAMS_DATA_SET_FLEXI_EP) != 0)
                        {
                            byte[] data = d.ExtraParams.GetFlexibleData();
                            m.ObjectData.Add(new ObjectExtraParams.Data
                            {
                                ObjectLocalID = (uint)d.LocalID,
                                ParamType = 0x0010,
                                ParamInUse = data != null,
                                ParamSize = data != null ? (uint)data.Length : 0,
                                ParamData = data ?? new byte[0]
                            });
                        }
                        if ((d.Flags & VC_EXTRA_PARAMS_DATA_SET_LIGHT_EP) != 0)
                        {
                            byte[] data = d.ExtraParams.GetLightData();
                            m.ObjectData.Add(new ObjectExtraParams.Data
                            {
                                ObjectLocalID = (uint)d.LocalID,
                                ParamType = 0x0020,
                                ParamInUse = data != null,
                                ParamSize = data != null ? (uint)data.Length : 0,
                                ParamData = data ?? new byte[0]
                            });
                        }
                        if ((d.Flags & VC_EXTRA_PARAMS_DATA_SET_SCULPT_EP) != 0)
                        {
                            byte[] data = d.ExtraParams.GetSculptData();
                            m.ObjectData.Add(new ObjectExtraParams.Data
                            {
                                ObjectLocalID = (uint)d.LocalID,
                                ParamType = 0x0030,
                                ParamInUse = data != null,
                                ParamSize = data != null ? (uint)data.Length : 0,
                                ParamData = data ?? new byte[0]
                            });
                        }
                        if ((d.Flags & VC_EXTRA_PARAMS_DATA_SET_PROJECTION_EP) != 0)
                        {
                            byte[] data = d.ExtraParams.GetProjectData();
                            m.ObjectData.Add(new ObjectExtraParams.Data
                            {
                                ObjectLocalID = (uint)d.LocalID,
                                ParamType = 0x0040,
                                ParamInUse = data != null,
                                ParamSize = data != null ? (uint)data.Length : 0,
                                ParamData = data ?? new byte[0]
                            });
                        }
                        if ((d.Flags & VC_EXTRA_PARAMS_DATA_SET_EXTENDEDMESH_EP) != 0)
                        {
                            byte[] data = d.ExtraParams.GetEMeshData();
                            m.ObjectData.Add(new ObjectExtraParams.Data
                            {
                                ObjectLocalID = (uint)d.LocalID,
                                ParamType = 0x0070,
                                ParamInUse = data != null,
                                ParamSize = data != null ? (uint)data.Length : 0,
                                ParamData = data ?? new byte[0]
                            });
                        }
                    }
                    viewerCircuit.SendMessage(m);
                }
            }
        }
    }
}

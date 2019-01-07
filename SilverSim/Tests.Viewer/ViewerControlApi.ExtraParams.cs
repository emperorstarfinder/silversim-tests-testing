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

using SilverSim.Scripting.Lsl;
using SilverSim.Types;
using System;

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

            public bool HasFlexible;
            public double FlexiGravity;
            public double FlexiFriction;
            public int FlexiSoftness;
            public double FlexiWind;
            public double FlexiTension;
            public Vector3 FlexiForce;

            public bool HasSculpt;
            public LSLKey SculptMap;
            public int SculptType;

            public bool HasLight;
            public Vector3 LightColor;
            public double LightIntensity;
            public double LightRadius;
            public double LightCutoff;
            public double LightFalloff;

            public bool HasProjection;
            public LSLKey ProjectionTexture;
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
                            HasFlexible = true;
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
                            HasSculpt = true;
                            SculptMap = new UUID(extraparams, offset);
                            SculptType = extraparams[offset + 16];
                            break;

                        case LightEP:
                            if (eplength < 16)
                            {
                                break;
                            }
                            HasLight = true;
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
                            HasProjection = true;
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

            public byte[] GetBytes()
            {
                int extraParamsNum = 0;
                int totalBytesLength = 1;
                if(HasFlexible)
                {
                    ++extraParamsNum;
                    totalBytesLength += 16;
                    totalBytesLength += 2 + 4;
                }

                if (HasSculpt)
                {
                    ++extraParamsNum;
                    totalBytesLength += 17;
                    totalBytesLength += 2 + 4;
                }

                if (HasProjection)
                {
                    ++extraParamsNum;
                    totalBytesLength += 28;
                    totalBytesLength += 2 + 4;
                }

                if (MeshFlags != 0)
                {
                    ++extraParamsNum;
                    totalBytesLength += 4;
                    totalBytesLength += 2 + 4;
                }

                if (HasLight)
                {
                    ++extraParamsNum;
                    totalBytesLength += 16;
                    totalBytesLength += 2 + 4;
                }

                var updatebytes = new byte[totalBytesLength];
                int i = 0;
                updatebytes[i++] = (byte)extraParamsNum;

                if (HasFlexible)
                {
                    updatebytes[i++] = FlexiEP % 256;
                    updatebytes[i++] = FlexiEP / 256;

                    updatebytes[i++] = 16;
                    updatebytes[i++] = 0;
                    updatebytes[i++] = 0;
                    updatebytes[i++] = 0;

                    updatebytes[i++] = (byte)((byte)((byte)(FlexiTension * 10.01f) & 0x7F) | (byte)((FlexiSoftness & 2) << 6));
                    updatebytes[i++] = (byte)((byte)((byte)(FlexiFriction * 10.01f) & 0x7F) | (byte)((FlexiSoftness & 1) << 7));
                    updatebytes[i++] = (byte)((FlexiGravity + 10.0f) * 10.01f);
                    updatebytes[i++] = (byte)(FlexiWind * 10.01f);
                    FlexiForce.ToBytes(updatebytes, i);
                    i += 12;
                }

                if (HasSculpt)
                {
                    updatebytes[i++] = SculptEP % 256;
                    updatebytes[i++] = SculptEP / 256;
                    updatebytes[i++] = 17;
                    updatebytes[i++] = 0;
                    updatebytes[i++] = 0;
                    updatebytes[i++] = 0;
                    SculptMap.AsUUID.ToBytes(updatebytes, i);
                    i += 16;
                    updatebytes[i++] = (byte)SculptType;
                }

                if (HasLight)
                {
                    updatebytes[i++] = LightEP % 256;
                    updatebytes[i++] = LightEP / 256;
                    updatebytes[i++] = 16;
                    updatebytes[i++] = 0;
                    updatebytes[i++] = 0;
                    updatebytes[i++] = 0;
                    Color color = new Color(LightColor);
                    Buffer.BlockCopy(color.AsByte, 0, updatebytes, i, 3);

                    updatebytes[i + 3] = (byte)(LightIntensity * 255f);
                    i += 4;
                    Float2LEBytes((float)LightRadius, updatebytes, i);
                    i += 4;
                    Float2LEBytes((float)LightCutoff, updatebytes, i);
                    i += 4;
                    Float2LEBytes((float)LightFalloff, updatebytes, i);
                    i += 4;
                }

                if (HasProjection)
                {
                    /* full block */
                    updatebytes[i++] = (ProjectionEP % 256);
                    updatebytes[i++] = (ProjectionEP / 256);
                    updatebytes[i++] = 28;
                    updatebytes[i++] = 0;
                    updatebytes[i++] = 0;
                    updatebytes[i++] = 0;
                    ProjectionTexture.AsUUID.ToBytes(updatebytes, i);
                    i += 16;
                    Float2LEBytes((float)ProjectionFOV, updatebytes, i);
                    i += 4;
                    Float2LEBytes((float)ProjectionFocus, updatebytes, i);
                    i += 4;
                    Float2LEBytes((float)ProjectionAmbience, updatebytes, i);
                }

                if (MeshFlags != 0)
                {
                    /* full block */
                    updatebytes[i++] = (ExtendedMeshEP % 256);
                    updatebytes[i++] = (ExtendedMeshEP / 256);
                    updatebytes[i++] = 4;
                    updatebytes[i++] = 0;
                    updatebytes[i++] = 0;
                    updatebytes[i++] = 0;
                    updatebytes[i++] = (byte)(((uint)MeshFlags) & 0xFF);
                    updatebytes[i++] = (byte)((((uint)MeshFlags) >> 8) & 0xFF);
                    updatebytes[i++] = (byte)((((uint)MeshFlags) >> 16) & 0xFF);
                    updatebytes[i++] = (byte)((((uint)MeshFlags) >> 24) & 0xFF);
                }

                return updatebytes;
            }
        }
    }
}

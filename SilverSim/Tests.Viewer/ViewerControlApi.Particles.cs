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
using SilverSim.Scripting.Lsl.Api.ByteString;
using SilverSim.Types;
using SilverSim.Types.Primitive;
using System;

namespace SilverSim.Tests.Viewer
{
    public partial class ViewerControlApi
    {
        [APIExtension(ExtensionName, "particlesystemdata")]
        [APIDisplayName("particlesystemdata")]
        [APIIsVariableType]
        [APIAccessibleMembers]
        [ImplementsCustomTypecasts]
        public sealed class ParticleSystemContainer
        {
            private ParticleSystem m_ParticleSystem;
            private byte[] m_PSBlock;

            public int Flags
            {
                get
                {
                    return (int)m_ParticleSystem.PartFlags;
                }
                set
                {
                    m_ParticleSystem.PartFlags = (uint)Flags;
                    m_PSBlock = m_ParticleSystem.GetBytes();
                }
            }

            public int SourcePattern
            {
                get
                {
                    return (int)m_ParticleSystem.Pattern;
                }
                set
                {
                    m_ParticleSystem.Pattern = (ParticleSystem.SourcePattern)value;
                    m_PSBlock = m_ParticleSystem.GetBytes();
                }
            }

            public double MaxAge
            {
                get
                {
                    return m_ParticleSystem.MaxAge;
                }
                set
                {
                    m_ParticleSystem.MaxAge = (float)value;
                    m_PSBlock = m_ParticleSystem.GetBytes();
                }
            }

            public double InnerAngle
            {
                get
                {
                    return m_ParticleSystem.InnerAngle;
                }
                set
                {
                    m_ParticleSystem.InnerAngle = (float)value;
                    m_PSBlock = m_ParticleSystem.GetBytes();
                }
            }

            public double OuterAngle
            {
                get
                {
                    return m_ParticleSystem.OuterAngle;
                }
                set
                {
                    m_ParticleSystem.OuterAngle = (float)value;
                    m_PSBlock = m_ParticleSystem.GetBytes();
                }
            }

            public double BurstRate
            {
                get
                {
                    return m_ParticleSystem.BurstRate;
                }
                set
                {
                    m_ParticleSystem.BurstRate = (float)value;
                    m_PSBlock = m_ParticleSystem.GetBytes();
                }
            }

            public double BurstSpeedMin
            {
                get
                {
                    return m_ParticleSystem.BurstSpeedMin;
                }
                set
                {
                    m_ParticleSystem.BurstSpeedMin = (float)value;
                    m_PSBlock = m_ParticleSystem.GetBytes();
                }
            }

            public double BurstSpeedMax
            {
                get
                {
                    return m_ParticleSystem.BurstSpeedMax;
                }
                set
                {
                    m_ParticleSystem.BurstSpeedMax = (float)value;
                    m_PSBlock = m_ParticleSystem.GetBytes();
                }
            }

            public int BurstPartCount
            {
                get
                {
                    return m_ParticleSystem.BurstPartCount;
                }
                set
                {
                    m_ParticleSystem.BurstPartCount = (byte)value;
                    m_PSBlock = m_ParticleSystem.GetBytes();
                }
            }

            public Vector3 AngularVelocity
            {
                get
                {
                    return m_ParticleSystem.AngularVelocity;
                }
                set
                {
                    m_ParticleSystem.AngularVelocity = value;
                    m_PSBlock = m_ParticleSystem.GetBytes();
                }
            }

            public Vector3 PartAcceleration
            {
                get
                {
                    return m_ParticleSystem.PartAcceleration;
                }
                set
                {
                    m_ParticleSystem.PartAcceleration = value;
                    m_PSBlock = m_ParticleSystem.GetBytes();
                }
            }

            public LSLKey TextureID
            {
                get
                {
                    return m_ParticleSystem.Texture;
                }
                set
                {
                    m_ParticleSystem.Texture = value;
                    m_PSBlock = m_ParticleSystem.GetBytes();
                }
            }

            public LSLKey TargetID
            {
                get
                {
                    return m_ParticleSystem.Target;
                }
                set
                {
                    m_ParticleSystem.Target = value;
                    m_PSBlock = m_ParticleSystem.GetBytes();
                }
            }

            public int PartDataFlags
            {
                get
                {
                    return (int)m_ParticleSystem.PartDataFlags;
                }
                set
                {
                    m_ParticleSystem.PartDataFlags = (ParticleSystem.ParticleDataFlags)value;
                    m_PSBlock = m_ParticleSystem.GetBytes();
                }
            }

            public double PartMaxAge
            {
                get
                {
                    return m_ParticleSystem.PartMaxAge;
                }
                set
                {
                    m_ParticleSystem.PartMaxAge = (float)value;
                    m_PSBlock = m_ParticleSystem.GetBytes();
                }
            }

            public Vector3 PartStartColor
            {
                get
                {
                    return m_ParticleSystem.PartStartColor;
                }
                set
                {
                    m_ParticleSystem.PartStartColor.R = value.X;
                    m_ParticleSystem.PartStartColor.G = value.Y;
                    m_ParticleSystem.PartStartColor.B = value.Z;
                    m_PSBlock = m_ParticleSystem.GetBytes();
                }
            }

            public double PartStartAlpha
            {
                get
                {
                    return m_ParticleSystem.PartStartColor.A;
                }
                set
                {
                    m_ParticleSystem.PartStartColor.A = value;
                    m_PSBlock = m_ParticleSystem.GetBytes();
                }
            }

            public Vector3 PartEndColor
            {
                get
                {
                    return m_ParticleSystem.PartEndColor;
                }
                set
                {
                    m_ParticleSystem.PartEndColor.R = value.X;
                    m_ParticleSystem.PartEndColor.G = value.Y;
                    m_ParticleSystem.PartEndColor.B = value.Z;
                    m_PSBlock = m_ParticleSystem.GetBytes();
                }
            }

            public double PartEndAlpha
            {
                get
                {
                    return m_ParticleSystem.PartEndColor.A;
                }
                set
                {
                    m_ParticleSystem.PartEndColor.A = value;
                    m_PSBlock = m_ParticleSystem.GetBytes();
                }
            }

            public Vector3 PartStartScale
            {
                get
                {
                    return new Vector3(m_ParticleSystem.PartStartScaleX, m_ParticleSystem.PartStartScaleY, 0);
                }
                set
                {
                    m_ParticleSystem.PartStartScaleX = (float)value.X;
                    m_ParticleSystem.PartStartScaleY = (float)value.Y;
                    m_PSBlock = m_ParticleSystem.GetBytes();
                }
            }

            public Vector3 PartEndScale
            {
                get
                {
                    return new Vector3(m_ParticleSystem.PartEndScaleX, m_ParticleSystem.PartEndScaleY, 0);
                }
                set
                {
                    m_ParticleSystem.PartEndScaleX = (float)value.X;
                    m_ParticleSystem.PartEndScaleY = (float)value.Y;
                    m_PSBlock = m_ParticleSystem.GetBytes();
                }
            }

            public double PartStartGlow
            {
                get
                {
                    return m_ParticleSystem.PartStartGlow;
                }
                set
                {
                    m_ParticleSystem.PartStartGlow = (float)value;
                    m_PSBlock = m_ParticleSystem.GetBytes();
                }
            }

            public double PartEndGlow
            {
                get
                {
                    return m_ParticleSystem.PartEndGlow;
                }
                set
                {
                    m_ParticleSystem.PartEndGlow = (float)value;
                    m_PSBlock = m_ParticleSystem.GetBytes();
                }
            }

            public int BlendFuncSource
            {
                get
                {
                    return (int)m_ParticleSystem.BlendFuncSource;
                }
                set
                {
                    m_ParticleSystem.BlendFuncSource = (ParticleSystem.BlendFunc)value;
                    m_PSBlock = m_ParticleSystem.GetBytes();
                }
            }

            public int BlendFuncDest
            {
                get
                {
                    return (int)m_ParticleSystem.BlendFuncDest;
                }
                set
                {
                    m_ParticleSystem.BlendFuncDest = (ParticleSystem.BlendFunc)value;
                    m_PSBlock = m_ParticleSystem.GetBytes();
                }
            }

            public ParticleSystemContainer()
            {
                m_ParticleSystem = new ParticleSystem();
                m_PSBlock = new byte[0];
            }

            public ParticleSystemContainer(byte[] data)
            {
                m_ParticleSystem = new ParticleSystem(data, 0);
                m_PSBlock = data;
            }

            public ByteArrayApi.ByteArray PSBlock
            {
                get
                {
                    byte[] psblock = new byte[m_PSBlock.Length];
                    Buffer.BlockCopy(m_PSBlock, 0, psblock, 0, m_PSBlock.Length);
                    return new ByteArrayApi.ByteArray(psblock);
                }
                set
                {
                    var ps = new ParticleSystem(value.Data, 0);
                    byte[] psblock = new byte[value.Data.Length];
                    Buffer.BlockCopy(value.Data, 0, psblock, 0, psblock.Length);
                    m_ParticleSystem = ps;
                    m_PSBlock = psblock;
                }
            }
        }
    }
}

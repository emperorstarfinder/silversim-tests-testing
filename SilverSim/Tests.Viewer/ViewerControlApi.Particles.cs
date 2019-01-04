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

            public int Flags => (int)m_ParticleSystem.PartFlags;

            public int SourcePattern => (int)m_ParticleSystem.Pattern;

            public double MaxAge => m_ParticleSystem.MaxAge;

            public double InnerAngle => m_ParticleSystem.InnerAngle;

            public double OuterAngle => m_ParticleSystem.OuterAngle;

            public double BurstRate => m_ParticleSystem.BurstRate;

            public double BurstSpeedMin => m_ParticleSystem.BurstSpeedMin;

            public double BurstSpeedMax => m_ParticleSystem.BurstSpeedMax;

            public int BurstPartCount => m_ParticleSystem.BurstPartCount;

            public Vector3 AngularVelocity => m_ParticleSystem.AngularVelocity;

            public Vector3 PartAcceleration => m_ParticleSystem.PartAcceleration;

            public LSLKey TextureID => m_ParticleSystem.Texture;

            public LSLKey TargetID => m_ParticleSystem.Target;

            public int PartDataFlags => (int)m_ParticleSystem.PartDataFlags;

            public double PartMaxAge => m_ParticleSystem.PartMaxAge;

            public Vector3 PartStartColor => m_ParticleSystem.PartStartColor;

            public double PartStartAlpha => m_ParticleSystem.PartStartColor.A;

            public Vector3 PartEndColor => m_ParticleSystem.PartEndColor;

            public double PartEndAlpha => m_ParticleSystem.PartEndColor.A;

            public Vector3 PartStartScale => new Vector3(m_ParticleSystem.PartStartScaleX, m_ParticleSystem.PartStartScaleY, 0);

            public Vector3 PartEndScale => new Vector3(m_ParticleSystem.PartEndScaleX, m_ParticleSystem.PartEndScaleY, 0);

            public double PartStartGlow => m_ParticleSystem.PartStartGlow;

            public double PartEndGlow => m_ParticleSystem.PartEndGlow;

            public int BlendFuncSource => (int)m_ParticleSystem.BlendFuncSource;

            public int BlendFuncDest => (int)m_ParticleSystem.BlendFuncDest;

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
            }
        }
    }
}

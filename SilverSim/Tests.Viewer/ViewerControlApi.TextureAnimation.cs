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
using SilverSim.Types.Primitive;
using System;

namespace SilverSim.Tests.Viewer
{
    public partial class ViewerControlApi
    {
        [APIExtension(ExtensionName, "textureanimationentry")]
        [APIDisplayName("textureanimationentry")]
        [APIAccessibleMembers]
        [APIIsVariableType]
        public sealed class TextureAnimationEntryContainer
        {
            private TextureAnimationEntry m_TextureAnimationEntry;

            public TextureAnimationEntryContainer()
            {
                m_TextureAnimationEntry = new TextureAnimationEntry();
            }

            public TextureAnimationEntryContainer(byte[] data)
            {
                m_TextureAnimationEntry = new TextureAnimationEntry(data, 0);
            }

            public int Flags
            {
                get
                {
                    return (int)m_TextureAnimationEntry.Flags;
                }
                set
                {
                    m_TextureAnimationEntry.Flags = (TextureAnimationEntry.TextureAnimMode)value;
                }
            }

            public int Face
            {
                get
                {
                    return m_TextureAnimationEntry.Face;
                }
                set
                {
                    m_TextureAnimationEntry.Face = (sbyte)value;
                }
            }

            public int SizeX
            {
                get
                {
                    return m_TextureAnimationEntry.SizeX;
                }
                set
                {
                    m_TextureAnimationEntry.SizeX = (byte)Math.Max(1, value);
                }
            }

            public int SizeY
            {
                get
                {
                    return m_TextureAnimationEntry.SizeY;
                }
                set
                {
                    m_TextureAnimationEntry.SizeY = (byte)Math.Max(1, value);
                }
            }

            public double Start
            {
                get
                {
                    return m_TextureAnimationEntry.Start;
                }
                set
                {
                    m_TextureAnimationEntry.Start = (float)value;
                }
            }

            public double Length
            {
                get
                {
                    return m_TextureAnimationEntry.Length;
                }
                set
                {
                    m_TextureAnimationEntry.Length = (float)value;
                }
            }

            public double Rate
            {
                get
                {
                    return m_TextureAnimationEntry.Rate;
                }
                set
                {
                    m_TextureAnimationEntry.Rate = (float)value;
                }
            }

            public ByteArrayApi.ByteArray Bytes
            {
                get
                {
                    return new ByteArrayApi.ByteArray(m_TextureAnimationEntry.GetBytes());
                }
                set
                {
                    m_TextureAnimationEntry = new TextureAnimationEntry(value.Data, 0);
                }
            }
        }
    }
}

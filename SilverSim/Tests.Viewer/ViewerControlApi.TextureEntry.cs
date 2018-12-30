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
using SilverSim.Types.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilverSim.Tests.Viewer
{
    public partial class ViewerControlApi
    {
        [APIExtension("ViewerControl", "textureentry")]
        [APIDisplayName("textureentry")]
        [APIAccessibleMembers]
        public sealed class TextureEntryContainer
        {
            private TextureEntry m_TextureEntry;

            public TextureEntryContainer()
            {
                m_TextureEntry = new TextureEntry();
            }

            public TextureEntryContainer(TextureEntry entry)
            {
                m_TextureEntry = entry;
            }

            public TextureEntryContainer(TextureEntryContainer entry)
            {
                m_TextureEntry = new TextureEntry(entry.m_TextureEntry);
            }

            public byte[] GetBytes() => m_TextureEntry.GetBytes();

            public TextureEntryFaceContainer this[int index] => new TextureEntryFaceContainer(m_TextureEntry[(uint)index]);
        }

        [APIExtension("ViewerControl")]
        public const int VC_MAPPING_TYPE_DEFAULT = 0;
        [APIExtension("ViewerControl")]
        public const int VC_MAPPING_TYPE_PLANAR = 2;
        [APIExtension("ViewerControl")]
        public const int VC_MAPPING_TYPE_SPHERICAL = 4;
        [APIExtension("ViewerControl")]
        public const int VC_MAPPING_TYPE_CYLINDRICAL = 6;

        [APIExtension("ViewerControl")]
        public const int VC_SHININESS_NONE = 0;
        [APIExtension("ViewerControl")]
        public const int VC_SHININESS_LOW = 64;
        [APIExtension("ViewerControl")]
        public const int VC_SHININESS_MEDIUM = 128;
        [APIExtension("ViewerControl")]
        public const int VC_SHININESS_HIGH = 192;

        [APIExtension("ViewerControl", "textureentryface")]
        [APIDisplayName("textureentryface")]
        [APIAccessibleMembers]
        public sealed class TextureEntryFaceContainer
        {
            private TextureEntryFace m_Face;

            public TextureEntryFaceContainer(TextureEntryFace face)
            {
                m_Face = face;
            }

            public int TexMapType
            {
                get { return (int)m_Face.TexMapType; }
                set { m_Face.TexMapType = (MappingType)value; }
            }

            public int MediaFlags
            {
                get { return m_Face.MediaFlags.ToLSLBoolean(); }
                set { m_Face.MediaFlags = value != 0; }
            }

            public int FullBright
            {
                get { return m_Face.FullBright.ToLSLBoolean(); }
                set { m_Face.FullBright = value != 0; }
            }

            public int Shiny
            {
                get { return (int)m_Face.Shiny; }
                set { m_Face.Shiny = (Shininess)value; }
            }

            public int Bump
            {
                get { return (int)m_Face.Bump; }
                set { m_Face.Bump = (Bumpiness)value; }
            }

            public double Glow
            {
                get { return m_Face.Glow; }
                set { m_Face.Glow = (float)value; }
            }

            public double Rotation
            {
                get { return m_Face.Rotation; }
                set { m_Face.Rotation = (float)value; }
            }

            public LSLKey MaterialID
            {
                get { return m_Face.MaterialID; }
                set { m_Face.MaterialID = value.AsUUID; }
            }

            public double OffsetU
            {
                get { return m_Face.OffsetU; }
                set { m_Face.OffsetU = (float)value; }
            }

            public double OffsetV
            {
                get { return m_Face.OffsetV; }
                set { m_Face.OffsetV = (float)value; }
            }

            public double RepeatU
            {
                get { return m_Face.RepeatU; }
                set { m_Face.RepeatU = (float)value; }
            }

            public double RepeatV
            {
                get { return m_Face.RepeatV; }
                set { m_Face.RepeatV = (float)value; }
            }

            public Vector3 TextureColor
            {
                get { return m_Face.TextureColor.AsVector3; }
                set
                {
                    ColorAlpha color = m_Face.TextureColor;
                    color.R = value.X;
                    color.G = value.Y;
                    color.B = value.Z;
                    m_Face.TextureColor = color;
                }
            }

            public double TextureAlpha
            {
                get { return m_Face.TextureColor.A; }
                set
                {
                    ColorAlpha color = m_Face.TextureColor;
                    color.A = value;
                    m_Face.TextureColor = color;
                }
            }

            public LSLKey TextureID
            {
                get { return m_Face.TextureID; }
                set { m_Face.TextureID = value.AsUUID; }
            }
        }
    }
}

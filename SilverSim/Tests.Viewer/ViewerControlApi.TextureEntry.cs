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

namespace SilverSim.Tests.Viewer
{
    public partial class ViewerControlApi
    {
        [APIExtension(ExtensionName, "textureentry")]
        [APIDisplayName("textureentry")]
        [APIAccessibleMembers]
        [APIIsVariableType]
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
            public byte[] GetBytes(bool fullbrightdisable, double glowlimitintensity) => m_TextureEntry.GetBytes(fullbrightdisable, (float)glowlimitintensity);

            public TextureEntryFaceContainer Default => new TextureEntryFaceContainer(m_TextureEntry.DefaultTexture);

            public TextureEntryFaceContainer this[int index] => new TextureEntryFaceContainer(m_TextureEntry[(uint)index]);

            public ByteArrayApi.ByteArray Bytes
            {
                get
                {
                    return new ByteArrayApi.ByteArray(GetBytes());
                }

                set
                {
                    m_TextureEntry = new TextureEntry(value.Data);
                }
            }

            public void OptimizeDefault(int numFaces)
            {
                m_TextureEntry.OptimizeDefault(numFaces);
            }

            public bool ContainsKey(int index) => index >= 0 && m_TextureEntry.ContainsKey((uint)index);
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction, "GetBytesLimited")]
        public ByteArrayApi.ByteArray TEGetBytesLimited(TextureEntryContainer te, int fullbrightdisable, double glowlimitintensity)
            => new ByteArrayApi.ByteArray(te.GetBytes(fullbrightdisable != 0, glowlimitintensity));

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction, "OptimizeDefault")]
        public void TEOptimizeDefault(TextureEntryContainer te, int numFaces) => te.OptimizeDefault(numFaces);

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction, "ContainsKey")]
        public int ContainsKey(TextureEntryContainer te, int face) => te.ContainsKey(face).ToLSLBoolean();

        [APIExtension(ExtensionName)]
        public const int VC_MAPPING_TYPE_DEFAULT = 0;
        [APIExtension(ExtensionName)]
        public const int VC_MAPPING_TYPE_PLANAR = 1;
        [APIExtension(ExtensionName)]
        public const int VC_MAPPING_TYPE_SPHERICAL = 2;
        [APIExtension(ExtensionName)]
        public const int VC_MAPPING_TYPE_CYLINDRICAL = 3;

        [APIExtension(ExtensionName)]
        public const int VC_SHININESS_NONE = 0;
        [APIExtension(ExtensionName)]
        public const int VC_SHININESS_LOW = 1;
        [APIExtension(ExtensionName)]
        public const int VC_SHININESS_MEDIUM = 2;
        [APIExtension(ExtensionName)]
        public const int VC_SHININESS_HIGH = 3;

        [APIExtension(ExtensionName, "textureentryface")]
        [APIDisplayName("textureentryface")]
        [APIAccessibleMembers]
        [APIIsVariableType]
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

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

using SilverSim.Main.Common;
using SilverSim.Scene.Types.KeyframedMotion;
using SilverSim.Scene.Types.KeyframedMotion.Serialization;
using SilverSim.Tests.Extensions;
using SilverSim.Types;
using System;

namespace SilverSim.Tests.Assets.Formats
{
    public sealed class OsKeyframeSerialization : ITest
    {
        private readonly byte[] TestData = Convert.FromBase64String("AAEAAAD/////AQAAAAAAAAAMAgAAABhPcGVuU2ltLlJlZ2lvbi5GcmFtZXdvcmsMAwAAABJPcGVuTWV0YXZlcnNlVHlwZXMFAQAAAC5PcGVuU2ltLlJlZ2lvbi5GcmFtZXdvcmsuU2NlbmVzLktleWZyYW1lTW90aW9uCwAAABRtX3NlcmlhbGl6ZWRQb3NpdGlvbg5tX2Jhc2VQb3NpdGlvbg5tX2Jhc2VSb3RhdGlvbg5tX2N1cnJlbnRGcmFtZQhtX2ZyYW1lcwttX2tleWZyYW1lcwZtX21vZGUGbV9kYXRhCW1fcnVubmluZwxtX2l0ZXJhdGlvbnMLbV9za2lwTG9vcHMEBAQEAwQEBAAAABVPcGVuTWV0YXZlcnNlLlZlY3RvcjMDAAAAFU9wZW5NZXRhdmVyc2UuVmVjdG9yMwMAAAAYT3Blbk1ldGF2ZXJzZS5RdWF0ZXJuaW9uAwAAADdPcGVuU2ltLlJlZ2lvbi5GcmFtZXdvcmsuU2NlbmVzLktleWZyYW1lTW90aW9uK0tleWZyYW1lAgAAALMBU3lzdGVtLkNvbGxlY3Rpb25zLkdlbmVyaWMuTGlzdGAxW1tPcGVuU2ltLlJlZ2lvbi5GcmFtZXdvcmsuU2NlbmVzLktleWZyYW1lTW90aW9uK0tleWZyYW1lLCBPcGVuU2ltLlJlZ2lvbi5GcmFtZXdvcmssIFZlcnNpb249MC40Ny4xMS4zNjQ4MywgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1udWxsXV05T3BlblNpbS5SZWdpb24uRnJhbWV3b3JrLlNjZW5lcy5LZXlmcmFtZU1vdGlvbitLZXlmcmFtZVtdAgAAADdPcGVuU2ltLlJlZ2lvbi5GcmFtZXdvcmsuU2NlbmVzLktleWZyYW1lTW90aW9uK1BsYXlNb2RlAgAAADlPcGVuU2ltLlJlZ2lvbi5GcmFtZXdvcmsuU2NlbmVzLktleWZyYW1lTW90aW9uK0RhdGFGb3JtYXQCAAAAAQgIAgAAAAUEAAAAFU9wZW5NZXRhdmVyc2UuVmVjdG9yMwMAAAABWAFZAVoAAAALCwsDAAAAYsMMQw7jsUItyqpBAQUAAAAEAAAAYsMMQw7jsUItyqpBBQYAAAAYT3Blbk1ldGF2ZXJzZS5RdWF0ZXJuaW9uBAAAAAFYAVkBWgFXAAAAAAsLCwsDAAAAAAAAAAAAAADkW18/kiz6PgUHAAAAN09wZW5TaW0uUmVnaW9uLkZyYW1ld29yay5TY2VuZXMuS2V5ZnJhbWVNb3Rpb24rS2V5ZnJhbWUHAAAACFBvc2l0aW9uCFJvdGF0aW9uDVN0YXJ0Um90YXRpb24GVGltZU1TCVRpbWVUb3RhbA9Bbmd1bGFyVmVsb2NpdHkNU3RhcnRQb3NpdGlvbgMDBAAABAR1U3lzdGVtLk51bGxhYmxlYDFbW09wZW5NZXRhdmVyc2UuVmVjdG9yMywgT3Blbk1ldGF2ZXJzZVR5cGVzLCBWZXJzaW9uPTAuMC4wLjAsIEN1bHR1cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49bnVsbF1deFN5c3RlbS5OdWxsYWJsZWAxW1tPcGVuTWV0YXZlcnNlLlF1YXRlcm5pb24sIE9wZW5NZXRhdmVyc2VUeXBlcywgVmVyc2lvbj0wLjAuMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPW51bGxdXRhPcGVuTWV0YXZlcnNlLlF1YXRlcm5pb24DAAAACAgVT3Blbk1ldGF2ZXJzZS5WZWN0b3IzAwAAABVPcGVuTWV0YXZlcnNlLlZlY3RvcjMDAAAAAgAAAAEIAAAABAAAAGLDDEMO47FCLcqqQQEJAAAABgAAAAAAAAAAAAAA5FtfP5Is+j4BCgAAAAYAAAAAAAAAAAAAAJIs+r7kW18/9AEAANAHAAABCwAAAAQAAAAAAAAAAAAAAAAAAAABDAAAAAQAAABiwwxDDuOxQi3KqkEJDQAAAAkOAAAABQ8AAAA3T3BlblNpbS5SZWdpb24uRnJhbWV3b3JrLlNjZW5lcy5LZXlmcmFtZU1vdGlvbitQbGF5TW9kZQEAAAAHdmFsdWVfXwAIAgAAAAIAAAAFEAAAADlPcGVuU2ltLlJlZ2lvbi5GcmFtZXdvcmsuU2NlbmVzLktleWZyYW1lTW90aW9uK0RhdGFGb3JtYXQBAAAAB3ZhbHVlX18ACAIAAAABAAAAAQ4AAAAAAAAABA0AAACzAVN5c3RlbS5Db2xsZWN0aW9ucy5HZW5lcmljLkxpc3RgMVtbT3BlblNpbS5SZWdpb24uRnJhbWV3b3JrLlNjZW5lcy5LZXlmcmFtZU1vdGlvbitLZXlmcmFtZSwgT3BlblNpbS5SZWdpb24uRnJhbWV3b3JrLCBWZXJzaW9uPTAuNDcuMTEuMzY0ODMsIEN1bHR1cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49bnVsbF1dAwAAAAZfaXRlbXMFX3NpemUIX3ZlcnNpb24EAAA5T3BlblNpbS5SZWdpb24uRnJhbWV3b3JrLlNjZW5lcy5LZXlmcmFtZU1vdGlvbitLZXlmcmFtZVtdAgAAAAgICREAAAABAAAARQAAAAcOAAAAAAEAAAACAAAABDdPcGVuU2ltLlJlZ2lvbi5GcmFtZXdvcmsuU2NlbmVzLktleWZyYW1lTW90aW9uK0tleWZyYW1lAgAAAAESAAAABwAAAAoBEwAAAAYAAAAAAAAAAAAAAAAAAAAAAIA/ARQAAAAGAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAARUAAAAEAAAAAAAAAAAAAAAAAAAAARYAAAAEAAAAAAAAAAAAAAAAAAAAARcAAAAHAAAACgEYAAAABgAAAAAAAAAAAAAAAACAP0YLUTIBGQAAAAYAAAAAAAAAAAAAAAAAAAAAAAAA0AcAAAAAAAABGgAAAAQAAAAAAAAAAAAAAAAAAAABGwAAAAQAAAAAAAAAAAAAAAAAAAAHEQAAAAABAAAABAAAAAQ3T3BlblNpbS5SZWdpb24uRnJhbWV3b3JrLlNjZW5lcy5LZXlmcmFtZU1vdGlvbitLZXlmcmFtZQIAAAABHAAAAAcAAAABHQAAAAQAAABiwwxDDuOxQi3KqkEBHgAAAAYAAAAAAAAAAAAAAORbXz+SLPo+AR8AAAAGAAAAAAAAAAAAAACSLPq+5FtfP9AHAADQBwAAASAAAAAEAAAAAAAAAAAAAAAAAAAAASEAAAAEAAAAYsMMQw7jsUItyqpBASIAAAAHAAAACgoBIwAAAAYAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABJAAAAAQAAAAAAAAAAAAAAAAAAAABJQAAAAQAAAAAAAAAAAAAAAAAAAABJgAAAAcAAAAKCgEnAAAABgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAEoAAAABAAAAAAAAAAAAAAAAAAAAAEpAAAABAAAAAAAAAAAAAAAAAAAAAEqAAAABwAAAAoKASsAAAAGAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAASwAAAAEAAAAAAAAAAAAAAAAAAAAAS0AAAAEAAAAAAAAAAAAAAAAAAAACw==");

        public void Cleanup()
        {
        }

        public bool Run()
        {
            KeyframedMotion kfm = KfOpenSim.Deserialize(TestData);
            byte[] result = kfm.Serialize(Vector3.Zero, Quaternion.Identity);
            KeyframedMotion kfm2 = KfOpenSim.Deserialize(result);
            return true;
        }

        public void Setup()
        {
        }

        public void Startup(ConfigurationLoader loader)
        {
        }
    }
}

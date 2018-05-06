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
using SilverSim.Scene.Types.Scene;
using SilverSim.Scene.Types.Script;
using SilverSim.Scripting.Lsl;
using SilverSim.Types;
using System;
using System.Runtime.Serialization;

namespace SilverSim.Tests.Lsl.TestWind
{
    [LSLImplementation]
    [ScriptApiName("WindTestExtensions")]
    [PluginName("TestWind")]
    public class WindModelFactory : IWindModelFactory, IPlugin, IScriptApi
    {
        public IWindModel Instantiate(SceneInterface scene) => new WindModel(scene);

        public void Startup(ConfigurationLoader loader)
        {
            /* intentionally left empty */
        }

        [APIExtension("TestWind", "windaccess")]
        [APIDisplayName("windaccess")]
        [APIAccessibleMembers]
        public sealed class WindModelAccessor
        {
            private readonly WindModel m_WindModel;

            public WindModelAccessor(WindModel wm)
            {
                m_WindModel = wm;
            }

            public Vector3 this[Vector3 pos]
            {
                get
                {
                    return ((IWindModel)m_WindModel)[pos];
                }
                set
                {
                    ((IWindModel)m_WindModel)[pos] = value;
                }
            }

            public Vector3 PrevailingWind
            {
                get
                {
                    return m_WindModel.PrevailingWind;
                }
                set
                {
                    m_WindModel.PrevailingWind = value;
                }
            }
        }

        public sealed class TestWindException : Exception
        {
            public TestWindException()
            {
            }

            public TestWindException(string message) : base(message)
            {
            }

            public TestWindException(string message, Exception innerException) : base(message, innerException)
            {
            }

            private TestWindException(SerializationInfo info, StreamingContext context) : base(info, context)
            {
            }
        }

        [APIExtension("TestWind", APIUseAsEnum.Getter, "Wind")]
        public WindModelAccessor GetWind(ScriptInstance instance)
        {
            lock(instance)
            {
                WindModel model = instance.Part.ObjectGroup.Scene.Environment.Wind as WindModel;
                if(model == null)
                {
                    throw new TestWindException("Wrong wind model in region for test wind");
                }

                return new WindModelAccessor(model);
            }
        }
    }
}

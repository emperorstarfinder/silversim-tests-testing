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

using log4net;
using SilverSim.Main.Common;
using SilverSim.Scene.Types.Object;
using SilverSim.Tests.Extensions;
using SilverSim.Types.Primitive;
using System.Reflection;

namespace SilverSim.Tests.SimulationData
{
    public class SLMeshFaceEncodingTest : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Startup(ConfigurationLoader loader)
        {
            /* intentionally left empty */
        }

        public void Setup()
        {
            /* intentionally left empty */
        }

        public bool Run()
        {
            bool success = true;
            m_Log.Info("Number of faces 1");
            var shape = new ObjectPart.PrimitiveShape
            {
                PCode = PrimitiveCode.Prim,
                PathCurve = 32,
                ProfileCurve = 5,
                PathBegin = 0,
                PathEnd = 0,
                PathScaleX = 100,
                PathScaleY = 100,
                PathShearX = 0,
                PathShearY = 0,
                PathTwist = 0,
                PathTwistBegin = 0,
                PathRadiusOffset = 0,
                PathTaperX = 0,
                PathTaperY = 0,
                PathRevolutions = 0,
                PathSkew = 0,
                ProfileBegin = 0,
                ProfileEnd = 0,
                ProfileHollow = 0,
            };
            if(shape.NumberOfSides != 1)
            {
                success = false;
                m_Log.InfoFormat("FAIL Got {0}", shape.NumberOfSides);
            }

            m_Log.Info("Number of faces 2");
            shape = new ObjectPart.PrimitiveShape
            {
                PCode = PrimitiveCode.Prim,
                PathCurve = 32,
                ProfileCurve = 5,
                PathBegin = 0,
                PathEnd = 0,
                PathScaleX = 100,
                PathScaleY = 165,
                PathShearX = 0,
                PathShearY = 0,
                PathTwist = 0,
                PathTwistBegin = 0,
                PathRadiusOffset = 0,
                PathTaperX = 0,
                PathTaperY = 0,
                PathRevolutions = 0,
                PathSkew = 0,
                ProfileBegin = 0,
                ProfileEnd = 0,
                ProfileHollow = 250,
            };
            if (shape.NumberOfSides != 2)
            {
                success = false;
                m_Log.InfoFormat("FAIL Got {0}", shape.NumberOfSides);
            }

            m_Log.Info("Number of faces 3");
            shape = new ObjectPart.PrimitiveShape
            {
                PCode = PrimitiveCode.Prim,
                PathCurve = 16,
                ProfileCurve = 0,
                PathBegin = 0,
                PathEnd = 0,
                PathScaleX = 100,
                PathScaleY = 100,
                PathShearX = 0,
                PathShearY = 0,
                PathTwist = 0,
                PathTwistBegin = 0,
                PathRadiusOffset = 0,
                PathTaperX = 0,
                PathTaperY = 0,
                PathRevolutions = 0,
                PathSkew = 0,
                ProfileBegin = 0,
                ProfileEnd = 0,
                ProfileHollow = 0,
            };
            if (shape.NumberOfSides != 3)
            {
                success = false;
                m_Log.InfoFormat("FAIL Got {0}", shape.NumberOfSides);
            }

            m_Log.Info("Number of faces 4");
            shape = new ObjectPart.PrimitiveShape
            {
                PCode = PrimitiveCode.Prim,
                PathCurve = 16,
                ProfileCurve = 0,
                PathBegin = 0,
                PathEnd = 0,
                PathScaleX = 100,
                PathScaleY = 100,
                PathShearX = 0,
                PathShearY = 0,
                PathTwist = 0,
                PathTwistBegin = 0,
                PathRadiusOffset = 0,
                PathTaperX = 0,
                PathTaperY = 0,
                PathRevolutions = 0,
                PathSkew = 0,
                ProfileBegin = 0,
                ProfileEnd = 0,
                ProfileHollow = 25000,
            };
            if (shape.NumberOfSides != 4)
            {
                success = false;
                m_Log.InfoFormat("FAIL Got {0}", shape.NumberOfSides);
            }

            m_Log.Info("Number of faces 5");
            shape = new ObjectPart.PrimitiveShape
            {
                PCode = PrimitiveCode.Prim,
                PathCurve = 16,
                ProfileCurve = 3,
                PathBegin = 0,
                PathEnd = 0,
                PathScaleX = 100,
                PathScaleY = 100,
                PathShearX = 0,
                PathShearY = 0,
                PathTwist = 0,
                PathTwistBegin = 0,
                PathRadiusOffset = 0,
                PathTaperX = 0,
                PathTaperY = 0,
                PathRevolutions = 0,
                PathSkew = 0,
                ProfileBegin = 0,
                ProfileEnd = 0,
                ProfileHollow = 0,
            };
            if (shape.NumberOfSides != 5)
            {
                success = false;
                m_Log.InfoFormat("FAIL Got {0}", shape.NumberOfSides);
            }

            m_Log.Info("Number of faces 6");
            shape = new ObjectPart.PrimitiveShape
            {
                PCode = PrimitiveCode.Prim,
                PathCurve = 16,
                ProfileCurve = 1,
                PathBegin = 0,
                PathEnd = 0,
                PathScaleX = 100,
                PathScaleY = 100,
                PathShearX = 0,
                PathShearY = 0,
                PathTwist = 0,
                PathTwistBegin = 0,
                PathRadiusOffset = 0,
                PathTaperX = 0,
                PathTaperY = 0,
                PathRevolutions = 0,
                PathSkew = 0,
                ProfileBegin = 0,
                ProfileEnd = 0,
                ProfileHollow = 0,
            };
            if (shape.NumberOfSides != 6)
            {
                success = false;
                m_Log.InfoFormat("FAIL Got {0}", shape.NumberOfSides);
            }

#if SEEMS_TO_HAVE_SPECIAL_HANDLING
            m_Log.Info("Number of faces 7");
            shape = new ObjectPart.PrimitiveShape
            {
                PCode = PrimitiveCode.Prim,
                PathCurve = 16,
                ProfileCurve = 1,
                PathBegin = 0,
                PathEnd = 0,
                PathScaleX = 100,
                PathScaleY = 100,
                PathShearX = 0,
                PathShearY = 0,
                PathTwist = 0,
                PathTwistBegin = 0,
                PathRadiusOffset = 0,
                PathTaperX = 0,
                PathTaperY = 0,
                PathRevolutions = 0,
                PathSkew = 0,
                ProfileBegin = 0,
                ProfileEnd = 12500,
                ProfileHollow = 0,
            };
            if (shape.NumberOfSides != 7)
            {
                success = false;
                m_Log.InfoFormat("FAIL Got {0}", shape.NumberOfSides);
            }
#endif

            m_Log.Info("Number of faces 8");
            shape = new ObjectPart.PrimitiveShape
            {
                PCode = PrimitiveCode.Prim,
                PathCurve = 16,
                ProfileCurve = 1,
                PathBegin = 0,
                PathEnd = 0,
                PathScaleX = 100,
                PathScaleY = 100,
                PathShearX = 0,
                PathShearY = 0,
                PathTwist = 0,
                PathTwistBegin = 0,
                PathRadiusOffset = 0,
                PathTaperX = 0,
                PathTaperY = 0,
                PathRevolutions = 0,
                PathSkew = 0,
                ProfileBegin = 0,
                ProfileEnd = 6250,
                ProfileHollow = 0,
            };
            if (shape.NumberOfSides != 8)
            {
                success = false;
                m_Log.InfoFormat("FAIL Got {0}", shape.NumberOfSides);
            }
            return success;
        }

        public void Cleanup()
        {
            /* intentionally left empty */
        }
    }
}

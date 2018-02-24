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
    public sealed class MeshFaceEncodingTest : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Startup(ConfigurationLoader loader)
        {
        }

        public void Setup()
        {

        }

        public void Cleanup()
        {

        }

        public bool Run()
        {
            m_Log.Info("Test num of faces 1 encoding");
            bool success = true;
            ObjectPart.PrimitiveShape shape = new ObjectPart.PrimitiveShape
            {
                PCode = PrimitiveCode.Prim,
                ProfileCurve = (byte)PrimitiveProfileShape.Circle,
                PathCurve = (byte)PrimitiveExtrusion.Curve1,
                PathScaleX = 100,
                PathScaleY = 150
            };

            if(!shape.IsSane)
            {
                m_Log.Error("faces 1 encoding not sane");
                success = false;
            }

            if(shape.NumberOfSides != 1)
            {
                m_Log.Error("faces 1 encoding not showing 1 faces");
                success = false;
            }


            m_Log.Info("Test num of faces 2 encoding");
            shape = new ObjectPart.PrimitiveShape
            {
                PCode = PrimitiveCode.Prim,
                ProfileCurve = (byte)PrimitiveProfileShape.Circle,
                PathCurve = (byte)PrimitiveExtrusion.Curve1,
                ProfileHollow = 27500,
                PathScaleX = 100,
                PathScaleY = 150
            };

            if (!shape.IsSane)
            {
                m_Log.Error("faces 2 encoding not sane");
                success = false;
            }

            if (shape.NumberOfSides != 2)
            {
                m_Log.Error("faces 2 encoding not showing 2 faces");
                success = false;
            }


            m_Log.Info("Test num of faces 3 encoding");
            shape = new ObjectPart.PrimitiveShape
            {
                PCode = PrimitiveCode.Prim,
                ProfileCurve = (byte)PrimitiveProfileShape.Circle,
                PathCurve = (byte)PrimitiveExtrusion.Straight,
                PathScaleX = 100,
                PathScaleY = 100
            };

            if (!shape.IsSane)
            {
                m_Log.Error("faces 3 encoding not sane");
                success = false;
            }

            if (shape.NumberOfSides != 3)
            {
                m_Log.Error("faces 3 encoding not showing 3 faces");
                success = false;
            }

            m_Log.Info("Test num of faces 4 encoding");
            shape = new ObjectPart.PrimitiveShape
            {
                PCode = PrimitiveCode.Prim,
                ProfileCurve = (byte)PrimitiveProfileShape.Circle,
                PathCurve = (byte)PrimitiveExtrusion.Straight,
                ProfileHollow = 27500,
                PathScaleX = 100,
                PathScaleY = 100
            };

            if (!shape.IsSane)
            {
                m_Log.Error("faces 4 encoding not sane");
                success = false;
            }

            if (shape.NumberOfSides != 4)
            {
                m_Log.Error("faces 4 encoding not showing 4 faces");
                success = false;
            }

            m_Log.Info("Test num of faces 5 encoding");
            shape = new ObjectPart.PrimitiveShape
            {
                PCode = PrimitiveCode.Prim,
                ProfileCurve = (byte)PrimitiveProfileShape.EquilateralTriangle,
                PathCurve = (byte)PrimitiveExtrusion.Straight,
                PathScaleX = 100,
                PathScaleY = 100
            };

            if (!shape.IsSane)
            {
                m_Log.Error("faces 5 encoding not sane");
                success = false;
            }

            if (shape.NumberOfSides != 5)
            {
                m_Log.Error("faces 5 encoding not showing 5 faces");
                success = false;
            }

            m_Log.Info("Test num of faces 6 encoding");
            shape = new ObjectPart.PrimitiveShape
            {
                PCode = PrimitiveCode.Prim,
                ProfileCurve = (byte)PrimitiveProfileShape.Square,
                PathCurve = (byte)PrimitiveExtrusion.Straight,
                PathScaleX = 100,
                PathScaleY = 100
            };

            if (!shape.IsSane)
            {
                m_Log.Error("faces 6 encoding not sane");
                success = false;
            }

            if (shape.NumberOfSides != 6)
            {
                m_Log.Error("faces 6 encoding not showing 6 faces");
                success = false;
            }

            m_Log.Info("Test num of faces 7 encoding");
            shape = new ObjectPart.PrimitiveShape
            {
                PCode = PrimitiveCode.Prim,
                ProfileCurve = (byte)PrimitiveProfileShape.Square,
                PathCurve = (byte)PrimitiveExtrusion.Straight,
                ProfileHollow = 27500,
                PathScaleX = 100,
                PathScaleY = 100
            };

            if (!shape.IsSane)
            {
                m_Log.Error("faces 7 encoding not sane");
                success = false;
            }
            if (shape.NumberOfSides != 7)
            {
                m_Log.Error("faces 7 encoding not showing 7 faces");
                success = false;
            }


            m_Log.Info("Test num of faces 8 encoding");
            shape = new ObjectPart.PrimitiveShape
            {
                PCode = PrimitiveCode.Prim,
                ProfileCurve = (byte)PrimitiveProfileShape.Square,
                PathCurve = (byte)PrimitiveExtrusion.Straight,
                ProfileBegin = 9375,
                PathScaleX = 100,
                PathScaleY = 100
            };

            if (!shape.IsSane)
            {
                m_Log.Error("faces 8 encoding not sane");
                success = false;
            }

            if (shape.NumberOfSides != 8)
            {
                m_Log.Error("faces 8 encoding not showing 8 faces");
                success = false;
            }

            m_Log.Info("Test num of faces 9 encoding");
            shape = new ObjectPart.PrimitiveShape
            {
                PCode = PrimitiveCode.Prim,
                ProfileCurve = (byte)PrimitiveProfileShape.Square,
                PathCurve = (byte)PrimitiveExtrusion.Straight,
                ProfileBegin = 9375,
                ProfileHollow = 27500,
                PathScaleX = 100,
                PathScaleY = 100
            };

            if (!shape.IsSane)
            {
                m_Log.Error("faces 9 encoding not sane");
                success = false;
            }

            if (shape.NumberOfSides != 9)
            {
                m_Log.Error("faces 9 encoding not showing 9 faces");
                success = false;
            }

            m_Log.Info("****** Use actual function *****");
            m_Log.Info("Test num of faces 1 encoding");
            shape = new ObjectPart.PrimitiveShape
            {
                PCode = PrimitiveCode.Prim,
                SculptType = PrimitiveSculptType.Mesh
            };

            shape.SetMeshNumFaces(1);

            if (!shape.IsSane)
            {
                m_Log.Error("faces 1 encoding not sane");
                success = false;
            }

            if (shape.NumberOfSides != 1)
            {
                m_Log.Error("faces 1 encoding not showing 1 faces");
                success = false;
            }


            m_Log.Info("Test num of faces 2 encoding");
            shape.SetMeshNumFaces(2);

            if (!shape.IsSane)
            {
                m_Log.Error("faces 2 encoding not sane");
                success = false;
            }

            if (shape.NumberOfSides != 2)
            {
                m_Log.Error("faces 2 encoding not showing 2 faces");
                success = false;
            }


            m_Log.Info("Test num of faces 3 encoding");
            shape.SetMeshNumFaces(3);

            if (!shape.IsSane)
            {
                m_Log.Error("faces 3 encoding not sane");
                success = false;
            }

            if (shape.NumberOfSides != 3)
            {
                m_Log.Error("faces 3 encoding not showing 3 faces");
                success = false;
            }

            m_Log.Info("Test num of faces 4 encoding");
            shape.SetMeshNumFaces(4);

            if (!shape.IsSane)
            {
                m_Log.Error("faces 4 encoding not sane");
                success = false;
            }

            if (shape.NumberOfSides != 4)
            {
                m_Log.Error("faces 4 encoding not showing 4 faces");
                success = false;
            }

            m_Log.Info("Test num of faces 5 encoding");
            shape.SetMeshNumFaces(5);

            if (!shape.IsSane)
            {
                m_Log.Error("faces 5 encoding not sane");
                success = false;
            }

            if (shape.NumberOfSides != 5)
            {
                m_Log.Error("faces 5 encoding not showing 5 faces");
                success = false;
            }

            m_Log.Info("Test num of faces 6 encoding");
            shape.SetMeshNumFaces(6);

            if (!shape.IsSane)
            {
                m_Log.Error("faces 6 encoding not sane");
                success = false;
            }

            if (shape.NumberOfSides != 6)
            {
                m_Log.Error("faces 6 encoding not showing 6 faces");
                success = false;
            }

            m_Log.Info("Test num of faces 7 encoding");
            shape.SetMeshNumFaces(7);

            if (!shape.IsSane)
            {
                m_Log.Error("faces 7 encoding not sane");
                success = false;
            }
            if (shape.NumberOfSides != 7)
            {
                m_Log.Error("faces 7 encoding not showing 7 faces");
                success = false;
            }

            m_Log.Info("Test num of faces 8 encoding");
            shape.SetMeshNumFaces(8);

            if (!shape.IsSane)
            {
                m_Log.Error("faces 8 encoding not sane");
                success = false;
            }

            if (shape.NumberOfSides != 8)
            {
                m_Log.Error("faces 8 encoding not showing 8 faces");
                success = false;
            }

            m_Log.Info("Test num of faces 9 encoding");
            shape.SetMeshNumFaces(9);

            if (!shape.IsSane)
            {
                m_Log.Error("faces 9 encoding not sane");
                success = false;
            }

            if (shape.NumberOfSides != 9)
            {
                m_Log.Error("faces 9 encoding not showing 9 faces");
                success = false;
            }

            return success;
        }
    }
}

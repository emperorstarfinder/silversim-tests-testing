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
using SilverSim.Scene.ServiceInterfaces.SimulationData;
using SilverSim.Scene.Types.Object;
using SilverSim.Types;
using SilverSim.Types.Asset.Format.Mesh;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SilverSim.Tests.SimulationData
{
    public class PhysicsConvexPrimStorageTests : CommonSimDataTest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override bool Run()
        {
            ISimulationDataPhysicsConvexStorageInterface m_PhysicsStorage = SimulationData.PhysicsConvexShapes;
            var shape = new PhysicsConvexShape();
            var hull = new PhysicsConvexShape.ConvexHull();
            hull.Vertices.Add(Vector3.UnitX);
            hull.Vertices.Add(Vector3.UnitY);
            hull.Vertices.Add(Vector3.UnitZ);
            hull.Triangles.Add(0);
            hull.Triangles.Add(1);
            hull.Triangles.Add(2);
            shape.Hulls.Add(hull);
            var shapeKey = new ObjectPart.PrimitiveShape();
            PhysicsConvexShape test;

            m_Log.Info("Testing non-existence 1");
            if (m_PhysicsStorage.ContainsKey(shapeKey))
            {
                return false;
            }
            m_Log.Info("Testing non-existence 2");
            if (m_PhysicsStorage.TryGetValue(shapeKey, out test))
            {
                return false;
            }
            m_Log.Info("Testing non-existence 3");
            try
            {
                shape = m_PhysicsStorage[shapeKey];
                return false;
            }
            catch (KeyNotFoundException)
            {
                /* we should see this one */
            }
            m_Log.Info("Testing non-existence 4");
            if (m_PhysicsStorage.Remove(shapeKey))
            {
                return false;
            }

            m_Log.Info("Create physics convex shape");
            m_PhysicsStorage[shapeKey] = shape;

            m_Log.Info("Testing existence 1");
            if (!m_PhysicsStorage.ContainsKey(shapeKey))
            {
                return false;
            }
            m_Log.Info("Testing existence 2");
            if (!m_PhysicsStorage.TryGetValue(shapeKey, out test))
            {
                return false;
            }
            m_Log.Info("Testing equality");
            if (!test.SerializedData.SequenceEqual(shape.SerializedData))
            {
                return false;
            }
            m_Log.Info("Testing existence 3");
            shape = m_PhysicsStorage[shapeKey];
            m_Log.Info("Testing equality");
            if (!test.SerializedData.SequenceEqual(shape.SerializedData))
            {
                return false;
            }
            m_Log.Info("Testing existence 4");
            if (!m_PhysicsStorage.Remove(shapeKey))
            {
                return false;
            }

            return true;
        }
    }
}

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

using SilverSim.Scene.Types.Scene;
using SilverSim.Types;
using System;
using EnvironmentController = SilverSim.Scene.Types.SceneEnvironment.EnvironmentController;

namespace SilverSim.Tests.Lsl.TestWind
{
    public sealed class WindModel : IWindModel, IWindModelPreset
    {
        private Vector3[,] m_WindVectors;
        private readonly GridVector m_RegionSize;

        Vector3 IWindModel.this[Vector3 pos]
        {
            get
            {
                int x = (int)pos.X.Clamp(0, m_RegionSize.X - 1);
                int y = (int)pos.Y.Clamp(0, m_RegionSize.Y - 1);
                return m_WindVectors[x, y];
            }
            set
            {
                int x = (int)pos.X.Clamp(0, m_RegionSize.X - 1);
                int y = (int)pos.Y.Clamp(0, m_RegionSize.Y - 1);
                m_WindVectors[x, y] = value;
            }
        }

        Vector3 IWindModelPreset.this[Vector3 pos]
        {
            get
            {
                int x = (int)pos.X.Clamp(0, m_RegionSize.X - 1);
                int y = (int)pos.Y.Clamp(0, m_RegionSize.Y - 1);
                return m_WindVectors[x, y];

                throw new NotImplementedException();
            }
            set
            {
                int x = (int)pos.X.Clamp(0, m_RegionSize.X - 1);
                int y = (int)pos.Y.Clamp(0, m_RegionSize.Y - 1);
                m_WindVectors[x, y] = value;
            }
        }

        public string Name => "TestWind";

        public Vector3 PrevailingWind { get; set; }

        public IWindModelPreset PresetWind => this;

        public void UpdateModel(EnvironmentController.SunData sunData, double dt)
        {
            /* intentionally left empty */
        }

        public WindModel(SceneInterface scene)
        {
            m_RegionSize = new GridVector(scene.SizeX, scene.SizeY);
            m_WindVectors = new Vector3[scene.SizeX, scene.SizeY];
        }
    }
}
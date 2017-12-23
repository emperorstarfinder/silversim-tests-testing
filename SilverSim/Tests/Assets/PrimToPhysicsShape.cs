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
using Nini.Config;
using SilverSim.Main.Common;
using SilverSim.Scene.Physics.ShapeManager;
using SilverSim.Scene.Types.Object;
using SilverSim.Scene.Types.Object.Mesh;
using SilverSim.ServiceInterfaces.Asset;
using SilverSim.Tests.Extensions;
using SilverSim.Types;
using SilverSim.Types.Asset;
using SilverSim.Types.Asset.Format.Mesh;
using SilverSim.Types.Primitive;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace SilverSim.Tests.Assets
{
    public class PrimToPhysicsShape : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        PhysicsShapeManager m_PhysicsShapeManager;
        AssetServiceInterface m_AssetService;
        PrimitivePhysicsShapeType m_PhysicsShapeType;
        ObjectPart.PrimitiveShape.Decoded m_Shape = new ObjectPart.PrimitiveShape.Decoded();
        string m_OutputFileName;

        public void Startup(ConfigurationLoader loader)
        {
            IConfig config = loader.Config.Configs[GetType().FullName];
            m_AssetService = loader.GetService<AssetServiceInterface>(config.GetString("AssetService", "AssetService"));
            m_PhysicsShapeManager = loader.GetService<PhysicsShapeManager>(config.GetString("PhysicsShapeManager"));


            string physicsShapeType = config.GetString("PhysicsShapeType", "prim").ToLowerInvariant();
            switch (physicsShapeType)
            {
                case "none":
                    m_PhysicsShapeType = PrimitivePhysicsShapeType.None;
                    break;

                case "prim":
                    m_PhysicsShapeType = PrimitivePhysicsShapeType.Prim;
                    break;

                case "convex":
                    m_PhysicsShapeType = PrimitivePhysicsShapeType.Convex;
                    break;

                default:
                    throw new ConfigurationLoader.ConfigurationErrorException(string.Format("Invalid PhysicsShapeType: {0}", physicsShapeType));
            }

            if (config.Contains("HexData"))
            {
                ObjectPart.PrimitiveShape p = new ObjectPart.PrimitiveShape();
                p.Serialization = config.GetString("HexData").FromHexStringToByteArray();
            }
            else
            {
                string shapeType = config.GetString("ShapeType", "Box").ToLowerInvariant();
                switch (shapeType)
                {
                    case "box":
                        m_Shape.ShapeType = PrimitiveShapeType.Box;
                        break;

                    case "cylinder":
                        m_Shape.ShapeType = PrimitiveShapeType.Cylinder;
                        break;

                    case "prism":
                        m_Shape.ShapeType = PrimitiveShapeType.Prism;
                        break;

                    case "sphere":
                        m_Shape.ShapeType = PrimitiveShapeType.Sphere;
                        break;

                    case "torus":
                        m_Shape.ShapeType = PrimitiveShapeType.Torus;
                        break;

                    case "tube":
                        m_Shape.ShapeType = PrimitiveShapeType.Tube;
                        break;

                    case "ring":
                        m_Shape.ShapeType = PrimitiveShapeType.Ring;
                        break;

                    case "sculpt":
                        m_Shape.ShapeType = PrimitiveShapeType.Sculpt;
                        break;

                    default:
                        throw new ConfigurationLoader.ConfigurationErrorException(string.Format("Invalid ShapeType: {0}", shapeType));
                }

                string sculptType = config.GetString("SculptType", "sphere").ToLowerInvariant();
                switch (sculptType)
                {
                    case "sphere":
                        m_Shape.SculptType = PrimitiveSculptType.Sphere;
                        break;

                    case "torus":
                        m_Shape.SculptType = PrimitiveSculptType.Torus;
                        break;

                    case "plane":
                        m_Shape.SculptType = PrimitiveSculptType.Plane;
                        break;

                    case "cylinder":
                        m_Shape.SculptType = PrimitiveSculptType.Cylinder;
                        break;

                    case "mesh":
                        m_Shape.SculptType = PrimitiveSculptType.Mesh;
                        break;

                    default:
                        throw new ConfigurationLoader.ConfigurationErrorException(string.Format("Invalid SculptType: {0}", sculptType));
                }

                if (config.Contains("SculptMapID"))
                {
                    m_Shape.SculptMap = new UUID(config.GetString("SculptMapID"));
                }

                if (config.Contains("SculptMapFile"))
                {
                    if (!config.Contains("SculptMapID"))
                    {
                        throw new ConfigurationLoader.ConfigurationErrorException("SculptMap parameter not present");
                    }
                    byte[] data;
                    using (var fs = new FileStream(config.GetString("SculptMapFile"), FileMode.Open))
                    {
                        var fileLength = (int)fs.Length;
                        data = new byte[fileLength];
                        if (fileLength != fs.Read(data, 0, fileLength))
                        {
                            throw new ConfigurationLoader.ConfigurationErrorException("Failed to load file");
                        }
                    }
                    var assetdata = new AssetData
                    {
                        Data = data,
                        Type = m_Shape.SculptType == PrimitiveSculptType.Mesh ? AssetType.Mesh : AssetType.Texture,
                        ID = m_Shape.SculptMap,
                        Name = "PrimToMesh imported"
                    };
                    m_AssetService.Store(assetdata);
                }

                if (config.GetBoolean("IsSculptInverted", false))
                {
                    m_Shape.IsSculptInverted = true;
                }
                if (config.GetBoolean("IsSculptMirrored", false))
                {
                    m_Shape.IsSculptMirrored = true;
                }

                string profileShape = config.GetString("ProfileShape", "Circle").ToLowerInvariant();
                switch (profileShape)
                {
                    case "circle":
                        m_Shape.ProfileShape = PrimitiveProfileShape.Circle;
                        break;

                    case "square":
                        m_Shape.ProfileShape = PrimitiveProfileShape.Square;
                        break;

                    case "isometrictriangle":
                        m_Shape.ProfileShape = PrimitiveProfileShape.IsometricTriangle;
                        break;

                    case "equilateraltriangle":
                        m_Shape.ProfileShape = PrimitiveProfileShape.EquilateralTriangle;
                        break;

                    case "righttriangle":
                        m_Shape.ProfileShape = PrimitiveProfileShape.RightTriangle;
                        break;

                    case "halfcircle":
                        m_Shape.ProfileShape = PrimitiveProfileShape.HalfCircle;
                        break;

                    default:
                        throw new ConfigurationLoader.ConfigurationErrorException(string.Format("Invalid ProfileShape: {0}", profileShape));
                }

                string holeShape = config.GetString("HollowShape", "Same").ToLowerInvariant();
                switch (holeShape)
                {
                    case "same":
                        m_Shape.HoleShape = PrimitiveProfileHollowShape.Same;
                        break;

                    case "circle":
                        m_Shape.HoleShape = PrimitiveProfileHollowShape.Circle;
                        break;

                    case "square":
                        m_Shape.HoleShape = PrimitiveProfileHollowShape.Square;
                        break;

                    case "triangle":
                        m_Shape.HoleShape = PrimitiveProfileHollowShape.Triangle;
                        break;

                    default:
                        throw new ConfigurationLoader.ConfigurationErrorException(string.Format("Invalid HollowShape: {0}", holeShape));
                }

                m_Shape.ProfileBegin = double.Parse(config.GetString("ProfileBegin", "0"), CultureInfo.InvariantCulture);
                m_Shape.ProfileEnd = double.Parse(config.GetString("ProfileEnd", "0"), CultureInfo.InvariantCulture);
                m_Shape.ProfileHollow = double.Parse(config.GetString("ProfileHollow", "0"), CultureInfo.InvariantCulture);
                m_Shape.IsHollow = m_Shape.ProfileHollow > 0;
                m_Shape.PathBegin = double.Parse(config.GetString("PathBegin", "0"), CultureInfo.InvariantCulture);
                m_Shape.PathEnd = double.Parse(config.GetString("PathEnd", "1"), CultureInfo.InvariantCulture);
                m_Shape.IsOpen = m_Shape.PathBegin > 0 || m_Shape.PathEnd < 1f;
                m_Shape.PathScale.X = double.Parse(config.GetString("PathScaleX", "0"), CultureInfo.InvariantCulture);
                m_Shape.PathScale.Y = double.Parse(config.GetString("PathScaleY", "0"), CultureInfo.InvariantCulture);
                m_Shape.TopShear.X = double.Parse(config.GetString("TopShearX", "0"), CultureInfo.InvariantCulture);
                m_Shape.TopShear.Y = double.Parse(config.GetString("TopShearY", "0"), CultureInfo.InvariantCulture);
                m_Shape.TwistBegin = double.Parse(config.GetString("TwistBegin", "0"), CultureInfo.InvariantCulture);
                m_Shape.TwistEnd = double.Parse(config.GetString("TwistEnd", "0"), CultureInfo.InvariantCulture);
                m_Shape.RadiusOffset = double.Parse(config.GetString("RadiusOffset", "0"), CultureInfo.InvariantCulture);
                m_Shape.Taper.X = double.Parse(config.GetString("TaperX", "0"), CultureInfo.InvariantCulture);
                m_Shape.Taper.Y = double.Parse(config.GetString("TaperY", "0"), CultureInfo.InvariantCulture);
                m_Shape.Revolutions = double.Parse(config.GetString("Revolutions", "1"), CultureInfo.InvariantCulture);
                m_Shape.Skew = double.Parse(config.GetString("Skew", "0"), CultureInfo.InvariantCulture);
            }

            m_OutputFileName = config.GetString("OutputFilename");
        }

        public void Setup()
        {
        }

        public bool Run()
        {
            PhysicsShapeReference physicsShapeRef;
            ObjectPart.PrimitiveShape ps = new ObjectPart.PrimitiveShape
            {
                DecodedParams = m_Shape
            };
            if (!m_PhysicsShapeManager.TryGetConvexShape(m_PhysicsShapeType, ps, out physicsShapeRef))
            {
                m_Log.Error("Could not generate physics hull shape");
                return false;
            }

            PhysicsConvexShape convexShape = physicsShapeRef;

            int hullidx = 0;
            foreach(PhysicsConvexShape.ConvexHull hull in convexShape.Hulls)
            {
                m_Log.InfoFormat("Hull {0}: Generated vertices: {1}", hullidx, hull.Vertices.Count);
                m_Log.InfoFormat("Hull {0}: Generated triangles: {1}", hullidx, hull.Triangles.Count / 3);
                ++hullidx;
            }
            m_Log.InfoFormat("Generated hulls: {0}", hullidx);

            /* write a blender .raw */
            ((PhysicsConvexShape)physicsShapeRef).DumpToBlenderRaw(m_OutputFileName);
            return true;
        }

        public void Cleanup()
        {
        }
    }
}

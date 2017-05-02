﻿// SilverSim is distributed under the terms of the
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
    public class PrimToMesh : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        AssetServiceInterface m_AssetService;
        ObjectPart.PrimitiveShape.Decoded m_Shape = new ObjectPart.PrimitiveShape.Decoded();
        string m_OutputFileName;

        public PrimToMesh()
        {
        }

        public void Startup(ConfigurationLoader loader)
        {
            IConfig config = loader.Config.Configs[GetType().FullName];
            m_AssetService = loader.GetService<AssetServiceInterface>(config.GetString("AssetService"));

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

            if(config.Contains("SculptMapFile"))
            {
                if (!config.Contains("SculptMapID"))
                {
                    throw new ConfigurationLoader.ConfigurationErrorException("SculptMap parameter not present");
                }
                byte[] data;
                using (FileStream fs = new FileStream(config.GetString("SculptMapFile"), FileMode.Open))
                {
                    int fileLength = (int)fs.Length;
                    data = new byte[fileLength];
                    if(fileLength != fs.Read(data, 0, fileLength))
                    {
                        throw new ConfigurationLoader.ConfigurationErrorException("Failed to load file");
                    }
                }
                AssetData assetdata = new AssetData();
                assetdata.Data = data;
                assetdata.Type = m_Shape.SculptType == PrimitiveSculptType.Mesh ? AssetType.Mesh : AssetType.Texture;
                assetdata.ID = m_Shape.SculptMap;
                assetdata.Name = "PrimToMesh imported";
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

            m_OutputFileName = config.GetString("OutputFilename");
        }

        public void Setup()
        {
        }

        string VertexToString(Vector3 v)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", v.X, v.Y, v.Z);
        }

        public bool Run()
        {
            MeshLOD mesh = m_Shape.ToMesh(m_AssetService);

            /* write a blender .raw */
            using (StreamWriter w = new StreamWriter(m_OutputFileName))
            {
                foreach(Triangle tri in mesh.Triangles)
                {
                    w.WriteLine("{0} {1} {2}",
                        VertexToString(mesh.Vertices[tri.Vertex1]),
                        VertexToString(mesh.Vertices[tri.Vertex2]),
                        VertexToString(mesh.Vertices[tri.Vertex3]));
                }
            }
            return true;
        }

        public void Cleanup()
        {
        }
    }
}
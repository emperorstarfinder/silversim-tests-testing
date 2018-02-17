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
using SilverSim.ServiceInterfaces.Asset;
using SilverSim.Tests.Extensions;
using SilverSim.Types;
using SilverSim.Types.Asset.Format.Mesh;
using SilverSim.Types.Primitive;
using System;
using System.Reflection;

namespace SilverSim.Tests.Assets
{
    public sealed class FuzzyPrimToPhysicsShapes : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        PhysicsShapeManager m_PhysicsShapeManager;
        AssetServiceInterface m_AssetService;

        public void Startup(ConfigurationLoader loader)
        {
            IConfig config = loader.Config.Configs[GetType().FullName];
            m_AssetService = loader.GetService<AssetServiceInterface>(config.GetString("AssetService", "AssetService"));
            m_PhysicsShapeManager = loader.GetService<PhysicsShapeManager>(config.GetString("PhysicsShapeManager", "PhysicsShapeManager"));

        }

        public void Setup()
        {
        }

        private void DumpParams(ObjectPart.PrimitiveShape.Decoded data)
        {
            m_Log.InfoFormat("ShapeType: {0}", data.ShapeType);
            m_Log.InfoFormat("SculptType: {0}", data.SculptType);
            m_Log.InfoFormat("PathBegin: {0}", data.PathBegin);
            m_Log.InfoFormat("PathCurve: {0}", data.PathCurve);
            m_Log.InfoFormat("PathEnd: {0}", data.PathEnd);
            m_Log.InfoFormat("HoleShape: {0}", data.HoleShape);
            m_Log.InfoFormat("Taper: {0}", data.Taper);
            m_Log.InfoFormat("PathScale: {0}", data.PathScale);
            m_Log.InfoFormat("ProfileBegin: {0}", data.ProfileBegin);
            m_Log.InfoFormat("ProfileEnd: {0}", data.ProfileEnd);
            m_Log.InfoFormat("ProfileHollow: {0}", data.ProfileHollow);
            m_Log.InfoFormat("ProfileShape: {0}", data.ProfileShape);
            m_Log.InfoFormat("SculptMap: {0}", data.SculptMap);
            m_Log.InfoFormat("Skew: {0}", data.Skew);
            m_Log.InfoFormat("TwistBegin: {0}", data.TwistBegin);
            m_Log.InfoFormat("TwistEnd: {0}", data.TwistEnd);
            m_Log.InfoFormat("TopShear: {0}", data.TopShear);
            m_Log.InfoFormat("Revolutions: {0}", data.Revolutions);
            m_Log.InfoFormat("RadiusOffset: {0}", data.RadiusOffset);
            m_Log.InfoFormat("IsSculptInverted: {0}", data.IsSculptInverted);
            m_Log.InfoFormat("IsSculptMirrored: {0}", data.IsSculptMirrored);
            m_Log.InfoFormat("IsHollow: {0}", data.IsHollow);
            m_Log.InfoFormat("IsOpen: {0}", data.IsOpen);
        }

        public bool Run()
        {
            PhysicsShapeReference physicsShapeRef;
            bool success = true;
            foreach (PrimitiveShapeType shapeType in new PrimitiveShapeType[] { PrimitiveShapeType.Box, PrimitiveShapeType.Cylinder, PrimitiveShapeType.Prism, PrimitiveShapeType.Ring, PrimitiveShapeType.Sphere, PrimitiveShapeType.Torus, PrimitiveShapeType.Tube })
            {
                foreach (PrimitiveProfileShape profileShape in new PrimitiveProfileShape[] { PrimitiveProfileShape.Circle, PrimitiveProfileShape.EquilateralTriangle, PrimitiveProfileShape.HalfCircle, PrimitiveProfileShape.IsometricTriangle, PrimitiveProfileShape.RightTriangle, PrimitiveProfileShape.Square })
                {
                    foreach (PrimitiveProfileHollowShape hollowShape in new PrimitiveProfileHollowShape[] { PrimitiveProfileHollowShape.Circle, PrimitiveProfileHollowShape.Same, PrimitiveProfileHollowShape.Square, PrimitiveProfileHollowShape.Triangle })
                    {
                        foreach (PrimitivePhysicsShapeType physicsShapeType in new PrimitivePhysicsShapeType[] { PrimitivePhysicsShapeType.Convex, PrimitivePhysicsShapeType.Prim })
                        {
                            foreach (PrimitiveExtrusion pathCurve in new PrimitiveExtrusion[] { PrimitiveExtrusion.Curve1, PrimitiveExtrusion.Curve2, PrimitiveExtrusion.Default, PrimitiveExtrusion.Straight })
                            {
                                for (double pathbegin = 0; pathbegin < 1; pathbegin += 0.1)
                                {
                                    for (double pathend = 0; pathend < 1; pathend += 0.1)
                                    {
                                        for (double profilebegin = 0; profilebegin < 1; profilebegin += 0.1)
                                        {
                                            for (double profileend = 0; profileend < 1; profileend += 0.1)
                                            {
                                                for (double revolutions = 1; revolutions < 4; revolutions += 0.5)
                                                {
                                                    for (double twistbegin = -1; twistbegin < 1; twistbegin += 0.1)
                                                    {
                                                        for (double twistend = -1; twistend < 1; twistend += 0.1)
                                                        {
                                                            /*
public double RadiusOffset;
    */
                                                            for (double hollow = 0; hollow < 0.90; hollow += 0.1)
                                                            {
                                                                for (double skew = -0.9; skew < 0.9; skew += 0.1)
                                                                {
                                                                    for (double topshearx = -1; topshearx < 1; topshearx += 0.1)
                                                                    {
                                                                        for (double topsheary = -1; topsheary < 1; topsheary += 0.1)
                                                                        {
                                                                            for (double taperx = -1; taperx < 1; taperx += 0.1)
                                                                            {
                                                                                for (double tapery = -1; tapery < 1; tapery += 0.1)
                                                                                {
                                                                                    for (double pathscalex = 0; pathscalex < 1; pathscalex += 0.1)
                                                                                    {
                                                                                        for (double pathscaley = 0; pathscaley < 1; pathscaley += 0.1)
                                                                                        {
                                                                                            var shape = new ObjectPart.PrimitiveShape.Decoded();
                                                                                            shape.ProfileShape = profileShape;
                                                                                            shape.ShapeType = shapeType;
                                                                                            shape.HoleShape = hollowShape;
                                                                                            shape.ProfileHollow = hollow;
                                                                                            shape.PathCurve = pathCurve;
                                                                                            shape.Revolutions = revolutions;
                                                                                            shape.PathBegin = pathbegin;
                                                                                            shape.PathEnd = pathend;
                                                                                            shape.TwistBegin = twistbegin;
                                                                                            shape.TwistEnd = twistend;
                                                                                            shape.ProfileBegin = profilebegin;
                                                                                            shape.ProfileEnd = profileend;
                                                                                            shape.TopShear.X = topshearx;
                                                                                            shape.TopShear.Y = topsheary;
                                                                                            shape.Taper.X = taperx;
                                                                                            shape.Taper.Y = tapery;
                                                                                            shape.PathScale.X = pathscalex;
                                                                                            shape.PathScale.Y = pathscaley;
                                                                                            shape.Skew = skew;

                                                                                            var ps = new ObjectPart.PrimitiveShape
                                                                                            {
                                                                                                DecodedParams = shape
                                                                                            };

                                                                                            m_Log.Info("---- Transformed to physics shape manager accepted format ----");
                                                                                            m_Log.InfoFormat("PhysicsShapeType: {0}/{1}", physicsShapeType, shapeType);
                                                                                            m_Log.Info($"Serialized data {physicsShapeType.ToString()}/{ps.Serialization.ToHexString()}");
                                                                                            try
                                                                                            {
                                                                                                if (!m_PhysicsShapeManager.TryGetConvexShape(physicsShapeType, ps, out physicsShapeRef))
                                                                                                {
                                                                                                    DumpParams(shape);
                                                                                                    m_Log.Error("Could not generate physics hull shape");
                                                                                                    success = false;
                                                                                                    continue;
                                                                                                }
                                                                                            }
                                                                                            catch(Exception e)
                                                                                            {
                                                                                                DumpParams(shape);
                                                                                                m_Log.Error("Could not generate physics hull shape", e);
                                                                                                continue;
                                                                                            }

                                                                                            PhysicsConvexShape convexShape = physicsShapeRef;

                                                                                            int hullidx = 0;
                                                                                            foreach (PhysicsConvexShape.ConvexHull hull in convexShape.Hulls)
                                                                                            {
                                                                                                m_Log.InfoFormat("Hull {0}: Generated vertices: {1}", hullidx, hull.Vertices.Count);
                                                                                                m_Log.InfoFormat("Hull {0}: Generated triangles: {1}", hullidx, hull.Triangles.Count / 3);
                                                                                                ++hullidx;
                                                                                            }
                                                                                            m_Log.InfoFormat("Generated hulls: {0}", hullidx);
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                /* write a blender .raw */
                                //                ((PhysicsConvexShape)physicsShapeRef).DumpToBlenderRaw(m_OutputFileName);
                            }
                        }
                    }
                }
            }

            return success;
        }

        public void Cleanup()
        {
        }
    }
}

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
using SilverSim.Scene.Physics.Common.Vehicle;
using SilverSim.Scene.Types.Object;
using SilverSim.Scene.Types.Physics;
using SilverSim.Scene.Types.Physics.Vehicle;
using SilverSim.Scene.Types.Scene;
using SilverSim.Scene.Types.Script;
using SilverSim.Scene.Types.Script.Events;
using SilverSim.Scripting.Lsl;
using SilverSim.Types;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace SilverSim.Tests.Lsl
{
    [LSLImplementation]
    [ScriptApiName("VehicleTestExtensions")]
    [PluginName("VehicleTestExtensions")]
    public class TestVehicleApi : IScriptApi, IPlugin
    {
        public class VehicleObject : IObject
        {
            public ILocalIDAccessor LocalID => null;

            public UUID ID => UUID.Zero;

            public double Mass { get; set; }

            public string Name { get; set; }
            public UGUI Owner { get; set; }
            public UGI Group { get; set; }
            public string Description { get; set; }
            public Vector3 Position { get; set; }
            public Vector3 Velocity { get; set; }
            public Vector3 AngularVelocity { get; set; }
            public Vector3 GlobalPosition { get; set; }
            public Vector3 LocalPosition { get; set; }
            public Vector3 Acceleration { get; set; }
            public Vector3 AngularAcceleration { get; set; }
            public Quaternion GlobalRotation { get; set; }
            public Quaternion LocalRotation { get; set; }
            public Quaternion Rotation { get; set; }
            public Vector3 Size { get; set; }
            public double PhysicsGravityMultiplier { get; set; }
            public PathfindingType PathfindingType { get; set; }
            public double WalkableCoefficientAvatar { get; set; }
            public double WalkableCoefficientA { get; set; }
            public double WalkableCoefficientB { get; set; }
            public double WalkableCoefficientC { get; set; }
            public double WalkableCoefficientD { get; set; }

            public byte[] TerseData { get; }

            public DetectedTypeFlags DetectedType { get; }

#pragma warning disable CS0067
            public event Action<IObject> OnPositionChange;
#pragma warning restore

            public void GetBoundingBox(out BoundingBox box) => box = new BoundingBox
            {
                CenterOffset = Vector3.Zero,
                Size = Vector3.One
            };

            public void GetObjectDetails(List<IValue>.Enumerator enumerator, AnArray paramList)
            {
                /* intentionally left empty */
            }

            public void GetPrimitiveParams(List<IValue>.Enumerator enumerator, AnArray paramList)
            {
                /* intentionally left empty */
            }

            public void GetPrimitiveParams(List<IValue>.Enumerator enumerator, AnArray paramList, CultureInfo cultureInfo)
            {
                /* intentionally left empty */
            }

            public bool IsInScene(SceneInterface scene) => false;

            public void MoveToTarget(Vector3 target, double tau, UUID notifyPrimId, UUID notifyItemId)
            {
                /* intentionally left empty */
            }

            public void PostEvent(IScriptEvent ev)
            {
                /* intentionally left empty */
            }

            public void SetPrimitiveParams(AnArray.MarkEnumerator enumerator)
            {
                /* intentionally left empty */
            }

            public void StopMoveToTarget()
            {
                /* intentionally left empty */
            }
        }

        [APIExtension("VehicleTest", "vehicleinstance")]
        [APIDisplayName("vehicleinstance")]
        [APIIsVariableType]
        [Serializable]
        [APIAccessibleMembers]
        public class VehicleInstance
        {
            [NonSerialized]
            private readonly VehicleParams m_VehicleParams;
            [NonSerialized]
            private readonly VehicleMotor m_VehicleMotor;
            [NonSerialized]
            private readonly PhysicsStateData m_PhysicsState;

            private void Init()
            {
                m_VehicleParams.VehicleType = VehicleType.Balloon;
                m_VehicleParams[VehicleFloatParamId.AngularDeflectionEfficiency] = 0;
                m_VehicleParams[VehicleFloatParamId.BankingEfficiency] = 0;
                m_VehicleParams[VehicleFloatParamId.Buoyancy] = 1;
                m_VehicleParams[VehicleFloatParamId.HoverEfficiency] = 0;
                m_VehicleParams[VehicleFloatParamId.LinearDeflectionEfficiency] = 0;
                m_VehicleParams[VehicleFloatParamId.VerticalAttractionEfficiency] = 0;
            }

            public VehicleInstance()
            {
                GravityConstant = 9.81;
                Mass = 1;
                m_VehicleParams = new VehicleParams(new ObjectPart());
                m_PhysicsState = new PhysicsStateData(new VehicleObject(), UUID.Zero);
                m_VehicleMotor = m_VehicleParams.GetMotor();
                Init();
            }

            public VehicleInstance(UUID sceneID)
            {
                GravityConstant = 9.81;
                Mass = 1;
                m_PhysicsState = m_PhysicsState = new PhysicsStateData(new VehicleObject(), sceneID);
                m_VehicleMotor = m_VehicleParams.GetMotor();
                Init();
            }

            public void Process(ScriptInstance instance, double dt)
            {
                m_VehicleMotor.Process(dt, m_PhysicsState, instance.Part.ObjectGroup.Scene, Mass, GravityConstant);
            }

            public double Mass { get; set; }
            public double GravityConstant { get; set; }

            public Vector3 LinearForce => m_VehicleMotor.LinearForce / Mass;
            public Vector3 AngularTorque => m_VehicleMotor.AngularTorque;

            public Vector3 Position
            {
                get
                {
                    return m_PhysicsState.Position;
                }
                set
                {
                    m_PhysicsState.Position = value;
                }
            }

            public Vector3 Velocity
            {
                get
                {
                    return m_PhysicsState.Velocity;
                }
                set
                {
                    m_PhysicsState.Velocity = value;
                }
            }

            public Quaternion Rotation
            {
                get
                {
                    return m_PhysicsState.Rotation;
                }
                set
                {
                    m_PhysicsState.Rotation = value.Normalize();
                }
            }

            public Vector3 AngularVelocity
            {
                get
                {
                    return m_PhysicsState.AngularVelocity;
                }
                set
                {
                    m_PhysicsState.AngularVelocity = value;
                }
            }

            public Vector3 Acceleration
            {
                get
                {
                    return m_PhysicsState.Acceleration;
                }
                set
                {
                    m_PhysicsState.Acceleration = value;
                }
            }

            public Vector3 AngularAcceleration
            {
                get
                {
                    return m_PhysicsState.AngularAcceleration;
                }
                set
                {
                    m_PhysicsState.AngularAcceleration = value;
                }
            }

            public Quaternion CameraRotation
            {
                get
                {
                    return m_PhysicsState.CameraRotation;
                }
                set
                {
                    m_PhysicsState.CameraRotation = value.Normalize();
                }
            }

            public int IsCameraDataValid
            {
                get
                {
                    return m_PhysicsState.IsCameraDataValid.ToLSLBoolean();
                }
                set
                {
                    m_PhysicsState.IsCameraDataValid = value != 0;
                }
            }

            public int IsAgentInMouselook
            {
                get
                {
                    return m_PhysicsState.IsAgentInMouselook.ToLSLBoolean();
                }
                set
                {
                    m_PhysicsState.IsAgentInMouselook = value != 0;
                }
            }

            public int Type
            {
                get
                {
                    return (int)m_VehicleParams.VehicleType;
                }
                set
                {
                    switch(value)
                    {
                        case 0:
                            m_VehicleParams.VehicleType = VehicleType.None;
                            break;

                        case 1:
                            m_VehicleParams.VehicleType = VehicleType.Sled;
                            break;

                        case 2:
                            m_VehicleParams.VehicleType = VehicleType.Car;
                            break;

                        case 3:
                            m_VehicleParams.VehicleType = VehicleType.Boat;
                            break;

                        case 4:
                            m_VehicleParams.VehicleType = VehicleType.Airplane;
                            break;

                        case 5:
                            m_VehicleParams.VehicleType = VehicleType.Balloon;
                            break;

                        default:
                            break;
                    }
                }
            }

            public Quaternion ReferenceFrame
            {
                get
                {
                    return m_VehicleParams[VehicleRotationParamId.ReferenceFrame];
                }
                set
                {
                    m_VehicleParams[VehicleRotationParamId.ReferenceFrame] = value;
                }
            }

            #region linear effects
            public Vector3 LinearFrictionTimescale
            {
                get
                {
                    return m_VehicleParams[VehicleVectorParamId.LinearFrictionTimescale];
                }
                set
                {
                    m_VehicleParams[VehicleVectorParamId.LinearFrictionTimescale] = value;
                }
            }

            public Vector3 LinearMotorDecayTimescale
            {
                get
                {
                    return m_VehicleParams[VehicleVectorParamId.LinearMotorDecayTimescale];
                }
                set
                {
                    m_VehicleParams[VehicleVectorParamId.LinearMotorDecayTimescale] = value;
                }
            }

            public Vector3 LinearMotorTimescale
            {
                get
                {
                    return m_VehicleParams[VehicleVectorParamId.LinearMotorTimescale];
                }
                set
                {
                    m_VehicleParams[VehicleVectorParamId.LinearMotorTimescale] = value;
                }
            }

            public Vector3 LinearMotorDirection
            {
                get
                {
                    return m_VehicleParams[VehicleVectorParamId.LinearMotorDirection];
                }
                set
                {
                    m_VehicleParams[VehicleVectorParamId.LinearMotorDirection] = value;
                }
            }

            public Vector3 LinearMotorOffset
            {
                get
                {
                    return m_VehicleParams[VehicleVectorParamId.LinearMotorOffset];
                }
                set
                {
                    m_VehicleParams[VehicleVectorParamId.LinearMotorOffset] = value;
                }
            }

            public double LinearDeflectionEfficiency
            {
                get
                {
                    return m_VehicleParams[VehicleFloatParamId.LinearDeflectionEfficiency];
                }
                set
                {
                    m_VehicleParams[VehicleFloatParamId.LinearDeflectionEfficiency] = value;
                }
            }

            public double LinearDeflectionTimescale
            {
                get
                {
                    return m_VehicleParams[VehicleFloatParamId.LinearDeflectionTimescale];
                }
                set
                {
                    m_VehicleParams[VehicleFloatParamId.LinearDeflectionTimescale] = value;
                }
            }
            #endregion

            #region angular effects
            public Vector3 AngularFrictionTimescale
            {
                get
                {
                    return m_VehicleParams[VehicleVectorParamId.AngularFrictionTimescale];
                }
                set
                {
                    m_VehicleParams[VehicleVectorParamId.AngularFrictionTimescale] = value;
                }
            }

            public Vector3 AngularMotorDirection
            {
                get
                {
                    return m_VehicleParams[VehicleVectorParamId.AngularMotorDirection];
                }
                set
                {
                    m_VehicleParams[VehicleVectorParamId.AngularMotorDirection] = value;
                }
            }

            public Vector3 AngularMotorDecayTimescale
            {
                get
                {
                    return m_VehicleParams[VehicleVectorParamId.AngularMotorDecayTimescale];
                }
                set
                {
                    m_VehicleParams[VehicleVectorParamId.AngularMotorDecayTimescale] = value;
                }
            }

            public Vector3 AngularMotorTimescale
            {
                get
                {
                    return m_VehicleParams[VehicleVectorParamId.AngularMotorTimescale];
                }
                set
                {
                    m_VehicleParams[VehicleVectorParamId.AngularMotorTimescale] = value;
                }
            }

            public double AngularDeflectionEfficiency
            {
                get
                {
                    return m_VehicleParams[VehicleFloatParamId.AngularDeflectionEfficiency];
                }
                set
                {
                    m_VehicleParams[VehicleFloatParamId.AngularDeflectionEfficiency] = value;
                }
            }

            public double AngularDeflectionTimescale
            {
                get
                {
                    return m_VehicleParams[VehicleFloatParamId.AngularDeflectionTimescale];
                }
                set
                {
                    m_VehicleParams[VehicleFloatParamId.AngularDeflectionTimescale] = value;
                }
            }
            #endregion

            #region banking
            public double BankingEfficiency
            {
                get
                {
                    return m_VehicleParams[VehicleFloatParamId.BankingEfficiency];
                }
                set
                {
                    m_VehicleParams[VehicleFloatParamId.BankingEfficiency] = value;
                }
            }

            public double BankingMix
            {
                get
                {
                    return m_VehicleParams[VehicleFloatParamId.BankingMix];
                }
                set
                {
                    m_VehicleParams[VehicleFloatParamId.BankingMix] = value;
                }
            }

            public double BankingTimescale
            {
                get
                {
                    return m_VehicleParams[VehicleFloatParamId.BankingTimescale];
                }
                set
                {
                    m_VehicleParams[VehicleFloatParamId.BankingTimescale] = value;
                }
            }

            public double BankingAzimuth
            {
                get
                {
                    return m_VehicleParams[VehicleFloatParamId.BankingAzimuth];
                }
                set
                {
                    m_VehicleParams[VehicleFloatParamId.BankingAzimuth] = value;
                }
            }

            public double InvertedBankingModifier
            {
                get
                {
                    return m_VehicleParams[VehicleFloatParamId.InvertedBankingModifier];
                }
                set
                {
                    m_VehicleParams[VehicleFloatParamId.InvertedBankingModifier] = value;
                }
            }
            #endregion

            #region vertical attractor
            public double VerticalAttractionEfficency
            {
                get
                {
                    return m_VehicleParams[VehicleFloatParamId.VerticalAttractionEfficiency];
                }
                set
                {
                    m_VehicleParams[VehicleFloatParamId.VerticalAttractionEfficiency] = value;
                }
            }

            public double VerticalAttractionTimescale
            {
                get
                {
                    return m_VehicleParams[VehicleFloatParamId.VerticalAttractionTimescale];
                }
                set
                {
                    m_VehicleParams[VehicleFloatParamId.VerticalAttractionTimescale] = value;
                }
            }
            #endregion

            #region buoyancy
            public double Buoyancy
            {
                get
                {
                    return m_VehicleParams[VehicleFloatParamId.Buoyancy];
                }
                set
                {
                    m_VehicleParams[VehicleFloatParamId.Buoyancy] = value;
                }
            }
            #endregion

            #region hover
            public double HoverHeight
            {
                get
                {
                    return m_VehicleParams[VehicleFloatParamId.HoverHeight];
                }
                set
                {
                    m_VehicleParams[VehicleFloatParamId.HoverHeight] = value;
                }
            }

            public double HoverEfficiency
            {
                get
                {
                    return m_VehicleParams[VehicleFloatParamId.HoverEfficiency];
                }
                set
                {
                    m_VehicleParams[VehicleFloatParamId.HoverEfficiency] = value;
                }
            }

            public double HoverTimescale
            {
                get
                {
                    return m_VehicleParams[VehicleFloatParamId.HoverTimescale];
                }
                set
                {
                    m_VehicleParams[VehicleFloatParamId.HoverTimescale] = value;
                }
            }
            #endregion

            #region Mouselook functions
            public double MouselookAzimuth
            {
                get
                {
                    return m_VehicleParams[VehicleFloatParamId.MouselookAzimuth];
                }
                set
                {
                    m_VehicleParams[VehicleFloatParamId.MouselookAzimuth] = value;
                }
            }

            public double MouselookAltitude
            {
                get
                {
                    return m_VehicleParams[VehicleFloatParamId.MouselookAltitude];
                }
                set
                {
                    m_VehicleParams[VehicleFloatParamId.MouselookAltitude] = value;
                }
            }
            #endregion

            #region disable features
            public double DisableMotorsAbove
            {
                get
                {
                    return m_VehicleParams[VehicleFloatParamId.DisableMotorsAbove];
                }
                set
                {
                    m_VehicleParams[VehicleFloatParamId.DisableMotorsAbove] = value;
                }
            }

            public double DisableMotorsAfter
            {
                get
                {
                    return m_VehicleParams[VehicleFloatParamId.DisableMotorsAfter];
                }
                set
                {
                    m_VehicleParams[VehicleFloatParamId.DisableMotorsAfter] = value;
                }
            }
            #endregion

            #region Wind effects
            public Vector3 LinearWindEfficiency
            {
                get
                {
                    return m_VehicleParams[VehicleVectorParamId.LinearWindEfficiency];
                }
                set
                {
                    m_VehicleParams[VehicleVectorParamId.LinearWindEfficiency] = value;
                }
            }

            public Vector3 AngularWindEfficiency
            {
                get
                {
                    return m_VehicleParams[VehicleVectorParamId.AngularWindEfficiency];
                }
                set
                {
                    m_VehicleParams[VehicleVectorParamId.AngularWindEfficiency] = value;
                }
            }
            #endregion
        }

        [APIExtension("VehicleTest", APIUseAsEnum.Getter, "VehicleInstance")]
        public VehicleInstance GetInstance(ScriptInstance instance)
        {
            lock (instance)
            {
                return new VehicleInstance(instance.Part.ObjectGroup.Scene.ID);
            }
        }

        [APIExtension("VehicleTest", APIUseAsEnum.MemberFunction, "Process")]
        public void Process(ScriptInstance instance, VehicleInstance vehicle, double deltatime)
        {
            lock (instance)
            {
                vehicle.Process(instance, deltatime);
                Vector3 linearForce = vehicle.LinearForce * deltatime;
                linearForce.Z -= vehicle.GravityConstant * deltatime;
                vehicle.Velocity += linearForce * deltatime;
                vehicle.Position += vehicle.Velocity * deltatime;
                vehicle.AngularVelocity += vehicle.AngularTorque * deltatime;
                vehicle.Rotation *= Quaternion.CreateFromEulers(vehicle.AngularVelocity * deltatime);
            }
        }

        public void Startup(ConfigurationLoader loader)
        {
            /* intentionally left empty */
        }
    }
}

using AddComponentMenu          = UnityEngine.AddComponentMenu;
using Assert                    = UnityEngine.Assertions.Assert;
using DisallowMultipleComponent = UnityEngine.DisallowMultipleComponent;
using ForceMode                 = UnityEngine.ForceMode;
using GameObject                = UnityEngine.GameObject;
using Mathf                     = UnityEngine.Mathf;
using MonoBehaviour             = UnityEngine.MonoBehaviour;
using Quaternion                = UnityEngine.Quaternion;
using Range                     = UnityEngine.RangeAttribute;
using Serializable              = System.SerializableAttribute;
using SerializeField            = UnityEngine.SerializeField;
using Time                      = UnityEngine.Time;
using Tooltip                   = UnityEngine.TooltipAttribute;
using Vector3                   = UnityEngine.Vector3;


namespace PSI
{
    /// <summary>
    /// A component which dictates how the GameObject should be handled by the Physics system.
    /// </summary>
    [Serializable, DisallowMultipleComponent, AddComponentMenu ("PSI/Rigidbody")]
    public sealed class Rigidbody : MonoBehaviour
    {
        #region General data

        [SerializeField, Tooltip ("The physical mass of the object, represented in kilograms.")]
        float m_mass = 1f;

        [SerializeField, Tooltip ("The energy threshold at which the object should come to a resting point.")]
        float m_sleepThreshold = 0.005f;

        /// <summary>
        /// Determines whether gravity should be applied.
        /// </summary>
        [Tooltip ("Determines whether gravity should be applied.")]
        public bool simulateGravity = true;

        /// <summary>
        /// Gets or sets the physical mass of the object, represented in kilograms-metres/second. Setting this will not
        /// reset the current inertia tensor value.
        /// </summary>
        /// <value>Must be larger than zero.</value>
        public float mass
        {
            get { return m_mass; }
            set
            {
                // Ensure we don't have zero mass.
                Assert.IsTrue (value > 0f, "Rigidbody mass must be larger than zero.");
                m_mass = Mathf.Max (0.0001f, value);
            }
        }

        /// <summary>
        /// Gets or sets the energy threshold at which the object should come to a resting point.
        /// </summary>
        /// <value>Must be a positive value.</value>
        public float sleepThreshold
        {
            get { return m_sleepThreshold; }
            set
            {
                // Must be a positive value.
                Assert.IsTrue (value >= 0f, "Rigibody sleep threshold must be a positive value.");
                m_sleepThreshold = Mathf.Max (0f, value);
            }
        }

        #endregion

        #region Linear motion

        [SerializeField, Tooltip ("A uniform drag co-efficient, determines how much damping will be applied.")]
        float m_drag = 0f;

        /// <summary>
        /// The linear momentum of the object represented in kilogram-metres/second.
        /// </summary>
        [System.NonSerialized]
        public Vector3 momentum = Vector3.zero;

        /// <summary>
        /// Accumulated linear force which should be applied taking into account the mass of an object.
        /// </summary>
        Vector3 m_linearForce = Vector3.zero;

        /// <summary>
        /// Accumulated linear acceleration which disregards an objects mass, waiting to be applied.
        /// </summary>
        Vector3 m_linearAcceleration = Vector3.zero;

        /// <summary>
        /// Gets or sets the position of the Rigidbody.
        /// </summary>
        /// <value>The desired position of the object in the scene.</value>
        public Vector3 position
        {
            get { return transform.position; }
            set { transform.position = value; }
        }

        /// <summary>
        /// Gets or sets the uniform drag co-efficient. The higher the value the more damping will be applied.
        /// </summary>
        /// <value>Must be a positive value.</value>
        public float drag
        {
            get { return m_drag; }
            set
            {
                // Must be a positive value.
                Assert.IsTrue (value >= 0f, "Rigidbody drag must be a positive value.");
                m_drag = Mathf.Max (0f, value);
            }
        }

        /// <summary>
        /// Gets or sets the velocity at which the object is moving in metres/s. Setting this will recalcuate the 
        /// momentum.
        /// </summary>
        /// <value>The desired velocity of the object.</value>
        public Vector3 velocity
        {
            get { return momentum / mass; }
            set { momentum = value * mass; }
        }

        /// <summary>
        /// Gets the amount of force being applied to the object before the next frame, mass will be taken into account.
        /// </summary>
        public Vector3 linearForce
        {
            get { return m_linearForce; }
        }

        /// <summary>
        /// Gets the amount of mass-independant acceleration that will be applied to the object before the next frame.
        /// </summary>
        public Vector3 linearAcceleration
        {
            get { return m_linearAcceleration; }
        }

        #endregion

        #region Rotational motion

        [SerializeField, Tooltip ("A uniform angular drag co-efficient, determines how much damping will be applied.")]
        float m_angularDrag = 0.005f;

        /// <summary>
        /// An offset representing the centre of mass. This effects how angular velocity is applied.
        /// </summary>
        [Tooltip ("An offset representing the centre of mass. This effects how angular velocity is applied.")]
        public Vector3 centreOfMass = Vector3.zero;

        /// <summary>
        /// The inertia tensor for the Rigidbody. This allows for the conversion between angular momentum and angular
        /// velocity.
        /// </summary>
        [System.NonSerialized]
        public Vector3 inertiaTensor = Vector3.zero;

        /// <summary>
        /// The angular momentum of the object. This controls the rate at which the object is rotating.
        /// </summary>
        /// <value>The desired angular momentum of the object.</value>
        [System.NonSerialized]
        public Vector3 angularMomentum = Vector3.zero;

        /// <summary>
        /// Accumulated torque that should be applied to the object before the next frame.
        /// </summary>
        Vector3 m_angularTorque = Vector3.zero;

        /// <summary>
        /// Accumulated rotational acceleration that should be applied to the object before the next frame.
        /// </summary>
        Vector3 m_angularAcceleration = Vector3.zero;

        /// <summary>
        /// Gets or sets the rotation of the Rigidbody.
        /// </summary>
        /// <value>The desired rotation of the object in the scene.</value>
        public Quaternion rotation
        {
            get { return transform.rotation; }
            set { transform.rotation = value; }
        }

        /// <summary>
        /// Gets or set the uniform angular drag co-efficient. The higher the value the more damping will be applied.
        /// </summary>
        /// <value>Must be a positive value.</value>
        public float angularDrag
        {
            get { return m_angularDrag; }
            set
            {
                // Must be a positive value.
                Assert.IsTrue (value >= 0f, "Rigidbody angular drag must be a positive value.");
                m_angularDrag = Mathf.Max (0f, value);
            }
        }

        /// <summary>
        /// Gets the angular velocity of the object. Setting this will recalculate angular momentum.
        /// </summary>
        /// <value>The desired angular velocity of the object.</value>
        public Vector3 angularVelocity
        {
            get { return Maths.IntegrateMomentum (angularMomentum, m_mass); }
            set { angularMomentum = Maths.DeriveMomentum (value, m_mass); }
        }

        /// <summary>
        /// Gets the derivative of the objects rotation, giving a "spin" factor which can be applied similarly to how
        /// velocity is applied to position.
        /// </summary>
        public Quaternion spin
        {
            get { return Maths.DeriveSpin (angularVelocity, rotation); }
        }

        /// <summary>
        /// Gets the amount of torque that should be applied to the object before the next frame.
        /// </summary>
        public Vector3 angularTorque
        {
            get { return m_angularTorque; }
        }

        /// <summary>
        /// Gets the amount of rotational acceleration that should be applied to the object before the next frame.
        /// </summary>
        public Vector3 angularAcceleration
        {
            get { return m_angularAcceleration; }
        }

        #endregion

        #region System data

        /// <summary>
        /// The Physics system the object is currently registered with.
        /// </summary>
        Physics m_system = null;

        /// <summary>
        /// Contains every Collider attached to the Rigidbody.
        /// </summary>
        MappedPopList<Collider> m_colliders = new MappedPopList<Collider> (4, 4);

        #endregion

        #region Component state management

        /// <summary>
        /// Ensures member data which needs to be clamped is successfully clamped.
        /// </summary>
        void OnValidate()
        {
            // Allow the properties to set limits.
            mass            = m_mass;
            sleepThreshold  = m_sleepThreshold;
            drag            = m_drag;
            angularDrag     = m_angularDrag;
        }

        /// <summary>
        /// Calculates the inertia tensor and centre of mass of the object. If a Rigidbody doesn't have any colliders
        /// attached then the inertia tensor will be spherical by default.
        /// </summary>
        void Awake()
        {
            ResetInertiaTensor();
        }

        /// <summary>
        /// Registers the object with a Physics system either the default system or one which exists within the level.
        /// </summary>
        void OnEnable()
        {
            // Ensure we have a physics system to register with.
            var m_system = Physics.FindSystem();
            Assert.IsNotNull (m_system, "Couldn't find a Physics system to register with.");

            m_system.Register (this);
        }

        /// <summary>
        /// Deregisters the object from the Physics system it was initially registered to.
        /// </summary>
        void OnDisable()
        {
            if (m_system && this)
            {
                m_system.Deregister (this);
            }
        }

        /// <summary>
        /// Attaches a Collider to the Rigidbody, this causes a reset of the inertia tensor.
        /// </summary>
        /// <param name="collider">The collider to be attached.</param>
        public void Attach (Collider collider)
        {
            m_colliders.Add (collider);
            ResetInertiaTensor();
            ResetCentreOfMass();
        }

        /// <summary>
        /// Detaches a Collider from the Rigidbody, this causes a reset of the inertia tensor.
        /// </summary>
        /// <param name="collider">The collider to be detached.</param>
        public void Detach (Collider collider)
        {
            m_colliders.Remove (collider);
            ResetInertiaTensor();
        }

        /// <summary>
        /// Computes a new value for the inertia tensor based on the current mass and attached Collider objects.
        /// </summary>
        public void ResetInertiaTensor()
        {
            // Just use the moments of inertia of the first collider until compound colliders are supported.
            if (m_colliders.Count > 0)
            {
                m_colliders[0].UpdateRigidbodyInertiaTensor();
            }

            // Rigidbodies should use spherical moments of inertia unless they contain colliders.
            else
            {
                // We need use scale to approximate a radius, averaging will be close enough if we have no colliders.
                var worldScale = transform.lossyScale;
                var radius = worldScale.x + worldScale.y + worldScale.z / 3f;

                // Spherical moments of inertia: V(2/5mR^2, 2/5mR^2, 2/5mR^2).
                var momentOfInertia = 2f / 5f * mass * (radius * radius);
                inertiaTensor = new Vector3 (momentOfInertia, momentOfInertia, momentOfInertia);
            }
        }

        /// <summary>
        /// Resets the centre of mass of the Rigidbody based on the currently attached Collider objects.
        /// </summary>
        public void ResetCentreOfMass()
        {
            // Simply average the position of each collider as a first time implementation.
            centreOfMass = Vector3.zero;

            foreach (var collider in m_colliders)
            {
                centreOfMass += collider.CalculateCentreOfMass();
            }

            if (m_colliders.Count > 0)
            {
                centreOfMass /= m_colliders.Count;
            }
        }

        #endregion

        #region Forces

        /// <summary>
        /// Adds a force to the object using the mode specified. Force and Impulse take the objects mass into account,
        /// Acceleration and VelocityChange ignore mass and move every object at the same speed.
        /// </summary>
        /// <param name="force">The desired force to be applied.</param>
        /// <param name="mode">Describes how the force should be applied.</param>
        public void AddForce (Vector3 force, ForceMode mode = ForceMode.Force)
        {
            switch (mode)
            {
                // Normal force.
                case ForceMode.Force:
                    m_linearForce += force;
                    break;

                // Mass-ignoring force.
                case ForceMode.Acceleration:
                    m_linearAcceleration += force;
                    break;

                // Force over a second applied in an instant.
                case ForceMode.Impulse:
                    m_linearForce += force / Time.fixedDeltaTime;
                    break;

                // Mass-ignoring force over a second applied in an instant.
                case ForceMode.VelocityChange:
                    m_linearAcceleration += force / Time.fixedDeltaTime;
                    break;

                default:
                    throw new System.NotImplementedException();
            }
        }

        /// <summary>
        /// Adds a torque to the object using the mode specified. Force and Impulse take the objects mass into account,
        /// Acceleration and VelocityChange ignore mass and move every object at the same speed.
        /// </summary>
        /// <param name="torque">The amount of torque to be applied.</param>
        /// <param name="mode">Describes how the force should be applied.</param>
        public void AddTorque (Vector3 torque, ForceMode mode = ForceMode.Force)
        {
            switch (mode)
            {
                // Normal torque.
                case ForceMode.Force:
                    m_angularTorque += torque;
                    break;

                // Mass-ignoring torque.
                case ForceMode.Acceleration:
                    m_angularAcceleration += torque;
                    break;

                // Torque over a second applied in an instant.
                case ForceMode.Impulse:
                    m_angularTorque += torque / Time.fixedDeltaTime;
                    break;

                // Mass-ignoring torque over a second applied in an instant.
                case ForceMode.VelocityChange:
                    m_angularAcceleration += torque / Time.fixedDeltaTime;
                    break;

                default:
                    throw new System.NotImplementedException();
            }
        }

        /// <summary>
        /// Adds force at a given point in space, this is the desired way of moving objects and causes both a linear
        /// and angular reaction from the object.
        /// </summary>
        /// <param name="force">The amount of force to be applied.</param>
        /// <param name="point">The point in world co-ordinates of the force.</param>
        /// <param name="mode">How the force should be applied.</param>
        public void AddForceAtPoint (Vector3 force, Vector3 point, ForceMode mode = ForceMode.Force)
        {
            var torque = Vector3.Cross (force, (position + centreOfMass) - point);
            AddForce (force, mode);
            AddTorque (torque, mode);
        }

        /// <summary>
        /// Resets the each accumulated force to zero.
        /// </summary>
        public void ResetAccumulatedForces()
        {
            m_linearForce = Vector3.zero;
            m_linearAcceleration = Vector3.zero;
            m_angularTorque = Vector3.zero;
            m_angularAcceleration = Vector3.zero;
        }

        #endregion
    }
}
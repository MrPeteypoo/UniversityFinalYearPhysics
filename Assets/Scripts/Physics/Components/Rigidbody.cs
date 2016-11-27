using AddComponentMenu          = UnityEngine.AddComponentMenu;
using Assert                    = UnityEngine.Assertions.Assert;
using DisallowMultipleComponent = UnityEngine.DisallowMultipleComponent;
using GameObject                = UnityEngine.GameObject;
using Mathf                     = UnityEngine.Mathf;
using MonoBehaviour             = UnityEngine.MonoBehaviour;
using Range                     = UnityEngine.RangeAttribute;
using SerializeField            = UnityEngine.SerializeField;
using Vector3                   = UnityEngine.Vector3;


namespace PSI
{
    /// <summary>
    /// A component which dictates how the GameObject should be handled by the Physics system.
    /// </summary>
    [System.Serializable, DisallowMultipleComponent, AddComponentMenu ("PSI/Rigidbody")]
    public sealed class Rigidbody : MonoBehaviour
    {
        #region Members

        /// <summary>
        /// The physical mass of the object, represented in kilograms.
        /// </summary>
        [SerializeField, Range (0.00001f, float.PositiveInfinity)]
        float m_mass = 1f;

        /// <summary>
        /// The uniform drag co-efficient of the object. The higher the value the more damping will be applied.
        /// </summary>
        [SerializeField, Range (0f, float.PositiveInfinity)]
        float m_drag = 0f;

        /// <summary>
        /// The velocity threshold at which the object should come to a resting point.
        /// </summary>
        [SerializeField, Range (0f, float.PositiveInfinity)]
        float m_sleepThreshold = 0.001f;

        /// <summary>
        /// Determines whether gravity should be applied.
        /// </summary>
        public bool simulateGravity = true;

        /// <summary>
        /// An offset representing the centre of mass. This effects how angular velocity is applied.
        /// </summary>
        public Vector3 centreOfMass = Vector3.zero;

        /// <summary>
        /// A cached copy of mass after being inverted.
        /// </summary>
        float m_inverseMass = 1f;

        /// <summary>
        /// How fast the object is travelling in each axis (metres/s).
        /// </summary>
        Vector3 m_velocity = Vector3.zero;

        /// <summary>
        /// Linear momentum represented in kilogram-metres/second.
        /// </summary>
        Vector3 m_momentum = Vector3.zero;

        /// <summary>
        /// Accumulated force to be applied next frame.
        /// </summary>
        Vector3 m_force = Vector3.zero;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the physical mass of the object, represented in kilograms.
        /// </summary>
        /// <value>Must be larger than zero.</value>
        public float mass
        {
            get { return m_mass; }
            set
            {
                // Ensure we don't have zero mass.
                Assert.IsTrue (mass > 0f, "Rigidbody mass must be larger than zero.");

                m_mass = Mathf.Max (0.00001f, value);
                m_inverseMass = 1f / m_mass;
            }
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
                Assert.IsTrue (m_drag >= 0f, "Rigidbody drag must be a positive value.");

                m_drag = Mathf.Max (0f, value);
            }
        }

        /// <summary>
        /// Gets or sets the velocity threshold at which the object should come to a resting point.
        /// </summary>
        /// <value>Must be a positive value.</value>
        public float sleepThreshold
        {
            get { return m_sleepThreshold; }
            set
            {
                // Must be a positive value.
                Assert.IsTrue (m_sleepThreshold >= 0f, "Rigibody sleep threshold must be a positive value.");

                m_sleepThreshold = Mathf.Max (0f, value);
            }
        }

        /// <summary>
        /// Gets the inverse of the objects mass.
        /// </summary>
        /// <value>The mass of the object after being inverted.</value>
        public float inverseMass
        {
            get { return m_inverseMass; }
        }

        /// <summary>
        /// Gets or sets the velocity at which the object is moving in metres/s. Setting this will recalcuate the 
        /// momentum.
        /// </summary>
        /// <value>The desired velocity of the object.</value>
        public Vector3 velocity
        {
            get { return m_velocity; }
            set
            {
                // We must recalcuate the momentum if the velocity is modified.
                m_velocity = value;
                m_momentum = velocity * mass;
            }
        }

        /// <summary>
        /// Gets or sets the linear momentum of the object, represented in kilogram-metres/second. Setting this will
        /// recalculate the momentum.
        /// </summary>
        /// <value>The desired momentum of the object.</value>
        public Vector3 momentum
        {
            get { return m_momentum; }
            set
            {
                m_momentum = value;
                m_velocity = m_momentum / mass;
            }
        }

        /// <summary>
        /// Gets the accumulated force to be applied before the next frame.
        /// </summary>
        public Vector3 force
        {
            get { return m_force; }
        }

        #endregion

        #region Component state management

        /// <summary>
        /// Registers the object with a Physics system either the default system or one which exists within the level.
        /// </summary>
        void OnEnable()
        {
            // Ensure we have a physics system to register with.
            var physics = Physics.FindSystem();
            Assert.IsNotNull (physics, "Couldn't find a Physics system to register with.");

            physics.Register (this);
        }

        #endregion

        #region Force

        /// <summary>
        /// Resets the accumulated force to zero.
        /// </summary>
        public void ResetForce()
        {
            m_force = Vector3.zero;
        }

        #endregion
    }
}
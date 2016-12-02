using Assert            = UnityEngine.Assertions.Assert;
using Mathf             = UnityEngine.Mathf;
using SerializeField    = UnityEngine.SerializeField;
using Tooltip           = UnityEngine.TooltipAttribute;
using Vector3           = UnityEngine.Vector3;


namespace PSI
{
    /// <summary>
    /// A simple spherical collider.
    /// </summary>
    public sealed class SphereCollider : Collider
    {
        #region Members

        /// <summary>
        /// The central offset of the sphere.
        /// </summary>
        [Tooltip ("The central offset of the sphere.")]
        public Vector3 centre = Vector3.zero;

        [SerializeField, Tooltip ("The length of the radius of the sphere.")]
        float m_radius = 5f;

        #endregion

        #region Properties and getters

        /// <summary>
        /// Gets the position of the sphere in world space with the centre offset applied.
        /// </summary>
        public Vector3 position
        {
            get { return transform.position + centre; }
        }

        /// <summary>
        /// Gets or sets the length of the radius of the sphere.
        /// </summary>
        /// <value>Must be a positive value.</value>
        public float radius
        {
            get { return m_radius; }
            set
            {
                Assert.IsTrue (value >= 0f, "Sphere colliders require a positive radius.");
                m_radius = Mathf.Max (0f, value);
                UpdateRigidbodyInertiaTensor();
            }
        }

        /// <summary>
        /// Gets the squared radius of the sphere.
        /// </summary>
        public float sqrRadius
        {
            get { return m_radius * m_radius; }
        }

        /// <summary>
        /// Gets the Derived.Sphere enum.
        /// </summary>
        /// <returns>Derived.Sphere</returns>
        public override sealed Derived GetDerivedType()
        {
            return Derived.Sphere;
        }

        #endregion

        #region State management

        /// <summary>
        /// Clamps the radius to a suitable value.
        /// </summary>
        void OnValidate()
        {
            radius = radius;
        }

        /// <summary>
        /// Updates the stored PhysicsMaterial if necessary, attaches to a Rigidbody and registers the SphereCollider
        /// with located Physics system.
        /// </summary>
        protected override void OnEnable ()
        {
            base.OnEnable();

            UpdateRigidbodyInertiaTensor();
            m_physics.Register (this);//, Derived.Sphere);
        }

        void OnDisable()
        {
            if (m_physics && this)
            {
                m_physics.Deregister (this);//, Derived.Sphere);
            }
        }

        /// <summary>
        /// Updates the intertia tensor of the attached Rigidbody.
        /// </summary>
        public override void UpdateRigidbodyInertiaTensor ()
        {
            // Do nothing if we aren't attached to anything.
            if (m_attachedRigidbody)
            {
                // Spherical moments of inertia: V(2/5mR^2, 2/5mR^2, 2/5mR^2).
                var mass = m_attachedRigidbody.mass;
                var momentOfInertia = 2f / 5f * mass * sqrRadius;

                var inertiaTensor = new Vector3 (momentOfInertia, momentOfInertia, momentOfInertia);
                m_attachedRigidbody.inertiaTensor = inertiaTensor;
            }
        }

        #endregion
    }
}
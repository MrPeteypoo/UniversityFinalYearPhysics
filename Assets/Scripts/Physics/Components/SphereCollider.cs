using AddComponentMenu  = UnityEngine.AddComponentMenu;
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
    [AddComponentMenu ("PSI/Sphere Collider")]
    public sealed class SphereCollider : Collider
    {
        #region Members

        [SerializeField, Tooltip ("The length of the radius of the sphere.")]
        float m_radius = 5f;

        #endregion

        #region Properties and getters

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

        #endregion

        #region Functionality

        /// <summary>
        /// Updates the intertia tensor of the attached Rigidbody.
        /// </summary>
        public override void UpdateRigidbodyInertiaTensor()
        {
            // Do nothing if we aren't attached to anything.
            if (m_attachedRigidbody)
            {
                // Spherical moments of inertia: V(2/5mR^2, 2/5mR^2, 2/5mR^2).
                var mass            = m_attachedRigidbody.mass;
                var momentOfInertia = 2f / 5f * mass * sqrRadius;
                var inertiaTensor   = new Vector3 (momentOfInertia, momentOfInertia, momentOfInertia);

                m_attachedRigidbody.inertiaTensor = inertiaTensor;
            }
        }

        /// <summary>
        /// Use the centre of the object as the centre of mass for the sphere.
        /// </summary>
        public override Vector3 CalculateCentreOfMass()
        {
            return centre;
        }

        #endregion
    }
}
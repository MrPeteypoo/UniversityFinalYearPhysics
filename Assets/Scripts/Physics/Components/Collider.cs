using AddComponentMenu  = UnityEngine.AddComponentMenu;
using Assert            = UnityEngine.Assertions.Assert;
using GameObject        = UnityEngine.GameObject;
using MonoBehaviour     = UnityEngine.MonoBehaviour;
using NonSerialized     = System.NonSerializedAttribute;
using Serializable      = System.SerializableAttribute;
using SerializeField    = UnityEngine.SerializeField;
using Tooltip           = UnityEngine.TooltipAttribute;
using Vector3           = UnityEngine.Vector3;

using System.Collections.Generic;


namespace PSI
{
    /// <summary>
    /// An abstraction to collidable objects in the game. Contains common data and functionality contained by colliders.
    /// </summary>
    [Serializable, AddComponentMenu ("PSI/Collider")]
    public abstract class Collider : MonoBehaviour
    {
        /// <summary>
        /// An enum representing each derived class from Collider. This is horrible and should be burned but the lack
        /// of metaprogramming features in C# is killing me.
        /// </summary>
        public enum Derived
        {
            Invalid,
            Sphere,
            Plane
        }

        #region Members

        /// <summary>
        /// The central offset of the sphere.
        /// </summary>
        [SerializeField, Tooltip ("The central offset of the sphere.")]
        Vector3 m_centre = Vector3.zero;

        /// <summary>
        /// The material containing the physical properties of the Collider object. If this is null when the object is
        /// starting then it will be set using the objects internal material ID.
        /// </summary>
        [NonSerialized]
        public PhysicsMaterial material = null;

        [SerializeField, Tooltip ("The material ID used to retrieve the physics properties of the collider.")]
        protected string m_materialID = null;

        /// <summary>
        /// The Rigidbody the Collider is attached to, any collisions will effect this object.
        /// </summary>
        protected Rigidbody m_attachedRigidbody = null;

        /// <summary>
        /// The physics system that the Collider is registered with.
        /// </summary>
        protected Physics m_physics = null;

        /// <summary>
        /// Contains each Collider that the current Collider is touching.
        /// </summary>
        private HashSet<Collider> m_touching = new HashSet<Collider>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the position of the sphere in world space with the centre offset applied.
        /// </summary>
        public Vector3 position
        {
            get { return transform.position + m_centre; }
        }

        /// <summary>
        /// Gets or sets the centre of the collider.
        /// </summary>
        /// <value>The desired centre.</value>
        public Vector3 centre
        {
            get { return m_centre; }
            set
            {
                m_centre = value;

                if (m_attachedRigidbody)
                {
                    m_attachedRigidbody.ResetCentreOfMass();
                }
            }
        }

        /// <summary>
        /// Gets the Rigidbody that the Collider is attached to. All collisions will effect this object. The Collider
        /// will prioritise attaching to a Rigidbody on the current object but if one can't be found then it will check
        /// its parent GameObject.
        /// </summary>
        public Rigidbody attachedRigidbody 
        {
            get { return m_attachedRigidbody; }
        }

        /// <summary>
        /// Gets whether the given Collider is static.
        /// </summary>
        public bool isStatic
        {
            get 
            {
                // Ensure both the collider and rigidbody are marked as static before classing it as static.
                return !m_attachedRigidbody || !m_attachedRigidbody.enabled;
            }
        }

        /// <summary>
        /// Gets whether the Collider is touching anything at the current point in time.
        /// </summary>
        /// <value>Whether the Collider is touching anything.</value>
        public bool isTouchingAnything
        {
            get { return m_touching.Count > 0; }
        }

        #endregion

        #region Functionality

        /// <summary>
        /// Updates the inertia tensor value on the attached Rigidbody.
        /// </summary>
        public abstract void UpdateRigidbodyInertiaTensor();

        /// <summary>
        /// Calculates the centre of mass of the object.
        /// </summary>
        public abstract Vector3 CalculateCentreOfMass();

        /// <summary>
        /// Gets the enum representing the derived type of the object.
        /// </summary>
        public virtual Derived GetDerivedType() { return Derived.Invalid; }

        /// <summary>
        /// Attempts to obtain the PhysicsMaterial for the Collider, attach itself to a Rigidbody and find a Physics
        /// system that can be registered with.
        /// </summary>
        virtual protected void OnEnable()
        {
            
            m_attachedRigidbody = FindAttachableRigidbody();
            material            = material ?? ObtainMaterial();
            UpdateRigidbodyInertiaTensor();

            // For some reason Unity is changing the object without destroying it so we have to handle that situation.
            if (!m_physics)
            {
                m_physics = Physics.FindSystem();
            }

            m_physics.Register (this);

            Assert.IsNotNull (m_physics, "Collider couldn't find a Physics system to register with.");
        }

        /// <summary>
        /// Deregisters the object if it has been attached to a Physics system.
        /// </summary>
        virtual protected void OnDisable()
        {
            if (m_physics && this)
            {
                m_physics.Deregister (this);
            }
        }

        /// <summary>
        /// Searches the scene for an object with the Tags.engine tag and gets the PhysicsMaterials component.
        /// </summary>
        protected PhysicsMaterial ObtainMaterial()
        {
            var engine = GameObject.FindGameObjectWithTag (Tags.engine);
            Assert.IsNotNull (engine, "Collider.ObtainMaterials() couldn't find an object with tag '" + Tags.engine +
                "' in the scene.");

            if (engine)
            {
                var materials = engine.GetComponent<PhysicsMaterials>();
                Assert.IsNotNull (materials, "Object with tag '" + Tags.engine + 
                    "' did not contain a PhysicsMaterial " + "component.");
                
                if (materials)
                {
                    return materials[m_materialID];
                }
            }

            // Sadface. :(
            return null;
        }

        /// <summary>
        /// Searches the current and parent GameObject for a Rigidbody component to attach to.
        /// </summary>
        protected Rigidbody FindAttachableRigidbody()
        {
            // Check the current object.
            var rigidbody = GetComponent<Rigidbody>();

            if (rigidbody)
            {
                return rigidbody;
            }

            // Check the parent.
            var parent = transform.root;
            return parent ? parent.GetComponent<Rigidbody>() : null;
        }

        /// <summary>
        /// Determines whether this instance is touching the specified collider.
        /// </summary>
        public bool IsTouching (Collider collider)
        {
            return m_touching.Contains (collider);
        }

        /// <summary>
        /// Adds the given Collider to a list of colliders that are currently "touching" this object.
        /// </summary>
        /// <param name="collider">The Collider to be added.</param>
        public void StartTouching (Collider collider)
        {
            m_touching.Add (collider);
        }

        /// <summary>
        /// Removes the given Collider from the list of colliders that are currently "touching" this object.
        /// </summary>
        /// <returns>Whether the collider was removed or not.</returns>
        /// <param name="collider">The Collider to be removed.</param>
        public bool StopTouching (Collider collider)
        {
            return m_touching.Remove (collider);
        }

        #endregion
    }
}
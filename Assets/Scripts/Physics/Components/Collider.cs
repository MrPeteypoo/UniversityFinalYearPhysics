using AddComponentMenu  = UnityEngine.AddComponentMenu;
using Assert            = UnityEngine.Assertions.Assert;
using GameObject        = UnityEngine.GameObject;
using MonoBehaviour     = UnityEngine.MonoBehaviour;
using NonSerialized     = System.NonSerializedAttribute;
using Serializable      = System.SerializableAttribute;
using SerializeField    = UnityEngine.SerializeField;
using Tooltip           = UnityEngine.TooltipAttribute;
using Vector3           = UnityEngine.Vector3;


namespace PSI
{
    /// <summary>
    /// An abstraction to collidable objects in the game. Contains common data and functionality contained by colliders.
    /// </summary>
    [Serializable, AddComponentMenu ("PSI/Collider")]
    public abstract class Collider : MonoBehaviour
    {
        #region Members

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

        #endregion

        #region Properties

        /// <summary>
        /// Gets the Rigidbody that the Collider is attached to. All collisions will effect this object. The Collider
        /// will prioritise attaching to a Rigidbody on the current object but if one can't be found then it will check
        /// its parent GameObject.
        /// </summary>
        public Rigidbody attachedRigidbody 
        {
            get { return m_attachedRigidbody; }
        }

        #endregion

        #region Functionality

        /// <summary>
        /// Attempts to obtain the PhysicsMaterial for the Collider and attach itself to a Rigidbody.
        /// </summary>
        virtual protected void Start()
        {
            m_attachedRigidbody = FindAttachableRigidbody();
            material            = material ?? ObtainMaterial();
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
        public Rigidbody FindAttachableRigidbody()
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

        #endregion
    }
}
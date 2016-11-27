using GameObject    = UnityEngine.GameObject;
using MonoBehaviour = UnityEngine.MonoBehaviour;
using Vector3       = UnityEngine.Vector3;


namespace PSI
{
    /// <summary>
    /// An abstraction to a Physics system. These systems manage the physical properties and simulation of the world.
    /// Objects must be registered for physical simulation to occur.
    /// </summary>
    public abstract class Physics : MonoBehaviour
    {
        #region Configuration

        /// <summary>
        /// The gravitational force to be applied to objects.
        /// </summary>
        public Vector3 gravity = new Vector3 (0f, -9.80665f, 0f);

        #endregion

        #region Registration

        /// <summary>
        /// Registers the given Rigidbody to be simulated by the system. This will cause forces to apply which will
        /// cause the attached GameObject to move and rotate freely.
        /// </summary>
        /// <param name="rigidbody">The Rigidbody to be registered.</param>
        public abstract void Register (Rigidbody rigidbody);

        /// <summary>
        /// Unregisters the given Rigidbody. This means it will no longer be tracked by the Physics system, resulting
        /// in the object no longer being simulated.
        /// </summary>
        /// <param name="rigidbody">Rigidbody.</param>
        public abstract void Deregister (Rigidbody rigidbody);

        #endregion

        #region Static functionality

        /// <summary>
        /// A reference to the default Physics system if required.
        /// </summary>
        protected static Physics defaultPhysicsSystem = null;

        /// <summary>
        /// Gets the default publicly available Physics system.
        /// </summary>
        public static Physics defaultSystem 
        { 
            get { return defaultPhysicsSystem; } 
        }

        /// <summary>
        /// Attempts to find a system within the currently active scene. If that fails Physics.defaultSystem will be
        /// returned.
        /// </summary>
        /// <returns>Either a valid system or null if none can be found.</returns>
        public static Physics FindSystem()
        {
            var engine = GameObject.FindGameObjectWithTag (Tags.engine);
            var system = engine.GetComponent<Physics>();

            return system ?? defaultSystem;
        }

        #endregion
    }
}
using AddComponentMenu          = UnityEngine.AddComponentMenu;
using Debug                     = UnityEngine.Debug;
using DisallowMultipleComponent = UnityEngine.DisallowMultipleComponent;
using GameObject                = UnityEngine.GameObject;
using MonoBehaviour             = UnityEngine.MonoBehaviour;
using Range                     = UnityEngine.RangeAttribute;
using Vector3                   = UnityEngine.Vector3;


namespace PSI
{
    /// <summary>
    /// A component which dictates how the GameObject should be handled by the Physics system.
    /// </summary>
    [System.Serializable, DisallowMultipleComponent, AddComponentMenu ("PSI/Rigidbody")]
    public sealed class Rigidbody : MonoBehaviour
    {
        #region Public members

        /// <summary>
        /// The physical mass of an object, represented in kilograms.
        /// </summary>
        public float mass = 1f;

        /// <summary>
        /// An offset representing the centre of mass. This effects how angular velocity is applied.
        /// </summary>
        public Vector3 centreOfMass = Vector3.zero;

        /// <summary>
        /// The uniform drag co-efficient of an object. The higher the value the more damping will be applied.
        /// </summary>
        [Range (0f, float.PositiveInfinity)]
        public float drag = 0f;

        #endregion

        #region Internal data

        /// <summary>
        /// The amount of accumulated force that should be applied.
        /// </summary>
        Vector3 m_force = Vector3.zero;

        #endregion

        #region Initialisation

        /// <summary>
        /// Registers the object with the physics system.
        /// </summary>
        void OnEnable()
        {
            // Ensure we have a physics system to register with.
            var physics = Physics.FindSystem();
            Debug.Assert (physics, "PSI.Rigidbody.Start(): couldn't find a Physics system to register with.");

            physics.Register (this);
        }
	
        // Update is called once per frame
        void Update()
        {
	
        }

        #endregion
    }
}
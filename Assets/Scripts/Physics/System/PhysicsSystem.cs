using AddComponentMenu  = UnityEngine.AddComponentMenu;
using Assert            = UnityEngine.Assertions.Assert;
using GameObject        = UnityEngine.GameObject;
using MonoBehaviour     = UnityEngine.MonoBehaviour;


namespace PSI
{
    /// <summary>
    /// TODO: PhysicsSystem.
    /// </summary>
    [AddComponentMenu ("PSI/PhysicsSystem")]
    public sealed class PhysicsSystem : Physics
    {
        #region Internal data

        /// <summary>
        /// Stores the registered Rigidbody objects in the scene.
        /// </summary>
        MappedPopList<Rigidbody> m_rigidbodies = new MappedPopList<Rigidbody> (100);

        #endregion

        #region Registration

        /// <summary>
        /// Registers the given Rigidbody to be simulated by the system. This will cause forces to apply which will
        /// cause the attached GameObject to move and rotate freely.
        /// </summary>
        /// <param name="rigidbody">The Rigidbody to be registered.</param>
        public override void Register (Rigidbody rigidbody)
        {
            m_rigidbodies.Add (rigidbody);
            var crap = new UnityEngine.Rigidbody();
        }

        /// <summary>
        /// Unregisters the given Rigidbody. This means it will no longer be tracked by the Physics system, resulting
        /// in the object no longer being simulated.
        /// </summary>
        /// <param name="rigidbody">Rigidbody.</param>
        public override void Deregister (Rigidbody rigidbody)
        {
            m_rigidbodies.SwapAndPop (rigidbody);
        }

        #endregion
    }
}
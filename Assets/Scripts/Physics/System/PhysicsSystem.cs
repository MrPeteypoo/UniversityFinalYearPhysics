using AddComponentMenu  = UnityEngine.AddComponentMenu;
using Assert            = UnityEngine.Assertions.Assert;
using GameObject        = UnityEngine.GameObject;
using MonoBehaviour     = UnityEngine.MonoBehaviour;

using System.Collections.Generic;


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
        MappedList<Rigidbody> m_rigidbodies = new MappedList<Rigidbody>();

        #endregion

        #region Physics methods

        /// <summary>
        /// Registers the given Rigidbody to be simulated by the system. This will cause forces to apply which will
        /// cause the attached GameObject to move and rotate freely.
        /// </summary>
        /// <param name="rigidbody">The Rigidbody to be registered.</param>
        public override void Register (Rigidbody rigidbody)
        {
            m_rigidbodies.Add (rigidbody);
        }

        #endregion
    }
}
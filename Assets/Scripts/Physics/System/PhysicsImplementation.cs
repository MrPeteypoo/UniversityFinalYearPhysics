using AddComponentMenu  = UnityEngine.AddComponentMenu;
using Assert            = UnityEngine.Assertions.Assert;
using GameObject        = UnityEngine.GameObject;
using MonoBehaviour     = UnityEngine.MonoBehaviour;
using Vector3           = UnityEngine.Vector3;


namespace PSI
{
    [AddComponentMenu ("PSI/PhysicsImplementation")]
    public sealed class PhysicsImplementation : PhysicsSystem
    {
        #region Internal data

        /// <summary>
        /// Stores the registered Rigidbody objects in the scene.
        /// </summary>
        MappedPopList<Rigidbody> m_rigidbodies = new MappedPopList<Rigidbody> (100);

        #endregion

        #region Execution

        /// <summary>
        /// Initialises the system, preparing it for use.
        /// </summary>
        /// <returns>Whether the initialisation was successful.</returns>
        public sealed override bool Initialise()
        {
            return true;
        }

        /// <summary>
        /// Nothing atm.
        /// </summary>
        /// <param name="deltaTime">How many seconds have passed since the last frame.</param>
        public sealed override void PreUpdate (float deltaTime)
        {
        }

        /// <summary>
        /// Performs a physics simulation pass on every registered object.
        /// </summary>
        /// <param name="deltaTime">How many seconds have passed since the last frame.</param>
        public sealed override void MainUpdate (float deltaTime)
        {
            foreach (var rigidbody in m_rigidbodies)
            {
                // Add gravity if necessary
                if (rigidbody.simulateGravity)
                {
                    rigidbody.AddAcceleration (gravity);
                }

                // Use RK4 to simulate the change in momentum and position of the object.
                RK4Integration.LinearMotion.Integrate (rigidbody, deltaTime);

                // Ensure we reset the forces correctly.
                rigidbody.ResetAccumulatedForces();
                Assert.IsTrue (rigidbody.accumulatedForce == Vector3.zero);
                Assert.IsTrue (rigidbody.accumulatedAcceleration == Vector3.zero);
            }
        }

        /// <summary>
        /// Detects collision of objects within the scene.
        /// </summary>
        /// <param name="deltaTime">How many second have passed since the last frame.</param>
        public sealed override void PostUpdate (float deltaTime)
        {
        }

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

﻿using AddComponentMenu  = UnityEngine.AddComponentMenu;
using Assert            = UnityEngine.Assertions.Assert;
using ForceMode         = UnityEngine.ForceMode;
using GameObject        = UnityEngine.GameObject;
using MonoBehaviour     = UnityEngine.MonoBehaviour;
using Vector3           = UnityEngine.Vector3;


namespace PSI
{
    [AddComponentMenu ("PSI/Physics Implementation")]
    public sealed class PhysicsImplementation : PhysicsSystem
    {
        #region Internal data

        /// <summary>
        /// Stores the registered Rigidbody objects in the scene.
        /// </summary>
        MappedPopList<Rigidbody> m_rigidbodies = new MappedPopList<Rigidbody> (100);

        /// <summary>
        /// Manages collision detection within the scene.
        /// </summary>
        CollisionDetection m_collision = new CollisionDetection();

        #endregion

        #region Execution

        /// <summary>
        /// Initialises the system, preparing it for use.
        /// </summary>
        /// <returns>Whether the initialisation was successful.</returns>
        public sealed override bool Initialise()
        {
            return m_collision.Initialise (gravity);
        }

        /// <summary>
        /// Performs a collision detection pass.
        /// </summary>
        /// <param name="deltaTime">How many seconds have passed since the last frame.</param>
        public sealed override void PreUpdate (float deltaTime)
        {
            // Ensure the gravity values are synchronised.
            m_collision.gravity = gravity;
            m_collision.Run();
        }

        /// <summary>
        /// Performs a physics simulation pass on every registered object.
        /// </summary>
        /// <param name="deltaTime">How many seconds have passed since the last frame.</param>
        public sealed override void MainUpdate (float deltaTime)
        {
            foreach (var rigidbody in m_rigidbodies)
            {
                // Add gravity if necessary.
                if (rigidbody.simulateGravity)
                {
                    rigidbody.AddForce (gravity, ForceMode.Acceleration);
                }

                // Use RK4 to simulate the change in momentum and position of the object.
                RK4Integrator.Integrate (rigidbody, deltaTime);

                // Ensure we reset the forces correctly.
                rigidbody.ResetAccumulatedForces();
            }
        }

        /// <summary>
        /// Nothing atm, in the future it will trigger collision callbacks.
        /// </summary>
        /// <param name="deltaTime">How many second have passed since the last frame.</param>
        public sealed override void PostUpdate (float deltaTime)
        {
            // TODO: Trigger collision callbacks.
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

        /// <summary>
        /// Register the given Collider with the system, allowing it to be used in collision detection.
        /// </summary>
        /// <param name="collider">The collider to be registered.</param>
        public override sealed void Register (Collider collider)
        {
            m_collision.Register (collider);
        }

        /// <summary>
        /// Unregisters the given Collider from the system, this stops it being used during collision detection.
        /// </summary>
        /// <param name="collider">The collider to be unregistered.</param>
        public override sealed void Deregister (Collider collider)
        {
            m_collision.Deregister (collider);
        }

        #endregion
    }
}

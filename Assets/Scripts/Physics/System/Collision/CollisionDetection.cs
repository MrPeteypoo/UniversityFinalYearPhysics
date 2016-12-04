using UnityEngine;
using System.Collections;

namespace PSI
{
    public sealed partial class CollisionDetection
    {
        #region Members

        /// <summary>
        /// Contains all static Colliders in the engine.
        /// </summary>
        Colliders m_static = new Colliders (100);

        /// <summary>
        /// Contains all dynamic Colliders in the engine.
        /// </summary>
        Colliders m_dynamic = new Colliders (100);

        #endregion

        #region Registration

        /// <summary>
        /// Registers a Collider with the CollisionDetection system.
        /// </summary>
        /// <param name="collider">The Collider to be registered.</param>
        /// <typeparam name="T">The actual derived type of the Collider.</typeparam>
        public void Register (Collider collider)
        {
            if (collider.isStatic)
            {
                m_static.Add (collider);
            }

            else
            {
                m_dynamic.Add (collider);
            }
        }

        /// <summary>
        /// Unregisters a Collider from the CollisionDetection system.
        /// </summary>
        /// <param name="collider">The Collider to be deregistered.</param>
        public void Deregister (Collider collider)
        {
            // It's possible for objects to have their static status changed after being added.
            if (collider.isStatic)
            {
                RemoveCollider (collider, m_static, m_dynamic);
            }

            else
            {
                RemoveCollider (collider, m_dynamic, m_static);
            }
        }

        /// <summary>
        /// Attempts to remove a Collider from the primary collection, if that fails then an attempt will be made to
        /// remove it from the fallback collection.
        /// </summary>
        /// <param name="collider">The Collider to be removed.</param>
        /// <param name="primary">The first collection to attempt to remove the Collider from.</param>
        /// <param name="fallback">Attempt to remove the Collider from this if the primary fails.</param>
        private void RemoveCollider (Collider collider, Colliders primary, Colliders fallback)
        {
            if (!primary.Remove (collider))
            {
                fallback.Remove (collider);
            }
        }

        #endregion

        #region Collision management

        /// <summary>
        /// Performs a collision detection pass of registered objects. This will test every dynamic object against 
        /// every dynamic and static object, therefore the less dynamic objects the better.
        /// </summary>
        public void Run()
        {
            CheckDynamicObjects();
            CheckDynamicAgainstStatic();
        }

        private void CheckDynamicObjects()
        {
            // Sphere.
            for (int i = 0; i < m_dynamic.spheres.Count; ++i)
            {
                // On sphere.
                for (int j = i+1; j < m_dynamic.spheres.Count; ++j)
                {
                    Collision.SphereOnSphere (m_dynamic.spheres[i], m_dynamic.spheres[j]);
                }
            }
        }

        private void CheckDynamicAgainstStatic()
        {
            // Sphere.
            for (int i = 0; i < m_dynamic.spheres.Count; ++i)
            {
                // On sphere.
                for (int j = 0; j < m_static.spheres.Count; ++j)
                {
                    Collision.SphereOnSphere (m_dynamic.spheres[i], m_static.spheres[j]);
                }
            }
        }

        #endregion
    }
}
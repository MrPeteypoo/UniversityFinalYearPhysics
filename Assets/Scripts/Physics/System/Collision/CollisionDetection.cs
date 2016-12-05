using Vector3 = UnityEngine.Vector3;


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

        /// <summary>
        /// Handles collision checking.
        /// </summary>
        Collision m_collision = new Collision();

        /// <summary>
        /// Handles collision response.
        /// </summary>
        Response m_response = new Response();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the gravitational acceleration which is used in determining frictional forces.
        /// </summary>
        public Vector3 gravity
        {
            get { return m_response.gravity; }
            set { m_response.gravity = value; }
        }

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
        /// Prepares the system for usage.
        /// </summary>
        /// <param name="gravity">The initial value to use for gravity.</param>
        public bool Initialise (Vector3 gravity)
        {
            m_collision.response = m_response;
            m_response.gravity = gravity;

            return true;
        }

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
            var spheres = m_dynamic.spheres;
            var planes  = m_dynamic.planes;

            // Sphere.
            for (int i = 0; i < spheres.Count; ++i)
            {
                var a = spheres[i];

                // On sphere.
                for (int j = i + 1; j < spheres.Count; ++j)
                {
                    var b = spheres[j];

                    // Don't check collision of compound colliders.
                    if (a.attachedRigidbody != b.attachedRigidbody)
                    {
                        m_collision.SphereOnSphere (a, b);
                    }
                }

                // On plane.
                for (int j = 0; j < planes.Count; ++j)
                {
                    var b = planes[j];

                    // Don't check collision of compound colliders.
                    if (a.attachedRigidbody != b.attachedRigidbody)
                    {
                        m_collision.SphereOnPlane (a, b);
                    }
                }
            }
        }

        private void CheckDynamicAgainstStatic()
        {
            // Sphere.
            foreach (var a in m_dynamic.spheres)
            {
                // On sphere.
                foreach (var b in m_static.spheres)
                {
                    m_collision.SphereOnSphere (a, b);
                }

                // On plane.
                foreach (var b in m_static.planes)
                {
                    m_collision.SphereOnPlane (a, b);
                }
            }

            // Plane.
            foreach (var a in m_dynamic.planes)
            {
                // On sphere.
                foreach (var b in m_static.spheres)
                {
                    m_collision.PlaneOnSphere (a, b);
                }
            }
        }

        #endregion
    }
}
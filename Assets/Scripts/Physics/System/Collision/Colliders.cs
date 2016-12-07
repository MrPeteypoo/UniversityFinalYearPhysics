using System;


namespace PSI
{
    public sealed partial class CollisionDetection
    {
        /// <summary>
        /// A collection of all Collider types registered with the engine.
        /// </summary>
        private struct Colliders
        {
            /// <summary>
            /// Contains SphereCollider objects.
            /// </summary>        
            public MappedPopList<SphereCollider> spheres;

            /// <summary>
            /// Contains PlaneCollider objects.
            /// </summary>
            public MappedPopList<PlaneCollider>planes;

            /// <summary>
            /// Constructs a Colliders object with the given capacity allocated for each Collider type.
            /// </summary>
            /// <param name="capacity">How many objects should be preallocated in each collection.</param>
            /// <param name="capacityIncrement">How much to increment capacity for each collection.</param>
            public Colliders (int capacity, int capacityIncrement = 32)
            {
                spheres = new MappedPopList<SphereCollider> (capacity, capacityIncrement);
                planes  = new MappedPopList<PlaneCollider> (capacity, capacityIncrement);
            }

            /// <summary>
            /// Adds the given Vollider to its corresponding collection.
            /// </summary>
            /// <param name="collider">The Collider to be added.</param>
            public void Add (Collider collider)
            {
                switch (collider.GetDerivedType())
                {
                    case Collider.Derived.Sphere:
                        spheres.Add (collider as SphereCollider);
                        break;
                    
                    case Collider.Derived.Plane:
                        planes.Add (collider as PlaneCollider);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }

            /// <summary>
            /// Removes the given Collider from its corresponding collection.
            /// </summary>
            /// <returns>Whether the Collider was successfully removed.</returns>
            /// <param name="collider">The Collider to be removed.</param>
            public bool Remove (Collider collider)
            {
                switch (collider.GetDerivedType())
                {
                    case Collider.Derived.Sphere:
                        return spheres.Remove (collider as SphereCollider);

                    case Collider.Derived.Plane:
                        return planes.Remove (collider as PlaneCollider);

                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}
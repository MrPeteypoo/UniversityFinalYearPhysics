using AddComponentMenu  = UnityEngine.AddComponentMenu;
using Vector3           = UnityEngine.Vector3;


namespace PSI
{
    /// <summary>
    /// A plane with an infinite height and width. Plane normals are specified by the transform.up vector.
    /// </summary>
    [AddComponentMenu ("PSI/Plane Collider")]
    public sealed class PlaneCollider : Collider
    {
        public override Derived GetDerivedType()
        {
            return Derived.Plane;
        }

        public override void UpdateRigidbodyInertiaTensor()
        {
            // Do nothing if we aren't attached to anything.
            if (m_attachedRigidbody)
            {
                // Box with no depth: V(1/12m(h^2), 1/12m(w^2), 1/12m(w^2 + h^2)).
                var mass                = m_attachedRigidbody.mass;
                var momentumOfInertia   = 1f / 12f * mass * (float.PositiveInfinity);

                m_attachedRigidbody.inertiaTensor = new Vector3 
                (
                    momentumOfInertia,
                    momentumOfInertia,
                    momentumOfInertia
                );
            }
        }

        public override Vector3 CalculateCentreOfMass()
        {
            return centre;
        }
    }
}
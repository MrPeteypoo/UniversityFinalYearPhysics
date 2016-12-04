using ForceMode = UnityEngine.ForceMode;
using Mathf     = UnityEngine.Mathf;
using Vector3   = UnityEngine.Vector3;


namespace PSI
{
    /// <summary>
    /// Checks and handles the collision of different Collider types.
    /// </summary>
    public static class Collision
    {
        /// <summary>
        /// Performs collision detection and response on two spheres.
        /// </summary>
        public static void SphereOnSphere (SphereCollider lhs, SphereCollider rhs)
        {
            // Obtain the positions.
            var a = lhs.position;
            var b = rhs.position;
            
            // We need to test whether the length from A to B is more than the radius.
            var difference      = b - a;
            var lengthSqr       = difference.sqrMagnitude;
            var radiusSum       = lhs.radius + rhs.radius;
            var radiusSumSqr    = radiusSum * radiusSum;

            if (lengthSqr <= radiusSumSqr)
            {
                // We've collided.
                var length          = Mathf.Sqrt (lengthSqr);
                var normal          = length != 0f ? difference / length : lhs.transform.forward;
                var intersection    = radiusSum - length;
                var pointOfContact  = a + normal * (lhs.radius - intersection);

                Respond (lhs, rhs, normal, intersection, pointOfContact);
            }
        }

        private static void Respond (Collider a, Collider b,
            Vector3 direction, float intersection, Vector3 contactPoint)
        {
            if (!a.isStatic)
            {
                // Perform a full impulse response if we have both objects.
                if (!b.isStatic)
                {
                    ImpulseResponse (a, b, direction, intersection, contactPoint);
                }

                // Perform a reduced collision response if one object is static.
                else
                {
                    StaticResponse (a, b, direction, intersection, contactPoint);
                }
            }

            // Perform a static response with "b" as the dynamic object.
            else if (!b.isStatic)
            {
                StaticResponse (b, a, -direction, intersection, contactPoint);
            }
        } 

        private static void StaticResponse (Collider dynamic, Collider stationary,
            Vector3 direction, float intersection, Vector3 contactPoint)
        {
            // Determine the surface properties that should be applied to both objects.
            var restitution = PhysicsMaterial.CalculateRestitution (dynamic.material, stationary.material);

            // Cache the rigidbody as we'll be using it a lot.
            var rigidbody = dynamic.attachedRigidbody;

            // Move the dynamic object completely away from the static object.
            var correction      = direction * intersection;
            rigidbody.position  -= correction;

            // We assume the static object has a mass so great it can't be moved and just scale by restitution.
            var momentumDirection   = Vector3.Reflect (rigidbody.momentum, direction).normalized;
            rigidbody.momentum      = momentumDirection * (rigidbody.momentum.magnitude * restitution);

            // Now do the same for angular momentum.
            var angularDirection        = Vector3.Reflect (rigidbody.angularMomentum, direction).normalized;
            rigidbody.angularMomentum   = -angularDirection * (rigidbody.angularMomentum.magnitude * restitution);
        }

        private static void ImpulseResponse (Collider a, Collider b,
            Vector3 direction, float intersection, Vector3 contactPoint)
        {
            // Adjust the position and momentum of each object.
            var correction = direction * (intersection * 0.5001f);
            a.attachedRigidbody.position -= correction;
            a.attachedRigidbody.position += correction;

            // Determine the surface properties that should be applied to both objects.
            var surfaceA = new PhysicsMaterial.Result();
            var surfaceB = new PhysicsMaterial.Result();
            PhysicsMaterial.Combine (a.material, b.material, out surfaceA, out surfaceB);

            // Correct the linear and rotational motion of the object.
            CorrectLinearMotion (a.attachedRigidbody, b.attachedRigidbody, surfaceA, surfaceB, direction);
            CorrectAngularMotion (a.attachedRigidbody, b.attachedRigidbody, surfaceA, surfaceB, direction);
        }

        private static void CorrectLinearMotion (Rigidbody rigidbodyA, Rigidbody rigidbodyB, 
            PhysicsMaterial.Result surfaceA, PhysicsMaterial.Result surfaceB, Vector3 normal)
        {
            // Cache the required movement data of each object.
            var massA       = rigidbodyA.mass;
            var massB       = rigidbodyB.mass;
            var momentumA   = rigidbodyA.momentum;
            var momentumB   = rigidbodyB.momentum;
            var velocityA   = rigidbodyA.velocity;
            var velocityB   = rigidbodyB.velocity;

            // Conservation of momentum formula: 
            // V1 = e(I1 + 2 * I2 - m2 * u1) / (m1 + m2).
            // V2 = e(u1 - u2) + V1.
            var v1 = surfaceA.restitution * (momentumA + 2f * momentumB - massB * velocityA) / (massA + massB);
            var v2 = surfaceB.restitution * (velocityA - velocityB) + v1;

            // We need the direction that both objects were moving to reflect the velocity correctly.
            var v1Direction = Vector3.Reflect (velocityA, -normal).normalized;
            var v2Direction = Vector3.Reflect (velocityB, normal).normalized;

            // Finally set the velocity of each object.
            rigidbodyA.velocity = v1Direction * v1.magnitude;
            rigidbodyB.velocity = v2Direction * v2.magnitude;
        }

        private static void CorrectAngularMotion (Rigidbody rigidbodyA, Rigidbody rigidbodyB, 
            PhysicsMaterial.Result surfaceA, PhysicsMaterial.Result surfaceB, Vector3 normal)
        {
            // Now do the same for rotation.
            var inertiaA    = rigidbodyA.inertiaTensor;
            var inertiaB    = rigidbodyB.inertiaTensor;
            var momentumA   = rigidbodyA.angularMomentum;
            var momentumB   = rigidbodyB.angularMomentum;
            var velocityA   = rigidbodyA.angularVelocity;
            var velocityB   = rigidbodyB.angularVelocity;

            // Conservation of momentum formula: 
            // V1 = e(I1 + 2 * I2 - m2 * u1) / (m1 + m2).
            // V2 = e(u1 - u2) + V1.
            var v1 = Maths.Divide (surfaceA.restitution * (momentumA + 2f * momentumB - Maths.Multiply (inertiaB, velocityA)), inertiaA + inertiaB);
            var v2 = surfaceB.restitution * (velocityA - velocityB) + v1;

            // We need the direction that both objects were moving to reflect the velocity correctly.
            var v1Direction = Vector3.Reflect (-momentumA, normal).normalized;
            var v2Direction = Vector3.Reflect (-momentumB, normal).normalized;

            // Finally set the velocity of each object.
            rigidbodyA.angularVelocity = v1Direction * v1.magnitude;
            rigidbodyB.angularVelocity = v2Direction * v2.magnitude;     
        }
    }
}

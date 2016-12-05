using ForceMode = UnityEngine.ForceMode;
using Mathf     = UnityEngine.Mathf;
using Time      = UnityEngine.Time;
using Vector3   = UnityEngine.Vector3;

using System;


namespace PSI
{
    public sealed partial class CollisionDetection
    {
        /// <summary>
        /// Handles collision response.
        /// </summary>
        private sealed class Response
        {
            #region Static members

            /// <summary>
            /// The acceleration of gravity, used in determining frictional forces.
            /// </summary>
            public Vector3 gravity;

            #endregion

            #region State notification

            public void Respond (Collider a, Collider b,
                Vector3 direction, float intersection, Vector3 contactPoint)
            {
                // Determine whether we're already touching the object.
                if (a.isTouchingAnything && a.IsTouching (b))
                {
                    Respond (a, b, direction, intersection, contactPoint, false);
                }

                else
                {
                    Respond (a, b, direction, intersection, contactPoint, true);

                    // The Collider and CollisionDetection class give us guarantees that we will never check two static
                    // colliders so we can safely set the objects as touching.
                    a.StartTouching (b);
                    b.StartTouching (a);
                }
            }

            public void NotColliding (Collider a, Collider b)
            {
                if (a.isTouchingAnything)
                {
                    if (a.StopTouching (b) && b.StopTouching (a))
                    {
                        // Trigger "a.OnCollisionEnd"
                    }
                }
            }

            #endregion

            #region Position correction

            /// <summary>
            /// Separates the two given rigidbodies by an equal amount.
            /// </summary>
            /// <param name="a">The first rigidbody.</param>
            /// <param name="b">The second rigidbody.</param>
            /// <param name="direction">The direction from "a" to "b".</param>
            /// <param name="intersection">The length to the intersection point from the given direction.</param>
            private void DynamicPositionCorrection (Rigidbody a, Rigidbody b, Vector3 direction, float intersection)
            {
                // Adjust the position and momentum of each object.
                var correction = direction * (intersection * 0.5f);
                a.position -= correction;
                a.position += correction;
            }

            /// <summary>
            /// Moves the given Rigidbody out of the way of a static object. 
            /// </summary>
            /// <param name="rigidbody">The Rigidbody to be moved.</param>
            /// <param name="direction">The direction from the Rigidbody to the static object.</param>
            /// <param name="intersection">The length to the intersection point from the given direction.</param>
            private void StaticPositionCorrection (Rigidbody rigidbody, Vector3 direction, float intersection)
            {
                // Move the dynamic object completely away from the static object.
                var correction      = direction * intersection;
                rigidbody.position  -= correction;
            }

            #endregion

            #region Response

            private void Respond (Collider a, Collider b,
                Vector3 direction, float intersection, Vector3 contactPoint, bool initialResponse)
            {
                if (!a.isStatic)
                {
                    // Perform a full impulse response if we have both objects.
                    if (!b.isStatic)
                    {
                        ImpulseResponse (a, b, direction, intersection, contactPoint, initialResponse);
                    }

                    // Perform a reduced collision response if one object is static.
                    else
                    {
                        StaticResponse (a, b, direction, intersection, contactPoint, initialResponse);
                    }
                }

                // Perform a static response with "b" as the dynamic object.
                else if (!b.isStatic)
                {
                    StaticResponse (b, a, -direction, intersection, contactPoint, initialResponse);
                }

                // This should never happen.
                else
                {
                    throw new ApplicationException();
                }
            }

            private void StaticResponse (Collider dynamic, Collider stationary,
                Vector3 direction, float intersection, Vector3 contactPoint, bool initialResponse)
            {
                // Cache the rigidbody as we'll be using it a lot.
                var rigidbody = dynamic.attachedRigidbody;

                // Get the surface properties.
                var staticFriction  = PhysicsMaterial.CalculateStaticFriction (dynamic.material, stationary.material);
                var kineticFriction = PhysicsMaterial.CalculateKineticFriction (dynamic.material, stationary.material);
                var restitution     = PhysicsMaterial.CalculateRestitution (dynamic.material, stationary.material);

                // Ensure the object is moved away.
                StaticPositionCorrection (rigidbody, direction, intersection);
                if (initialResponse)
                {

                    // We assume the static object has a mass so great it can't be moved and just scale by restitution.
                    var momentumDirection   = Vector3.Reflect (rigidbody.momentum, direction);
                    rigidbody.momentum      = momentumDirection * restitution;

                    // Now do the same for angular momentum.
                    var angularDirection        = Vector3.Reflect (rigidbody.angularMomentum, direction).normalized;
                    rigidbody.angularMomentum   = -angularDirection * restitution;
                }

                // Apply friction.
                ApplyFriction (rigidbody, staticFriction, kineticFriction, contactPoint);
            }

            private void ImpulseResponse (Collider a, Collider b,
                Vector3 direction, float intersection, Vector3 contactPoint, bool initialResponse)
            {
                // Cache the rigidbodies since we'll use them a lot.
                var rigidbodyA = a.attachedRigidbody;
                var rigidbodyB = b.attachedRigidbody;

                // Determine the surface properties that should be applied to both objects.
                var surfaceA = new PhysicsMaterial.Result();
                var surfaceB = new PhysicsMaterial.Result();
                PhysicsMaterial.Combine (a.material, b.material, out surfaceA, out surfaceB);

                // Adjust the position.
                DynamicPositionCorrection (a.attachedRigidbody, b.attachedRigidbody, direction, intersection);
                // Correct the linear and rotational motion of the object.
                if (initialResponse)
                {

                    CorrectLinearMotion (a.attachedRigidbody, b.attachedRigidbody, surfaceA, surfaceB, direction);
                    CorrectAngularMotion (a.attachedRigidbody, b.attachedRigidbody, surfaceA, surfaceB, direction);
                }

                // Apply friction.
                ApplyFriction (rigidbodyA, surfaceA.staticFriction, surfaceA.kineticFriction, contactPoint);
                ApplyFriction (rigidbodyB, surfaceB.staticFriction, surfaceB.kineticFriction, contactPoint);
            }

            private void CorrectLinearMotion (Rigidbody rigidbodyA, Rigidbody rigidbodyB, 
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

            private void CorrectAngularMotion (Rigidbody rigidbodyA, Rigidbody rigidbodyB, 
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

            #endregion

            #region Friction

            public void ApplyFriction (Rigidbody rigidbody, float staticCoefficient, float kineticCoefficient,
                Vector3 contactPoint)
            {
                // Square the threshold for efficiency.
                var threshold       = rigidbody.sleepThreshold * rigidbody.sleepThreshold;
                var gravityToApply  = rigidbody.simulateGravity ? gravity : Vector3.zero;

                var rigidbodyToContact  = (contactPoint - rigidbody.position);
                var gravitationalForce  = rigidbody.mass * gravityToApply;
                var angle               = Vector3.Angle (-rigidbodyToContact, Vector3.down) * Mathf.Deg2Rad;
                var N                   = gravitationalForce * Mathf.Cos (angle);

                var force       = rigidbody.momentum / Time.fixedDeltaTime + gravitationalForce;
                var direction   = force.normalized;

                // Apply to linear momentum.
                if (force.sqrMagnitude < threshold)
                {
                    rigidbody.AddForce (-staticCoefficient * direction * N.magnitude);
                }

                else
                {
                    rigidbody.AddForce (-kineticCoefficient * direction * N.magnitude);
                }

                rigidbody.AddForce (N);
            }

            #endregion
        }
    }
}
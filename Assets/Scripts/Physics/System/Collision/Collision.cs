using ForceMode = UnityEngine.ForceMode;
using Mathf     = UnityEngine.Mathf;
using Vector3   = UnityEngine.Vector3;

using System;


namespace PSI
{
    public sealed partial class CollisionDetection
    {
        /// <summary>
        /// Checks the collision of different Collider types and forwards them to CollisionDetection.Response.
        /// </summary>
        private struct Collision
        {
            #region Members

            /// <summary>
            /// The instance of CollisionDetection.Response forward collision information to.
            /// </summary>
            public Response response;

            #endregion

            #region Sphere

            /// <summary>
            /// Performs collision detection and response on two spheres.
            /// </summary>
            public void SphereOnSphere (SphereCollider lhs, SphereCollider rhs)
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

                    response.Respond (lhs, rhs, normal, intersection, pointOfContact);
                }

                else
                {
                    response.NotColliding (lhs, rhs);
                }
            }

            /// <summary>
            /// Performs collision detection and response on a sphere and plane.
            /// </summary>
            public void SphereOnPlane (SphereCollider lhs, PlaneCollider rhs)
            {
                // We need position of each object and the normal of the plane.
                var sphere  = lhs.position;
                var plane   = rhs.position;
                var normal  = rhs.transform.up;

                // The formula for collision is: c.n - q.n < radium.
                var sphereDot   = Vector3.Dot (sphere, normal);
                var planeDot    = Vector3.Dot (plane, normal);
                var distance    = sphereDot - planeDot;

                if (Mathf.Abs (distance) < lhs.radius)
                {
                    // We've collided!
                    var intersection    = lhs.radius - distance;
                    var pointOfContact  = sphere - normal * intersection;

                    response.Respond (lhs, rhs, -normal, intersection, pointOfContact);
                }

                else
                {
                    response.NotColliding (lhs, rhs);
                }
            }

            #endregion

            #region Plane

            /// <summary>
            /// Performs collision detection between a plane and sphere.
            /// </summary>
            public void PlaneOnSphere (PlaneCollider lhs, SphereCollider rhs)
            {
                SphereOnPlane (rhs, lhs);
            }

            public void PlaneOnPlane (PlaneCollider lhs, PlaneCollider rhs)
            {
                // TODO: Implement plane on plane.
            }

            #endregion
        }
    }
}

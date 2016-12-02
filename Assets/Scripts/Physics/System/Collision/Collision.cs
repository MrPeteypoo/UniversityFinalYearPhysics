using Mathf = UnityEngine.Mathf;


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
                var length = Mathf.Sqrt (lengthSqr);
                
            }
        }
    }
}

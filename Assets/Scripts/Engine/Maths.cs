using UnityEngine;


/// <summary>
/// Contains utility mathematical functions.
/// </summary>
public static class Maths
{

    #region Calculus

    /// <summary>
    /// Integrates force with respect to time, producing a momentum value.
    /// </summary>
    /// <returns>The momentum of the integrated force as kilogram-metres/second.</returns>
    /// <param name="force">A force value to be integrated.</param>
    /// <param name="deltaTime">The time-step to use in integrating force.</param>
    public static Vector3 IntegrateForce (Vector3 force, float deltaTime)
    {
        return force * deltaTime;
    }

    /// <summary>
    /// Integrates linear momentum with respect to mass, producing a velocity value.
    /// </summary>
    /// <returns>The linear velocity of the integrated momentum as metres/second.</returns>
    /// <param name="momentum">A momentum value to be integrated.</param>
    /// <param name="mass">The mass of an object containing the given momentum.</param>
    public static Vector3 IntegrateMomentum (Vector3 momentum, float mass)
    {
        return momentum / mass;
    }

    /// <summary>
    /// Integrates angular momentum with respect to moments of inertia, producing a velocity value.
    /// </summary>
    /// <returns>The angular velocity of the integrated momentum as metres/second.</returns>
    /// <param name="momentum">The angular momentum value to be integrated.</param>
    /// <param name="inertiaTensor">The diagonal moments of inertia of an object.</param>
    public static Vector3 IntegrateMomentum (Vector3 momentum, Vector3 inertiaTensor)
    {
        var velocity = new Vector3();

        // Ensure we avoid divide by zero errors.
        for (int i = 0; i < 3; ++i)
        {
            velocity[i] = inertiaTensor[i] != 0f ? momentum[i] / inertiaTensor[i] : 0f;
        }

        return velocity;
    }

    /// <summary>
    /// Derives momentum from the given velocity with respect to mass.
    /// </summary>
    /// <returns>The momentum value in kilogram-metres/second.</returns>
    /// <param name="velocity">The velocity value to derive from.</param>
    /// <param name="mass">The mass of an object containing the given velocity.</param>
    public static Vector3 DeriveMomentum (Vector3 velocity, float mass)
    {
        return velocity * mass;
    }

    /// <summary>
    /// Derives momentum from the given angular velocity with respect to moments of inertia.
    /// </summary>
    /// <returns>The momentum.</returns>
    /// <param name="angularVelocity">Angular velocity.</param>
    /// <param name="inertiaTensor">Inertia tensor.</param>
    public static Vector3 DeriveMomentum (Vector3 angularVelocity, Vector3 inertiaTensor)
    {
        return Multiply (angularVelocity, inertiaTensor);
    }

    /// <summary>
    /// Derives the "spin" acceleration factor from the given angular velocity with respect to an objects orientation.
    /// </summary>
    /// <returns>The resulting "spin" velocity.</returns>
    /// <param name="angularVelocity">The angular velocity to derive from.</param>
    /// <param name="orientation">The current orientation of an object.</param>
    public static Quaternion DeriveSpin (Vector3 angularVelocity, Quaternion orientation)
    {
        // The formula is: dq/dt = 0.5wq, w is angular velocity as a quaternion and q is the current rotation.
        var velocityQuaternion  = new Quaternion (angularVelocity.x, angularVelocity.y, angularVelocity.z, 0f);
        var unscaled            = velocityQuaternion * orientation;

        // Since Unity doesn't allow the scaling of Quaternions we must do it ourselves.
        const float half = 0.5f;
        return new Quaternion (unscaled.x * half, unscaled.y * half, unscaled.z * half, unscaled.w * half);
    }

    #endregion

    #region Unity shortcomings

    /// <summary>
    /// Adds two quaternions together using component-wise addition.
    /// </summary>
    public static Quaternion Add (Quaternion lhs, Quaternion rhs)
    {
        return new Quaternion
        (
            lhs.x + rhs.x,
            lhs.y + rhs.y,
            lhs.z + rhs.z,
            lhs.w + rhs.w
        );
    }

    /// <summary>
    /// Scales each component inside the given Quaternion.
    /// </summary>
    public static Quaternion Scale (Quaternion lhs, float rhs)
    {
        return new Quaternion 
        (
            lhs.x * rhs, 
            lhs.y * rhs, 
            lhs.z * rhs, 
            lhs.w * rhs
        );
    }

    /// <summary>
    /// Scales each component inside the given Quaternion.
    /// </summary>
    public static Quaternion Scale (float lhs, Quaternion rhs)
    {
        return Scale (rhs, lhs);
    }

    /// <summary>
    /// Multiply two vectors together using component-wise multiplication.
    /// </summary>
    public static Vector3 Multiply (Vector3 lhs, Vector3 rhs)
    {
        return new Vector3
        (
            lhs.x * rhs.x,
            lhs.y * rhs.y,
            lhs.z * rhs.z
        );
    }

    public static Vector3 Divide (Vector3 lhs, Vector3 rhs)
    {
        return new Vector3
        (
            lhs.x / rhs.x,
            lhs.y / rhs.y,
            lhs.z / rhs.z
        );
    }

    #endregion
}


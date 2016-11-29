using Vector3 = UnityEngine.Vector3;


namespace PSI
{
    /// <summary>
    /// Contains functions used to integrate different physical simulations such as linear motion and angular momentum
    /// using the Runge-Kutta 4th Order numerical integration method.
    /// </summary>
    public static partial class RK4Integration
    {
        /// <summary>
        /// Calculates the position and momentum of a rigidbody.
        /// </summary>
        public sealed class LinearMotion
        {
            /// <summary>
            /// Update the momentum and position of the given rigidbody using the RK4 integration method.
            /// </summary>
            /// <param name="rigidbody">The object to be updated.</param>
            /// <param name="deltaTime">How long to step with each evaluation.</param>
            public static void Integrate (Rigidbody rigidbody, float deltaTime)
            {
                // Cache anything calculated multiple times.
                var halfDelta = deltaTime / 2f;
                var sixth = 1f / 6f;

                // RK4 uses four evaulations to create values to weight.
                var first   = Evaluation.Evaluate (rigidbody, new Evaluation(), 0f);
                var second  = Evaluation.Evaluate (rigidbody, first, halfDelta);
                var third   = Evaluation.Evaluate (rigidbody, second, halfDelta);
                var fourth  = Evaluation.Evaluate (rigidbody, third, deltaTime);

                // Now we use Taylor Series expansion to weight the values: 1/6 * (dxdt1 + 2 * (dxdt2 + dxdt3) + dxdt4).
                var translation = sixth * (first.velocity + 2f * (second.velocity + third.velocity) + fourth.velocity);
                var momentumInc = sixth * (first.force + 2f * (second.force + third.force) + fourth.force);

                // Finally set the correct position and momentum.
                rigidbody.transform.position    += translation * deltaTime;
                rigidbody.momentum              += momentumInc * deltaTime;
            }

            /// <summary>
            /// Contains the integrated velocity and derivative force of the rigidbody.
            /// </summary>
            private struct Evaluation
            {
                /// <summary>
                /// The evaluated velocity of an object at a given time-step.
                /// </summary>
                public Vector3 velocity;

                /// <summary>
                /// The evaluated force applied to an object at a given time-step.
                /// </summary>
                public Vector3 force;

                /// <summary>
                /// Sets the values of an Evaluation upon construction.
                /// </summary>
                /// <param name="velocity">The evaluated velocity of an object at a given time-step.</param>
                /// <param name="force">The evaluated force applied to an object at a given time-step.</param>
                public Evaluation (Vector3 velocity, Vector3 force)
                {
                    this.velocity   = velocity;
                    this.force      = force;
                }

                /// <summary>
                /// Integrates the velocity of a rigidbody and calculates the forces being applied to it at the given
                /// time-step.
                /// </summary>
                /// <param name="rigidbody">The rigidbody containing momentum and force data.</param>
                /// <param name="previous">A previous evaluation to step from.</param>
                /// <param name="deltaTime">The time-step to apply from the previous evaluation.</param>
                public static Evaluation Evaluate (Rigidbody rigidbody, Evaluation previous, float deltaTime)
                {
                    // Integrating force in respect to time gives us momentum.
                    var momentum = rigidbody.momentum + previous.force * deltaTime;

                    // Integrate velocity by dividing by the objects mass.
                    var velocity = (momentum) / rigidbody.mass;

                    return new Evaluation (velocity, CalculateForce (rigidbody, momentum));
                }

                /// <summary>
                /// Calculates the forces applied to the object.
                /// </summary>
                /// <returns>The total force to be applied to the object.</returns>
                /// <param name="rigidbody">The Rigidbody containing accumulated forces.</param>
                /// <param name="momentum">The momentum of the object at the current time-step.</param>
                private static Vector3 CalculateForce (Rigidbody rigidbody, Vector3 momentum)
                {
                    // Force is assumed to be added every frame.
                    var force = rigidbody.accumulatedForce;

                    // Acceleration is also assumed to be added every frame. Acceleration is independant of mass so we
                    // must multiply it by the objects mass.
                    var acceleration = rigidbody.accumulatedAcceleration * rigidbody.mass;

                    // Calculate resisted motion according to Mathematics for 3D Game Rogramming and Computer Graphics.
                    // Formula: mg - mkx'. "mg" is contained within acceleration, momentum gives us "mx'" and "k" is
                    // the rigidbody drag co-efficient.
                    var resistance = momentum * rigidbody.drag;

                    // Add them together and we're done!
                    return force + acceleration - resistance;
                }
            }
        }
    }
}
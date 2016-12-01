using Quaternion    = UnityEngine.Quaternion;
using Vector3       = UnityEngine.Vector3;


namespace PSI
{
    /// <summary>
    /// Contains functionality to perform physical simulations of linear motion and rotation using the Runge-Kutta 4th 
    /// Order numerical integration method.
    /// </summary>
    public static partial class RK4Integrator
    {
        /// <summary>
        /// Updates the position, rotation and associated momentum using RK4 integration.
        /// </summary>
        /// <param name="rigidbody">The object to be updated.</param>
        /// <param name="deltaTime">The length of the normal time-step.</param>
        public static void Integrate (Rigidbody rigidbody, float deltaTime)
        {
            // Cache anything calculated multiple times.
            var halfDelta = deltaTime / 2f;
            var sixth = 1f / 6f;

            // RK4 uses four evaulations to create values to weight.
            var first   = Evaluation.Evaluate (rigidbody);
            var second  = Evaluation.Evaluate (rigidbody, first, halfDelta);
            var third   = Evaluation.Evaluate (rigidbody, second, halfDelta);
            var fourth  = Evaluation.Evaluate (rigidbody, third, deltaTime);

            // Now we use Taylor Series expansion to weight the values: 1/6 * (dxdt1 + 2 * (dxdt2 + dxdt3) + dxdt4).
            var velocity    = sixth * (first.velocity + 2f * (second.velocity + third.velocity) + fourth.velocity);
            var force       = sixth * (first.force + 2f * (second.force + third.force) + fourth.force);
            var torque      = sixth * (first.torque + 2f * (second.torque + third.torque) + fourth.torque);

            // Since Unity doesn't support scaling and adding Quaternions the spin is going to be ugly....
            var spin = Maths.Scale (sixth, Maths.Add (Maths.Add (first.spin, Maths.Scale (2f, Maths.Add (second.spin, third.spin))), fourth.spin));

            // Finally set the correct position and momentum.
            rigidbody.position          += velocity * deltaTime;
            rigidbody.rotation          = Maths.Add (rigidbody.rotation, Maths.Scale (spin, deltaTime));
            rigidbody.momentum          += force * deltaTime;
            rigidbody.angularMomentum   += torque * deltaTime;
        }

        /// <summary>
        /// Contains the integrated velocity and derivative force of the rigidbody.
        /// </summary>
        private struct Evaluation
        {

            #region Derivatives

            /// <summary>
            /// The evaluated velocity of an object at a given time-step.
            /// </summary>
            public Vector3 velocity;

            /// <summary>
            /// The evaluated spin of an object at a given time-step.
            /// </summary>
            public Quaternion spin;

            /// <summary>
            /// The evaluated force applied to an object at a given time-step.
            /// </summary>
            public Vector3 force;

            /// <summary>
            /// The evaluated torque applied to an object at a given time-step.
            /// </summary>
            public Vector3 torque;

            #endregion

            #region Evaluation

            /// <summary>
            /// Initial evaluation at the current point in time.
            /// </summary>
            /// <param name="rigidbody">The rigidbody containing linear and angular physics data.</param>
            public static Evaluation Evaluate (Rigidbody rigidbody)
            {
                var evaluation      = new Evaluation();
                evaluation.velocity = rigidbody.velocity;
                evaluation.spin     = rigidbody.spin;
                evaluation.force    = CalculateForce (rigidbody, rigidbody.momentum);
                evaluation.torque   = CalculateTorque (rigidbody, rigidbody.angularMomentum);

                return evaluation;
            }

            /// <summary>
            /// Evaluates the simulation at the given time-step.
            /// </summary>
            /// <param name="rigidbody">The rigidbody containing linear and rotational physics data.</param>
            /// <param name="previous">A previous evaluation to step from.</param>
            /// <param name="deltaTime">The time-step to apply from the previous evaluation.</param>
            public static Evaluation Evaluate (Rigidbody rigidbody, Evaluation previous, float deltaTime)
            {
                // Integrating force in respect to time gives us momentum.
                var momentum        = rigidbody.momentum + Maths.IntegrateForce (previous.force, deltaTime);
                var angularMomentum = rigidbody.angularMomentum + Maths.IntegrateForce (previous.torque, deltaTime);
                var angularVelocity = Maths.IntegrateMomentum (angularMomentum, rigidbody.inertiaTensor);

                // Using the calculated values we can determine the integrals and derivatives.
                var evaluation      = new Evaluation();
                evaluation.velocity = Maths.IntegrateMomentum (momentum, rigidbody.mass);
                evaluation.spin     = Maths.DeriveSpin (angularVelocity, rigidbody.rotation);
                evaluation.force    = CalculateForce (rigidbody, momentum);
                evaluation.torque   = CalculateTorque (rigidbody, angularMomentum);

                // We're done with our evaluation.
                return evaluation;
            }

            /// <summary>
            /// Calculates the linear force applied to the object.
            /// </summary>
            /// <returns>The total force to be applied to the object.</returns>
            /// <param name="rigidbody">The Rigidbody containing accumulated forces.</param>
            /// <param name="momentum">The momentum of the object at the current time-step.</param>
            private static Vector3 CalculateForce (Rigidbody rigidbody, Vector3 momentum)
            {
                // Force is assumed to be added every frame.
                var force = rigidbody.linearForce;

                // Acceleration is also assumed to be added every frame. Acceleration is independant of mass so we
                // must multiply it by the objects mass.
                var acceleration = rigidbody.linearAcceleration * rigidbody.mass;

                // Calculate resisted motion according to Mathematics for 3D Game Rogramming and Computer Graphics.
                // Formula: mg - mkx'. "mg" is contained within acceleration, momentum gives us "mx'" and "k" is
                // the rigidbody drag co-efficient.
                var resistance = momentum * rigidbody.drag;

                // Add them together and we're done!
                return force + acceleration - resistance;
            }

            /// <summary>
            /// Calculates the torque applied to the object.
            /// </summary>
            /// <returns>The total torque to be applied to the object.</returns>
            /// <param name="rigidbody">The Rigidbody containing accumulated rotational forces.</param>
            /// <param name="angularMomentum">The angular momentum of the object at the current time-step.</param>
            private static Vector3 CalculateTorque (Rigidbody rigidbody, Vector3 angularMomentum)
            {
                // Torque is assumed to be added every frame.
                var torque = rigidbody.angularTorque;

                // Acceleration is also assumed to be added every frame but it's also independant of mass.
                var acceleration = rigidbody.angularAcceleration * rigidbody.mass;

                // Use similar calculation to linear motion for resisted motion.
                var resistance = angularMomentum * rigidbody.angularDrag;

                // Add them all together and we're done.
                return torque + acceleration - resistance;
            }

            #endregion
        }
    }
}
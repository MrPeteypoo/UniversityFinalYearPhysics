using Assert                    = UnityEngine.Assertions.Assert;
using Mathf                     = UnityEngine.Mathf;
using Object                    = UnityEngine.Object;
using PhysicsMaterialCombine    = UnityEngine.PhysicMaterialCombine;
using Range                     = UnityEngine.RangeAttribute;
using SerializeField            = UnityEngine.SerializeField;
using Tooltip                   = UnityEngine.TooltipAttribute;


namespace PSI
{
    /// <summary>
    /// A representation of the physical properties of a material. Physics materials control the restitution and 
    /// friction attributes of collision.
    /// </summary>
    public sealed class PhysicsMaterial : Object
    {
        #region Members

        [SerializeField, Tooltip ("Determines the strength of friction when objects are moving against the surface.")]
        float m_kineticFriction = 0.5f;

        [SerializeField, Tooltip ("Effects how much force is required to initially move a static object.")]
        float m_staticFriction = 0.5f;

        [SerializeField, Range (0f, 1f), Tooltip ("How much momentum is maintained when colliding with the surface.")]
        float m_restitution = 0.5f;

        /// <summary>
        /// How frictional forces will be calculated for the current surface.
        /// </summary>
        [Tooltip ("How frictional forces will be calculated for the current surface.")]
        public PhysicsMaterialCombine frictionCalculation = PhysicsMaterialCombine.Average;

        /// <summary>
        /// How restitution will be calculated for the current surface.
        /// </summary>
        [Tooltip ("How restitution will be calculated for the current surface.")]
        public PhysicsMaterialCombine restituionCalculation = PhysicsMaterialCombine.Average;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the strength of friction when objects are moving against the surface.
        /// </summary>
        /// <value>Must be a positive value.</value>
        public float kineticFriction
        {
            get { return m_kineticFriction; }
            set 
            { 
                Assert.IsTrue (value >= 0f, "Physics material kinetic friction must be a positive value.");
                m_kineticFriction = Mathf.Max (0f, value);
            }
        }

        /// <summary>
        /// Gets or sets the co-efficient effecting how much force is required to initially move a static object.
        /// </summary>
        /// <value>Must be a positive value.</value>
        public float staticFriction
        {
            get { return m_staticFriction; }
            set 
            { 
                Assert.IsTrue (value >= 0f, "Physics material static friction must be a positive value.");
                m_staticFriction = Mathf.Max (0f, value);
            }
        }

        /// <summary>
        /// Gets or sets the scalar determining how much momentum is maintained after colliding with another surface.
        /// </summary>
        /// <value>Must be between 0f and 1f.</value>
        public float restitution
        {
            get { return m_restitution; }
            set 
            { 
                Assert.IsTrue (value >= 0f && value <= 1f, "Physics material restitution must be between 0f and 1f.");
                m_restitution = Mathf.Clamp (value, 0f, 1f);
            }
        }

        #endregion

        #region Validation

        void OnValidate()
        {
            // The properties handle clamping.
            kineticFriction = kineticFriction;
            staticFriction = staticFriction;
        }

        #endregion

        #region Calculations

        /// <summary>
        /// Calculates the kinetic friction co-efficient for the current surface based on the other material given.
        /// </summary>
        /// <returns>A calculated kinectic friction co-efficient.</returns>
        /// <param name="other">The other surface, if null then no calculation will be performed.</param>
        public float CalculateKineticFriction (PhysicsMaterial other)
        {
            return other ? Calculate (kineticFriction, other.kineticFriction, frictionCalculation) : kineticFriction;
        }

        /// <summary>
        /// Calculates the static friction co-efficient for the current surface based on the other material given.
        /// </summary>
        /// <returns>A calculated static friction co-efficient.</returns>
        /// <param name="other">The other surface, if null then no calculation will be performed.</param>
        public float CalculateStaticFriction (PhysicsMaterial other)
        {
            return other ? Calculate (staticFriction, other.staticFriction, frictionCalculation) : staticFriction;
        }

        /// <summary>
        /// Calculates the resitution for the current object based on the other material given.
        /// </summary>
        /// <returns>A calculated restitution value.</returns>
        /// <param name="other">The other surface, if null then no calculation will be performed.</param>
        public float CalculateRestitution (PhysicsMaterial other)
        {
            return other ? Calculate (restitution, other.restitution, restituionCalculation) : restitution;
        }

        /// <summary>
        /// Calculates a value from two given values based on the desired calculation method.
        /// </summary>
        /// <param name="lhs">The first value.</param>
        /// <param name="rhs">The second value.</param>
        /// <param name="calculation">How the final value should be calculated.</param>
        public static float Calculate (float lhs, float rhs, PhysicsMaterialCombine calculation)
        {
            switch (calculation)
            {
                case PhysicsMaterialCombine.Average:
                    return (lhs + rhs) / 2f;
                case PhysicsMaterialCombine.Multiply:
                    return lhs * rhs;
                case PhysicsMaterialCombine.Maximum:
                    return Mathf.Max (lhs, rhs);
                case PhysicsMaterialCombine.Minimum:
                    return Mathf.Min (lhs, rhs);
                default:
                    throw new System.NotImplementedException();
            }
        }

        #endregion
    }
}
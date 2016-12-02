using MonoBehaviour     = UnityEngine.MonoBehaviour;
using SerializeField    = UnityEngine.SerializeField;
using Tooltip           = UnityEngine.TooltipAttribute;

using System.Collections.Generic;


namespace PSI
{
    /// <summary>
    /// Contains all PhysicsMaterial objects in the scene.
    /// </summary>
    public sealed class PhysicsMaterials : MonoBehaviour
    {
        /// <summary>
        /// Maps material names to properties.
        /// </summary>
        [SerializeField, Tooltip ("All available materials in the scene.")]
        Dictionary<string, PhysicsMaterial> m_materials = new Dictionary<string, PhysicsMaterial>();

        /// <summary>
        /// Obtains a material from the given ID. If the material doesn't exist then null will be returned.
        /// </summary>
        /// <param name="id">The name ID of the material to retrieve.</param>
        public PhysicsMaterial this[string id]
        {
            get { return m_materials.ContainsKey (id) ? m_materials[id] : null; }
        }
    }
}
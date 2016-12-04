using AddComponentMenu  = UnityEngine.AddComponentMenu;
using MonoBehaviour     = UnityEngine.MonoBehaviour;
using SerializeField    = UnityEngine.SerializeField;
using Tooltip           = UnityEngine.TooltipAttribute;

using System;
using System.Collections.Generic;


namespace PSI
{
    /// <summary>
    /// Contains all PhysicsMaterial objects in the scene.
    /// </summary>
    [AddComponentMenu ("PSI/Physics Materials")]
    public sealed class PhysicsMaterials : MonoBehaviour
    {
        [Serializable]
        private struct Pair
        {
            public string ID;
            public PhysicsMaterial material;
        }

        [SerializeField, Tooltip ("All available materials in the scene.")]
        List<Pair> m_editorValues = new List<Pair>();

        /// <summary>
        /// Maps material names to properties.
        /// </summary>
        Dictionary<string, PhysicsMaterial> m_materials = new Dictionary<string, PhysicsMaterial>();

        /// <summary>
        /// Obtains a material from the given ID. If the material doesn't exist then null will be returned.
        /// </summary>
        /// <param name="id">The name ID of the material to retrieve.</param>
        public PhysicsMaterial this[string id]
        {
            get { return m_materials.ContainsKey (id) ? m_materials[id] : null; }
        }

        void Awake()
        {
            foreach (var pair in m_editorValues)
            {
                if (!m_materials.ContainsKey (pair.ID))
                {
                    m_materials[pair.ID] = pair.material;
                }
            }

            m_editorValues.Clear();
        }
    }
}
using UnityEngine.Assertions;
using UnityEngine;


/// <summary>
/// Effectively both a game controller and the manager of the Physics system used in the scene. Simulation
/// maintains the flow of the white box testing ground for the Physics system and controls when physics updates
/// are performed.
/// </summary>
public class Simulation : MonoBehaviour
{
    #region Members

    /// <summary>
    /// The PhysicsSystem object attached to the GameObject.
    /// </summary>
    PSI.PhysicsSystem m_physics = null;

    /// <summary>
    /// Determines whether we should run a physics update on the current frame.
    /// </summary>
    bool m_updatePhysics = true;

    #endregion

    #region Game management

    /// <summary>
    /// Initialises the PhysicsSystem component on the current GameObject.
    /// </summary>
    void Awake()
    {
        // Ensure we have a system to initialise.
        m_physics = GetComponent<PSI.PhysicsSystem>();

        Assert.IsNotNull (m_physics, "PhysicsSystem object not available.");
        Assert.IsTrue (m_physics.Initialise(), "PhysicsSystem failed to initialise.");

        m_physics.SetAsDefault();
    }

    /// <summary>
    /// We use a three-stage update process, these stages are called pre-update, main and post-update.
    /// </summary>
    void FixedUpdate()
    {
        if (m_updatePhysics)
        {
            m_physics.PreUpdate (Time.deltaTime);
            m_physics.MainUpdate (Time.deltaTime);
            m_physics.PostUpdate (Time.deltaTime);
        }
    }

    #endregion
}

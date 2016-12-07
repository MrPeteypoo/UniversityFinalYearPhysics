using UnityEngine.Assertions;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// Effectively both a game controller and the manager of the Physics system used in the scene. Simulation
/// maintains the flow of the white box testing ground for the Physics system and controls when physics updates
/// are performed.
/// </summary>
[RequireComponent (typeof (PSI.PhysicsImplementation)), RequireComponent (typeof (PSI.PhysicsMaterials))]
public sealed class Simulation : MonoBehaviour
{
    #region Members

    /// <summary>
    /// The PhysicsSystem object attached to the GameObject.
    /// </summary>
    PSI.PhysicsSystem m_physics = null;

    /// <summary>
    /// Determines whether we should run a physics update on the current frame.
    /// </summary>
    bool m_updatePhysics = false;

    /// <summary>
    /// Whether the physics system should process a single frame and then pause.
    /// </summary>
    bool m_singleFrame = false;

    /// <summary>
    /// The prefab to instantiate when shooting a sphere.
    /// </summary>
    [SerializeField, Tooltip ("The prefab to instantiate when shooting a sphere.")]
    GameObject m_prefab = null;

    [SerializeField]
    GameObject m_camera = null;

    float m_kineticFriction = 0.5f;
    float m_staticFriction = 0.5f;
    float m_restitution = 0.5f;
    PhysicMaterialCombine m_frictionCombine = PhysicMaterialCombine.Average;
    PhysicMaterialCombine m_restitutionCombine = PhysicMaterialCombine.Average;
    float m_mass = 10f;
    float m_drag = 0.01f;
    float m_angularDrag = 0.01f;
    float m_x = 15f;
    float m_y = -15f;
    float m_z = 15f;
    ForceMode m_forceMode = ForceMode.Force;

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

            if (m_singleFrame)
            {
                m_updatePhysics = false;
                m_singleFrame = false;
            }
        }
    }

    public void FlipUpdatePhysics()
    {
        m_updatePhysics = !m_updatePhysics;
    }

    public void SkipFrame()
    {
        m_updatePhysics = true;
        m_singleFrame = true;
    }

    public void ResetLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene (0);
    }

    public void ShootSphere()
    {
        var sphere = GameObject.Instantiate (m_prefab, m_camera.transform.position, m_camera.transform.rotation) 
            as GameObject;
        
        var rigidbody   = sphere.GetComponent<PSI.Rigidbody>();
        var collider    = sphere.GetComponent<PSI.Collider>();

        rigidbody.mass = m_mass;
        rigidbody.drag = m_drag;
        rigidbody.angularDrag = m_angularDrag;

        var material = new PSI.PhysicsMaterial();
        material.kineticFriction = m_kineticFriction;
        material.staticFriction = m_staticFriction;
        material.restitution = m_restitution;
        material.frictionCalculation = m_frictionCombine;
        material.restituionCalculation = m_restitutionCombine;

        collider.material = material;

        var forcePoint = rigidbody.position - sphere.transform.forward + sphere.transform.up;
        rigidbody.AddForceAtPoint (new Vector3 (m_x, m_y, m_z), forcePoint, m_forceMode);
    }

    public void SetKinetic (string value)
    {
        m_kineticFriction = float.Parse (value);
    }

    public void SetStatic (string value)
    {
        m_staticFriction = float.Parse (value);
    }

    public void SetRestitution (string value)
    {
        m_restitution = float.Parse (value);
    }

    public void SetFrictionCombine (int value)
    {
        switch (value)
        {
            case 0:
                m_frictionCombine = PhysicMaterialCombine.Average;
                break;
            case 1:
                m_frictionCombine = PhysicMaterialCombine.Multiply;
                break;
            case 2:
                m_frictionCombine = PhysicMaterialCombine.Maximum;
                break;
            case 3:
                m_frictionCombine = PhysicMaterialCombine.Minimum;
                break;
        }
    }

    public void SetRestitutionCombine (int value)
    {
        switch (value)
        {
            case 0:
                m_restitutionCombine = PhysicMaterialCombine.Average;
                break;
            case 1:
                m_restitutionCombine = PhysicMaterialCombine.Multiply;
                break;
            case 2:
                m_restitutionCombine = PhysicMaterialCombine.Maximum;
                break;
            case 3:
                m_restitutionCombine = PhysicMaterialCombine.Minimum;
                break;
        }
    }

    public void SetMass (string value)
    {
        m_mass = float.Parse (value);
    }

    public void SetDrag (string value)
    {
        m_drag = float.Parse (value);
    }

    public void SetAngularDrag (string value)
    {
        m_angularDrag = float.Parse (value);
    }

    public void SetForceMode (int value)
    {
        switch (value)
        {
            case 0:
                m_forceMode = ForceMode.Force;
                break;
            case 1:
                m_forceMode = ForceMode.Acceleration;
                break;
            case 2:
                m_forceMode = ForceMode.Impulse;
                break;
            case 3:
                m_forceMode = ForceMode.VelocityChange;
                break;
        }
    }

    public void SetX (string value)
    {
        m_x = float.Parse (value);
    }

    public void SetY (string value)
    {
        m_y = float.Parse (value);
    }

    public void SetZ (string value)
    {
        m_z = float.Parse (value);
    }

    #endregion
}

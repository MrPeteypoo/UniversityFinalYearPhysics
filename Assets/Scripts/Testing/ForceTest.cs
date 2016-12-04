using UnityEngine;
using System.Collections;

public class ForceTest : MonoBehaviour
{
    Vector3 m_start = Vector3.zero;
    Vector3 m_end = Vector3.zero;
    float m_time = 0f;
    [SerializeField, Range (0f, 100f)] float m_force = 5f;
    [SerializeField, Range (0f, 100f)] float m_distance = 10f;

    PSI.Rigidbody m_psi = null;
    Rigidbody m_unity = null;

    bool m_printResult = true;

    void Start()
    {
        m_start = transform.position;
        m_end = m_start + transform.forward * m_distance;

        m_psi = GetComponent<PSI.Rigidbody>();
        m_unity = GetComponent<Rigidbody>();
    }

    // Use this for initialization
    void FixedUpdate()
    {
        if (m_end.z > transform.position.z)
        {
            m_time += Time.deltaTime;

            if (m_psi.enabled)
            {
                m_psi.AddForce (transform.forward * m_force);
                m_psi.AddTorque (transform.forward * m_force);
            }
            else
            {
                m_unity.AddForce (transform.forward * m_force);
            }
        }
        else if (m_printResult)
        {
            Debug.Log ("Time taken: " + m_time + ", height: " + transform.position.y);
            m_printResult = false;
        }
    }
}

using UnityEngine;
using System.Collections;

public class HeightTracker : MonoBehaviour {
    float m_time = 0f;
	// Use this for initialization
	void Start () {
        Debug.Log ("Starting height: " + transform.position.y);
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if (m_time >= 1f)
        {
            Debug.Log ("Height: " + transform.position.y);
            m_time -= 1f;
        }
        m_time += Time.deltaTime;
	}
}

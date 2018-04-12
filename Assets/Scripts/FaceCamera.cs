using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour {

    Camera m_Camera;

    void Awake()
    {
        m_Camera = Camera.main;
    }

    void Update()
    {
        Vector3 targetVector = this.transform.position - m_Camera.transform.position;
        transform.rotation = Quaternion.LookRotation(targetVector, m_Camera.transform.rotation * Vector3.up);
    }

    public void DestroyInTime(float timeAlive)
    {
        StartCoroutine(DestroyIE(timeAlive));
    }

    IEnumerator DestroyIE(float timeAlive)
    {
        yield return new WaitForSeconds(timeAlive);
        Destroy(gameObject);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public MeshRenderer mesh;

    private void Awake()
    {
        mesh = GetComponent<MeshRenderer>();
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name == "Cube")
        {
            other.gameObject.GetComponent<MeshRenderer>().sharedMaterial = mesh.sharedMaterial;
            Destroy(other);
        }
    }
}

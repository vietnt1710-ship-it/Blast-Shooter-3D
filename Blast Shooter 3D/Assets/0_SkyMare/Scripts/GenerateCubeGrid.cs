using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class GenerateCubeGrid : MonoBehaviour
{
    public int sizeX = 20;
    public int sizeY = 20;
    public int sizeZ = 20;
    public float cubeScale = 0.1f;
    public float cubeScaleTransform = 0.11f;
    public List<GameObject> cubeLists = new List<GameObject>();
    void Start()
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

                    cube.transform.localScale = Vector3.one * cubeScale;
                    cube.transform.position = new Vector3(
                        x * cubeScaleTransform,
                        y * cubeScaleTransform,
                        z * cubeScaleTransform
                    );

                    cube.transform.parent = transform;
                    cubeLists.Add( cube );                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           
                }
            }
        }
        DOVirtual.DelayedCall(5, () =>
        {
            UnLoad();
        });
    }

    public void UnLoad()
    {
        for(int i = 0;i < cubeLists.Count; i++)
        {
            if (cubeLists[i].GetComponent<Collider>() != null)
            {
                cubeLists[i].gameObject.SetActive(false);
            }
        }
    }
}

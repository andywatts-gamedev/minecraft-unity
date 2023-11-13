using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Place : MonoBehaviour
{
    public GameObject Torch;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log("---------");
                Debug.Log("Hit: " + hit.point);
                
                var floor = new float3(Mathf.Floor(hit.point.x), Mathf.Floor(hit.point.y), Mathf.Floor(hit.point.z));
                Debug.Log(floor);
                
                var offset = new float3(0.5f, 0.5f, 0.5f);
                Debug.Log(offset);

                var xyz = floor + offset;
                Debug.Log(xyz);
                    
                GameObject.Instantiate(Torch, xyz, Quaternion.identity);
            }
        }
    }
    
}

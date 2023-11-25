using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;
using Unity.Mathematics;
using System.Threading.Tasks;

public class Render3D : MonoBehaviour
{
    private static Render3D _instance;
    public static Render3D Instance { get { return _instance; } }

    public static bool busy;
    public Camera cam;
    EntityManager em;
    public Transform stage;

    public RenderTexture renderTexture;

    public Texture2D Snapshot(GameObject go)
    {
        // Reset stage
        stage.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        
        // Position gameObject
        go.transform.parent = stage;
        var bounds = go.GetComponent<MeshFilter>().mesh.bounds;
        go.transform.position = -bounds.center;
        go.GetComponent<MeshRenderer>().renderingLayerMask = (uint) LayerMask.NameToLayer("Render3D");
        go.layer = (int) LayerMask.NameToLayer("Render3D");
        
        // Rotate stage
        stage.transform.rotation = Quaternion.Euler(15f, -130f, 15f);
        
        cam.targetTexture = renderTexture;
        cam.Render();

        // Copy temp renderTexture
        RenderTexture saveActive = RenderTexture.active;
        RenderTexture.active = cam.targetTexture;
        Texture2D texture = new Texture2D(cam.targetTexture.width, cam.targetTexture.height, TextureFormat.ARGB32, false, true);
        texture.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
        texture.Apply();
        RenderTexture.active = saveActive;

        // Release temporary
        cam.targetTexture = null;
        RenderTexture.ReleaseTemporary(cam.targetTexture);
        return texture;
    }

    void PositionCamera(Bounds bounds)
    {
        float cameraDistance = 2.0f; // Constant factor

        //
        Vector3 objectSizes = bounds.max - bounds.min;

        float objectSize = Mathf.Max(objectSizes.x, objectSizes.y, objectSizes.z);

        // Visible height 1 meter in front
        float cameraView = 2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad * cam.fieldOfView);

        // Combined wanted distance from the object
        float distance = cameraDistance * objectSize / cameraView;

        // Estimated offset from the center to the outside of the object
        distance += 0.5f * objectSize;

        cam.transform.position = bounds.center - distance * cam.transform.forward;
    }

    private void Start()
    {
    }

    private void Awake()
    {
        _instance = this;
    }

}

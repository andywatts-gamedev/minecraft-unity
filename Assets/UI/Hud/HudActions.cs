using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class HudActions : MonoBehaviour
{
    public GameObject Torch;

    public void Place(InputAction.CallbackContext context)
    {
        var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            var floor = new float3(Mathf.Floor(hit.point.x), Mathf.Floor(hit.point.y), Mathf.Floor(hit.point.z));
            var offset = new float3(0.5f, 0.5f, 0.5f);
            var xyz = floor + offset;
            // Instantiate(Torch, xyz, Quaternion.identity);
            Debug.Log("Got place " + xyz);
        }
    }
    
    void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
    }

}
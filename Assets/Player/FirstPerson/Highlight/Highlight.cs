using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class Highlight : MonoBehaviour
{
    
    private static Highlight _instance;
    public static Highlight Instance => _instance;
    
    public int3 highlightXyz;
    public int3 blueprintStartXyz;

    private void Awake()
    {
        _instance = this;
    }

    private void Update()
    {
    }

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }

    public void MoveHighlight()
    {
        var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            var hitVector3 = hit.point - hit.normal * 0.05f;
            var floor = new float3(Mathf.Floor(hitVector3.x), Mathf.Floor(hitVector3.y), Mathf.Floor(hitVector3.z));
            var offset = new float3(0.5f, 0.5f, 0.5f);
            highlightXyz = new int3(floor + offset);
            if (!transform.position.Equals(floor + offset))
                transform.position = floor + offset;
        }
    }

    public void Place(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit)) return;
        var Voxel = (hit.point - hit.normal * 0.05f).ToInt3();
        var VoxelAdjacent = (hit.point + hit.normal * 0.05f).ToInt3();
        var blockIndex = (ushort) Blocks.Instance.blocks.FindIndex(b => b == Toolbar.Instance.selectedBlock);
        World.Instance.voxels[VoxelAdjacent.ToIndex(World.Instance.dims)] = blockIndex;
        World.Instance.UpdateMesh();
    }

    public void Remove(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit)) return;
        var voxel = (hit.point - hit.normal * 0.05f).ToInt3();
        World.Instance.voxels[voxel.ToIndex(World.Instance.dims)] = (ushort) Blocks.Instance.blocks.FindIndex(b => b == Blocks.Instance.Air);
        World.Instance.UpdateMesh();
    }

    public void Blueprint(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (blueprintStartXyz.Equals(default))
            blueprintStartXyz = highlightXyz;
        else
        {
            var blueprint = ScriptableObject.CreateInstance<Blueprint>();
            blueprint.Name = "blueprint";
            var startXyz = new int3(math.min(blueprintStartXyz.x, highlightXyz.x), math.min(blueprintStartXyz.y, highlightXyz.y), math.min(blueprintStartXyz.z, highlightXyz.z));
            var endXyz = new int3(math.max(blueprintStartXyz.x, highlightXyz.x), math.max(blueprintStartXyz.y, highlightXyz.y), math.max(blueprintStartXyz.z, highlightXyz.z));
            blueprint.Dims = endXyz - startXyz;
            var blueprintSize = (blueprint.Dims.x+1) * (blueprint.Dims.y+1) * (blueprint.Dims.z+1);
            blueprint.blocks = new Block[blueprintSize];
            
            for (var z = 0; z <= blueprint.Dims.z; z++)
            for (var y = 0; y <= blueprint.Dims.y; y++)
            for (var x = 0; x <= blueprint.Dims.x; x++)
            {
                var xyz = new int3(x, y, z);
                var index = xyz.ToIndex(blueprint.Dims);
                var worldIndex = (startXyz + xyz).ToIndex(World.Instance.dims);
                var blockIndex = World.Instance.voxels[worldIndex];
                blueprint.blocks[index] = Blocks.Instance.blocks[blockIndex];  // TODO will need to update for rotation/palette
            }
            blueprintStartXyz = default;
            
            var datetime = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            var path = $"Assets/Resources/Blueprints/blueprint-{datetime}.asset";
            UnityEditor.AssetDatabase.CreateAsset(blueprint, path);
            Debug.Log($"Created blueprint at {path}");
        }
    }

}
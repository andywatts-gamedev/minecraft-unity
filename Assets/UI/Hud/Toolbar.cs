using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

public class Toolbar : MonoBehaviour
{
    public int selectedItem;
    public List<Block> blocks;
    public GameObject blockPrefab;
    private VisualElement root;
    private UQueryBuilder<VisualElement> slots;
    private VisualElement toolbar;
    public static Toolbar Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        root = gameObject.GetComponent<UIDocument>().rootVisualElement;
        toolbar = root.Q<VisualElement>("toolbar");
        slots = toolbar.Query<VisualElement>("toolbarSlot");

        var blockGO = Instantiate(blockPrefab);
        blockGO.transform.position = new Vector3(-0.0f, -0.2f, -0f);
        blockGO.transform.rotation = Quaternion.Euler(15f, 180+47, 15f);
        var meshFilter = blockGO.GetComponent<MeshFilter>();
        blockGO.GetComponent<MeshRenderer>().materials[0].SetTexture("_TextureArray", Textures.Instance.opaqueTexture2DArray);

        foreach (var (block, slotIndex) in blocks.Select((value, i) => (value, i)))
        {
            meshFilter.mesh = block.GenerateMesh();
            
            var texture = Render3D.Instance.Snapshot(blockGO);
            var slot = slots.AtIndex(slotIndex);
            var image = new Image();
            image.image = texture; 
            if (slot.childCount > 0)
                slot.RemoveAt(0);
            slot.Add(image);
        }
        SetActiveToolbarItem(0);
    }

    // Called from Dashboard
    public void SetToolbarItem(int slotIndex, Texture blockImage)
    {
        slotIndex = slotIndex - 1;
        if (slotIndex == -1)
            slotIndex = 9;

        var slot = slots.AtIndex(slotIndex);
        var image = new Image();
        image.image = blockImage;
        if (slot.childCount > 0)
            slot.RemoveAt(0);
        slot.Add(image);
    }

    public void SetActiveToolbarItem(int slotIndex)
    {
        selectedItem = slotIndex;
        for (var i = 0; i < 10; i++)
            slots.AtIndex(i).RemoveFromClassList("toolbarSlotActive");
        slots.AtIndex(slotIndex).AddToClassList("toolbarSlotActive");
    }
}
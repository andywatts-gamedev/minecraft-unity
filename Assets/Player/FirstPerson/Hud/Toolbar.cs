using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UIElements;

public class Toolbar : MonoBehaviour
{
    public int selectedItem;
    public Block selectedBlock;
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
        var meshFilter = blockGO.GetComponent<MeshFilter>();
        blockGO.GetComponent<MeshRenderer>().materials[0].SetTexture("_TextureArray", Textures.Instance.opaqueTexture2DArray);

        // Populate toolbar
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
        SetActiveItem(0);
    }

    // Called from Dashboard
    public void SetItem(int slotIndex, Texture blockImage)
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
    
    public void SetActiveItem(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (((KeyControl) context.control).keyCode == Key.Digit1)
            SetActiveItem(0);
        else if (((KeyControl) context.control).keyCode == Key.Digit2)
            SetActiveItem(1);
        else if (((KeyControl) context.control).keyCode == Key.Digit3)
            SetActiveItem(2);
        else if (((KeyControl) context.control).keyCode == Key.Digit4)
            SetActiveItem(3);
        else if (((KeyControl) context.control).keyCode == Key.Digit5)
            SetActiveItem(4);
        else if (((KeyControl) context.control).keyCode == Key.Digit6)
            SetActiveItem(5);
        else if (((KeyControl) context.control).keyCode == Key.Digit7)
            SetActiveItem(6);
        else if (((KeyControl) context.control).keyCode == Key.Digit8)
            SetActiveItem(7);
        else if (((KeyControl) context.control).keyCode == Key.Digit9)
            SetActiveItem(8);
        else if (((KeyControl) context.control).keyCode == Key.Digit0)
            SetActiveItem(9);
    }

    public void SetActiveItem(int slotIndex)
    {
        selectedItem = slotIndex;
        selectedBlock = blocks[slotIndex];
        for (var i = 0; i < 10; i++)
            slots.AtIndex(i).RemoveFromClassList("toolbarSlotActive");
        slots.AtIndex(slotIndex).AddToClassList("toolbarSlotActive");
    }
}
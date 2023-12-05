using System;
using Unity.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "BlockState", menuName = "Oasis/BlockState")]
public class BlockState : ScriptableObject
{
    public Block Block;
    public State[] States;
    public Model Model;
    public int Y;
}

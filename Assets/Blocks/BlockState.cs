using System;
using Unity.Collections;
using UnityEngine;

[Serializable]
public struct BlockState
{
    public Block Block;
    public FixedList512Bytes<State> States;
    // [SerializeField] public State[] States;
}

public struct State : IEquatable<State>
{
    public FixedString32Bytes Key;
    public FixedString32Bytes Value;

    public override bool Equals(object obj)
    {
        return obj is State other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Key, Value);
    }
        
    public bool Equals(State other)
    {
        return Key == other.Key && Value == other.Value;
    }

    public static bool operator ==(State x, State y)
    {
        return x.Equals(y);
    }

    public static bool operator !=(State x, State y)
    {
        return !x.Equals(y);
    }
}


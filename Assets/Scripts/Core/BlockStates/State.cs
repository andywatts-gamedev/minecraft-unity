using System;
using Unity.Collections;
using UnityEngine;

[Serializable]
public struct State : IEquatable<State>
{
    // TODO - use FixedString32Bytes when inspector supports it
    // public FixedString32Bytes Key;
    // public FixedString32Bytes Value;
    
    public string Key;
    public string Value;

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


using UnityEngine;

public class TreasureChest : MonoBehaviour
{
    // remaining durability
    private int _durability;
    private int _value;

    public void Initialize(int durability, int value)
    {
        this._durability = durability;
        this._value = value;
    }

    public void Hit()
    {
        _durability--;
        if (_durability <= 0)
        {
            Destroy(gameObject);
            // TODO: treasure chest function, animation, etc...
        }
    }
}
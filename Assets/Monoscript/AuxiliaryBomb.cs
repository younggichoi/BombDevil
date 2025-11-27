using UnityEngine;

public class AuxiliaryBomb : MonoBehaviour
{
    // Explode API
    // plan to add exploding motion (when the art is complete)
    public void Explode()
    {
        Destroy(gameObject);
    }
}

using UnityEngine;

public class Megaphone : MonoBehaviour
{
    public static Vector3? activeMegaphonePosition;

    private void OnEnable()
    {
        activeMegaphonePosition = transform.position;
    }

    private void OnDisable()
    {
        if (activeMegaphonePosition == transform.position)
        {
            activeMegaphonePosition = null;
        }
    }
}

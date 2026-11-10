using UnityEngine;

[ExecuteAlways]
public class BillboardYOnly : MonoBehaviour
{
    private Transform mainCameraTransform;

    void LateUpdate()
    {
        if (mainCameraTransform == null)
        {
            if (Camera.main != null)
            {
                mainCameraTransform = Camera.main.transform;
            }
        }

        if (mainCameraTransform != null)
        {
            transform.rotation = Quaternion.Euler(mainCameraTransform.eulerAngles);
        }
    }
}
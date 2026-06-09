using UnityEngine;
public class screenWrap : MonoBehaviour
{
    private Camera mainCamera;
    private Vector2 screenBounds;
    private float objectWidth;
    private float objectHeight;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = Camera.main;
        screenBounds = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, mainCamera.transform.position.z));
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        objectWidth = sr.bounds.extents.x;
        objectHeight = sr.bounds.extents.y;
    }

    void LateUpdate()
    {
        Vector3 viewPos = transform.position;
        // batas horizontal
        if (viewPos.x > screenBounds.x + objectWidth)
        {
            viewPos.x = -screenBounds.x - objectWidth;
        }
        else if (viewPos.x < -screenBounds.x - objectWidth)
        {
            viewPos.x = screenBounds.x + objectWidth;
        }

        // batas vertikal
        if (viewPos.y > screenBounds.y + objectHeight)
        {
            viewPos.y = -screenBounds.y - objectHeight;
        }
        else if (viewPos.y < -screenBounds.y - objectHeight)
        {
            viewPos.y = screenBounds.y + objectHeight;
        }
        transform.position = viewPos;
    }
}

using UnityEngine;

public class ArrowController : MonoBehaviour
{

    void Update()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        transform.position = mousePosition;
    }

    private void FixedUpdate()
    {
        if (Cursor.visible)
        {
            Cursor.visible = false;
        }
    }
}
using UnityEngine;

namespace ItemCompare.Patches;

public class TooltipFollower : MonoBehaviour
{
    public RectTransform target;
    public Vector2 offset = new Vector2(100, 0); // Adjust based on your needs

    void LateUpdate()
    {
        if (target != null)
        {
            RectTransform rt = this.GetComponent<RectTransform>();
            rt.position = target.position + (Vector3)offset;

            // Optionally, clamp to screen as needed
            Utils.ClampUIToScreen(rt);
        }
    }
}
using UnityEngine;

public class DetectZone : MonoBehaviour
{
    public bool inDetactZone;

    [Header("References")]
    public RectTransform handleRect;
    public RectTransform detectZoneRect;

    void Update()
    {
        if (IsOverlapping(handleRect, detectZoneRect))
        {
            inDetactZone = true;
        }
        else
        {
            inDetactZone = false;
        }
    }

    private bool IsOverlapping(RectTransform rect1, RectTransform rect2)
    {
        //Get the four edge of the gameObject
        Vector3[] corners1 = new Vector3[4];
        Vector3[] corners2 = new Vector3[4];

        //Convert those edge into rectangles
        rect1.GetWorldCorners(corners1);
        rect2.GetWorldCorners(corners2);

        //Get rectangles position & size
        //corners[0]: bottom - left
        //corners[1]: top - left
        //corners[2]: top - right
        //corners[3]: bottom - right
        Rect r1 = new Rect(corners1[0], corners1[2] - corners1[0]);
        Rect r2 = new Rect(corners2[0], corners2[2] - corners2[0]);

        //Check if r1 is overlaping with r2
        return r1.Overlaps(r2);
    }
}
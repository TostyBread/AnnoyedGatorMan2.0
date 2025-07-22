// LabelDisplay.cs
using TMPro;
using UnityEngine;

public class LabelDisplay : MonoBehaviour
{
    [Header("Label Anchor")]
    public Transform anchorPoint;

    [Header("Label Settings")]
    public string labelText;
    public Color labelColor = Color.red;
    public int fontSize = 8;
    public Vector3 offset = Vector3.zero;
    public TMP_FontAsset customFontAsset; // Load your custom font asset here

    private TextMeshPro label;

    void Start()
    {
        CreateLabel();
    }

    public void SetLabel(string text)
    {
        labelText = text;
        if (label != null)
        {
            label.text = text;
        }
    }

    public void SetLabelFromId(int id)
    {
        SetLabel(id.ToString());
    }

    public void DisableLabel()
    {
        if (label != null)
        {
            label.gameObject.SetActive(false);
        }
    }

    private void CreateLabel()
    {
        if (anchorPoint == null)
        {
            Debug.LogWarning($"LabelDisplay on {gameObject.name} has no anchorPoint assigned.");
            return;
        }

        GameObject labelObj = new GameObject("WorldLabel");
        labelObj.transform.SetParent(anchorPoint, false);
        labelObj.transform.localPosition = offset;
        labelObj.transform.localRotation = Quaternion.identity;
        labelObj.transform.localScale = Vector3.one;

        label = labelObj.AddComponent<TextMeshPro>();

        if (customFontAsset != null)
        {
            label.font = customFontAsset; // customFontAsset will be use here
        }

        label.text = labelText;
        label.fontSize = fontSize;
        label.color = labelColor;
        label.alignment = TextAlignmentOptions.Center;
        label.sortingOrder = 100;
    }

    void LateUpdate()
    {
        if (label != null)
        {
            label.transform.rotation = Quaternion.identity;
        }
    }
}
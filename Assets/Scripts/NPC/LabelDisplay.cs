// LabelDisplay.cs
using TMPro;
using UnityEngine;

public class LabelDisplay : MonoBehaviour
{
    [Header("Label Settings")]
    public string labelText;
    public Color labelColor = Color.red;
    public Vector3 offset = Vector3.zero;
    public TMP_FontAsset customFontAsset;

    [Header("ID GameObject Prefab")]
    public GameObject idGameObjectPrefab; // Assign the ID GameObject prefab (ID Tag > Canvas > ID)
    public float idGameObjectScale = 1.1f; // Scale multiplier for the spawned ID GameObject

    private TextMeshPro label;
    private TextMeshProUGUI labelUI;
    private GameObject idGameObject;
    private bool usingExistingLabel = false;

    void Start()
    {
        InitializeLabel();
    }

    private void InitializeLabel()
    {
        // First, try to find an existing TMP component in children (world space)
        label = GetComponentInChildren<TextMeshPro>();
        
        // If not found, try TextMeshProUGUI (UI component)
        if (label == null)
        {
            labelUI = GetComponentInChildren<TextMeshProUGUI>();
            if (labelUI != null)
            {
                usingExistingLabel = true;
                ConfigureLabelUI();
            }
        }
        
        if (label != null)
        {
            usingExistingLabel = true;
            ConfigureLabel();
        }
        else if (labelUI == null)
        {
            // If no existing label, try to spawn from prefab
            if (idGameObjectPrefab != null)
            {
                SpawnIdGameObject();
            }
            else
            {
                Debug.LogWarning($"LabelDisplay on {gameObject.name} has no existing TMP/TextMeshProUGUI and no idGameObjectPrefab assigned.");
            }
        }
    }

    private void SpawnIdGameObject()
    {
        // Instantiate the ID GameObject prefab as a child of this object
        idGameObject = Instantiate(idGameObjectPrefab, transform);
        idGameObject.transform.localPosition = offset;
        idGameObject.transform.localRotation = Quaternion.identity;
        idGameObject.transform.localScale = Vector3.one * idGameObjectScale; // NEW: Apply scale multiplier

        // Try to find TextMeshPro (world space) first
        label = idGameObject.GetComponentInChildren<TextMeshPro>();
        
        // If not found, try TextMeshProUGUI (UI)
        if (label == null)
        {
            labelUI = idGameObject.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (label != null)
        {
            ConfigureLabel();
        }
        else if (labelUI != null)
        {
            ConfigureLabelUI();
        }
        else
        {
            Debug.LogWarning($"ID GameObject prefab on {gameObject.name} does not contain TextMeshPro or TextMeshProUGUI in its children hierarchy.");
        }
    }

    public void SetLabel(string text)
    {
        labelText = text;
        if (label != null)
        {
            label.text = text;
        }
        else if (labelUI != null)
        {
            labelUI.text = text;
        }
    }

    public void SetLabelFromId(int id)
    {
        SetLabel(id.ToString());
    }

    public void DisableLabel()
    {
        if (idGameObject != null)
        {
            idGameObject.SetActive(false);
        }
        else if (label != null)
        {
            label.gameObject.SetActive(false);
        }
        else if (labelUI != null)
        {
            labelUI.gameObject.SetActive(false);
        }
    }

    private void ConfigureLabel()
    {
        if (label == null) return;

        if (customFontAsset != null)
            label.font = customFontAsset;

        label.text = labelText;
        label.color = labelColor;
        label.alignment = TextAlignmentOptions.Center;
        label.sortingOrder = 10;
    }

    private void ConfigureLabelUI()
    {
        if (labelUI == null) return;

        if (customFontAsset != null)
            labelUI.font = customFontAsset;

        labelUI.text = labelText;
        labelUI.color = labelColor;
        labelUI.alignment = TextAlignmentOptions.Center;
    }

    void LateUpdate()
    {
        if (label != null && !usingExistingLabel && idGameObject != null)
        {
            idGameObject.transform.rotation = Quaternion.identity;
        }
    }
}
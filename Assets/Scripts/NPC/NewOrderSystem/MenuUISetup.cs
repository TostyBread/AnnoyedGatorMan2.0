using UnityEngine;
using TMPro;

public class MenuUISetup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI idText;

    public void SetupFromNPC(NPCBehavior npc)
    {
        var label = GetComponentInChildren<LabelDisplay>();
        if (label) label.SetLabelFromId(npc.customerId);

        if (idText != null)
        {
            idText.text = npc.customerId.ToString();
        }
        else
        {
            // Try to find a Canvas child to use as the parent for the ID text
            Canvas canvas = GetComponentInChildren<Canvas>();
            Transform parentTransform = canvas != null ? canvas.transform : transform;

            GameObject idObj = new GameObject("CustomerID");
            idObj.transform.SetParent(parentTransform, false);
            var tmp = idObj.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 14;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.text = npc.customerId.ToString();
            tmp.rectTransform.anchoredPosition = new Vector2(0, -30);
        }

        var patience = npc.GetComponent<NPCPatience>();
        var patienceBar = GetComponentInChildren<PatienceBarController>();
        if (patience && patienceBar) patience.patienceBar = patienceBar;

        npc.AttachMenu(gameObject);
        npc.TriggerPatience();
    }
}
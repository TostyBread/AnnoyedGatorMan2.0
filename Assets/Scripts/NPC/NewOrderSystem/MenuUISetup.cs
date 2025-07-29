using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MenuUISetup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI idText;
    [Header("NPC Visuals")]
    [SerializeField] private Image npcImageTarget;
    [SerializeField] private Sprite[] npcTypeImages;

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

        if (npcImageTarget != null && npcTypeImages != null && npcTypeImages.Length > 0)
        {
            int index = Mathf.Clamp(npc.customerType, 0, npcTypeImages.Length - 1);
            npcImageTarget.sprite = npcTypeImages[index];
        }

        var patience = npc.GetComponent<NPCPatience>();
        var patienceBar = GetComponentInChildren<PatienceBarController>();
        if (patience && patienceBar) patience.patienceBar = patienceBar;

        npc.AttachMenu(gameObject);
        npc.TriggerPatience();
    }
}
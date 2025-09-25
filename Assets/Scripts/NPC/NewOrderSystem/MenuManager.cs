using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [SerializeField] private Transform[] menuStackPositions;
    [SerializeField] private float slideDuration = 0.3f;
    [SerializeField] private float patienceExtensionPerSlot = 40f;

    private Queue<GameObject> activeMenus = new();
    private readonly Dictionary<NPCBehavior, GameObject> npcMenuMap = new(); // Add this line

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    public void SpawnMenuForNPC(NPCBehavior npc, GameObject menuPrefab)
    {
        if (menuPrefab == null || npc == null || menuStackPositions.Length == 0) return;
        
        // Check if NPC already has a menu
        if (npcMenuMap.ContainsKey(npc))
        {
            Debug.LogWarning("NPC already has a menu!");
            return;
        }

        int slot = activeMenus.Count;
        if (slot >= menuStackPositions.Length)
        {
            Debug.LogWarning("No available menu slots");
            return;
        }

        Vector3 spawnPos = menuStackPositions[slot].position;
        Vector3 offscreenPos = spawnPos + Vector3.down * 4f;
        GameObject menu = Instantiate(menuPrefab, offscreenPos, Quaternion.identity);

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null) menu.transform.SetParent(canvas.transform, false);

        StartCoroutine(SlideToPosition(menu.transform, spawnPos));

        var uiSetup = menu.GetComponent<MenuUISetup>();
        if (uiSetup != null)
        {
            uiSetup.SetupFromNPC(npc);
            npcMenuMap[npc] = menu; // Add this line
        }

        var patience = npc.GetComponent<NPCPatience>();
        if (patience != null)
        {
            patience.patienceDuration += patienceExtensionPerSlot * slot;
            patience.ResetPatience();
            patience.StartPatience();
        }

        activeMenus.Enqueue(menu);
    }

    public void RemoveMenuForNPC(NPCBehavior npc)
    {
        if (npc == null || !npcMenuMap.ContainsKey(npc)) return;

        GameObject menuToRemove = npcMenuMap[npc];
        npcMenuMap.Remove(npc);
        
        Queue<GameObject> newQueue = new();
        while (activeMenus.Count > 0)
        {
            GameObject current = activeMenus.Dequeue();
            if (current != menuToRemove)
            {
                newQueue.Enqueue(current);
            }
            else
            {
                StartCoroutine(SlideAndDestroy(current));
            }
        }

        activeMenus = newQueue;
        RearrangeMenus();
    }

    public void ClearFrontMenuAnimated()
    {
        if (activeMenus.Count == 0) return;

        GameObject front = activeMenus.Dequeue();
        if (front != null)
        {
            var uiSetup = front.GetComponent<MenuUISetup>();
            if (uiSetup != null && uiSetup.AssociatedNPC != null)
            {
                npcMenuMap.Remove(uiSetup.AssociatedNPC);
            }
            StartCoroutine(SlideAndDestroy(front));
        }

        RearrangeMenus();
    }

    private void RearrangeMenus()
    {
        GameObject[] menus = activeMenus.ToArray();
        for (int i = 0; i < menus.Length && i < menuStackPositions.Length; i++)
        {
            if (menus[i] != null)
            {
                StartCoroutine(SlideToPosition(menus[i].transform, menuStackPositions[i].position));
            }
        }
    }

    private IEnumerator SlideToPosition(Transform obj, Vector3 targetPos)
    {
        if (obj == null) yield break;
        Vector3 startPos = obj.position;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            if (obj == null) yield break;
            obj.position = Vector3.Lerp(startPos, targetPos, elapsed / slideDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (obj != null) obj.position = targetPos;
    }

    private IEnumerator SlideAndDestroy(GameObject menuObj)
    {
        if (menuObj == null) yield break;

        Transform obj = menuObj.transform;
        Vector3 startPos = obj.position;
        Vector3 endPos = startPos + Vector3.down * 4f;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            if (obj == null) yield break;
            obj.position = Vector3.Lerp(startPos, endPos, elapsed / slideDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (menuObj != null) Destroy(menuObj);
    }

    public void ClearAllMenus()
    {
        npcMenuMap.Clear(); // Add this line
        while (activeMenus.Count > 0)
        {
            GameObject menu = activeMenus.Dequeue();
            if (menu != null) StartCoroutine(SlideAndDestroy(menu));
        }
    }
}
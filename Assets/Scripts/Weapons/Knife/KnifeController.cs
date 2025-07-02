using System.Collections;
using UnityEngine;

public class KnifeController : MonoBehaviour, IUsable
{
    [Header("Knife Settings")]
    public float stabDuration = 0.2f;
    public float stabCooldown = 0.5f;
    public Vector3 stabOffset = new Vector3(0.2f, 0, 0);

    [Header("References")]
    public Collider2D knifeCollider;
    public Transform knifeTransform;

    [Header("Do not touch")]
    [SerializeField] private bool isUsable = true;
    [SerializeField] private bool isStabbing = false;
    [SerializeField] private bool isInUsableMode = false;

    private void Start()
    {
        if (knifeCollider == null)
        {
            knifeCollider = GetComponent<Collider2D>();
        }
        if (knifeTransform == null)
        {
            knifeTransform = transform;
        }
        knifeCollider.enabled = false;
    }

    public void ToggleUsableMode(bool enable)
    {
        isInUsableMode = enable;
        knifeTransform.localRotation = Quaternion.Euler(0, 0, isInUsableMode ? -90f : 0f);
        Debug.Log(isInUsableMode ? "Knife in stab mode." : "Knife in normal mode.");
    }
    public bool IsInUsableMode() => isInUsableMode;
    public void Use()
    {
        if (!isUsable || !isInUsableMode || isStabbing) return;

        AudioManager.Instance.PlaySound("slash1", 1.0f, transform.position);
        StartCoroutine(PerformStab());
    }

    private IEnumerator PerformStab()
    {
        isStabbing = true;
        isUsable = false;
        knifeCollider.enabled = true;
        Vector3 originalPosition = knifeTransform.localPosition;
        knifeTransform.localPosition += stabOffset;

        yield return new WaitForSeconds(stabDuration);

        knifeCollider.enabled = false;
        knifeTransform.localPosition = originalPosition;

        yield return new WaitForSeconds(stabCooldown);
        isUsable = true;
        isStabbing = false;
    }

    public void EnableUsableFunction()
    {
        isUsable = true;

        // Sync with PlayerInputManager's usable mode
        PlayerInputManager inputManager = FindObjectOfType<PlayerInputManager>();
        if (inputManager != null && inputManager.IsUsableModeEnabled())
        {
            ToggleUsableMode(true);
        }

        Debug.Log("Knife usable function enabled.");
    }

    public void DisableUsableFunction()
    {
        isUsable = false;
        Debug.Log("Knife usable function disabled.");
    }
}
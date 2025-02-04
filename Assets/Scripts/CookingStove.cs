using UnityEngine;

public class CookingStove : MonoBehaviour
{
    [Header("Stove Settings")]
    public GameObject fireCollider; // The fire collider that enables cooking
    public bool isStoveOn = false;

    [Header("Detection Settings")]
    public float activationRange = 2.5f; // Range within which the stove can be turned on
    public string[] ignitableTags = {}; // Tags that can ignite the stove

    private Transform playerTransform; // Reference to the player

    private void Start()
    {
        playerTransform = FindObjectOfType<CharacterMovement>().transform; // Dynamically find player
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(2)) // Middle-click to toggle stove
        {
            Vector2 mouseWorldPos = ScreenToWorldPointMouse.Instance.GetMouseWorldPosition();
            Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);

            if (hit != null && hit.gameObject == gameObject && Vector2.Distance(transform.position, playerTransform.position) <= activationRange)
            {
                ToggleStove();
            }
        }
    }

    public void ToggleStove() // Changed to 'public' so PlayerInputManager can access it
    {
        isStoveOn = !isStoveOn;
        fireCollider.SetActive(isStoveOn);
        Debug.Log($"Stove toggled: {(isStoveOn ? "ON" : "OFF")}");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (string tag in ignitableTags)
        {
            if (collision.gameObject.CompareTag(tag))
            {
                isStoveOn = true;
                fireCollider.SetActive(true);
                Debug.Log("Stove ignited by external fire source.");
                break;
            }
        }
    }
}
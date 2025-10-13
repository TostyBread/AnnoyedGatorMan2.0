using UnityEngine;

public class P2CharacterSelectManager : MonoBehaviour
{
    [Header("Character References")]
    public SpriteRenderer[] sprites;    // UI selection sprites
    public GameObject[] characters;    // Actual character prefabs

    private static int P2characterIndex = 0;
    private static int P2lastCharacterIndex = -1;

    private void Awake()
    {
        // Initialize all characters as inactive
        if (characters != null &&
            P2characterIndex >= 0 &&
            P2characterIndex < characters.Length &&
            characters[P2characterIndex] != null)
        {
            foreach (var character in characters)
            {
                if (character != null) character.SetActive(false);
            }
            characters[P2characterIndex].SetActive(true);
        }

        UpdateP2Character(); // Ensure the correct sprite is active at start
    }

    private void Update()
    {
        if (P2characterIndex != P2lastCharacterIndex)
        {
            UpdateP2Character();
            P2lastCharacterIndex = P2characterIndex;
        }
    }

    private void UpdateP2Character()
    {
        if (sprites == null) return;

        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] != null)
                sprites[i].gameObject.SetActive(i == P2characterIndex);
        }
    }

    public void P2NextCharacter()
    {
        if (sprites == null || sprites.Length == 0) return;

        P2characterIndex++;
        if (P2characterIndex >= sprites.Length) P2characterIndex = 0;
    }

    public void P2PreviousCharacter()
    {
        if (sprites == null || sprites.Length == 0) return;

        P2characterIndex--;
        if (P2characterIndex < 0) P2characterIndex = sprites.Length - 1;
    }
}
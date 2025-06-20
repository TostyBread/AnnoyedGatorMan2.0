using UnityEngine;

public class P1CharacterSelectManager : MonoBehaviour
{
    [Header("Character References")]
    public SpriteRenderer[] sprites;    // UI selection sprites
    public GameObject[] characters;    // Actual character prefabs

    private static int P1characterIndex = 0;
    private static int P1lastCharacterIndex = -1;

    private void Awake()
    {
        // Initialize all characters as inactive
        if (characters != null &&
            P1characterIndex >= 0 &&
            P1characterIndex < characters.Length &&
            characters[P1characterIndex] != null)
        {
            foreach (var character in characters)
            {
                if (character != null) character.SetActive(false);
            }
            characters[P1characterIndex].SetActive(true);
        }
    }

    private void Update()
    {
        if (P1characterIndex != P1lastCharacterIndex)
        {
            UpdateP1Character();
            P1lastCharacterIndex = P1characterIndex;
        }
    }

    private void UpdateP1Character()
    {
        if (sprites == null) return;

        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] != null)
                sprites[i].gameObject.SetActive(i == P1characterIndex);
        }
    }

    public void P1NextCharacter()
    {
        if (sprites != null && P1characterIndex < sprites.Length - 1)
            P1characterIndex++;
    }

    public void P1PreviousCharacter()
    {
        if (P1characterIndex > 0)
            P1characterIndex--;
    }
}
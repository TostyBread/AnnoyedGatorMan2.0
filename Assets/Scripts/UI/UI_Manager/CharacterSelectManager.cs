using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectManager : MonoBehaviour
{
    [Header("Character References")]
    public SpriteRenderer playerSprite;
    public SpriteRenderer[] sprites;

    private static int characterIndex = 0;
    private static int lastCharacterIndex = -1;

    private void Update()
    {
        if (characterIndex != lastCharacterIndex)
        {
            UpdateCharacter();
            lastCharacterIndex = characterIndex;
        }

        if (playerSprite != null)
        {
            playerSprite.sprite = sprites[characterIndex].sprite;
        }

        Debug.Log(characterIndex);
    }

    private void UpdateCharacter()
    {
        for (int i = 0; i < sprites.Length; i++)
        {
            sprites[i].gameObject.SetActive(i == characterIndex);
        }
    }

    public void NextCharacter()
    {
        if (characterIndex < 2) characterIndex++;
    }

    public void PreviousCharacter() 
    {
        if (characterIndex > 0) characterIndex--;
    }
}

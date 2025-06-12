using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class P2CharacterSelectManager : MonoBehaviour
{
    [Header("Character References")]
    public SpriteRenderer player2Sprite;
    public SpriteRenderer[] sprites;

    [Header("Player1 References")]
    private static int P2characterIndex = 0;
    private static int P2lastCharacterIndex = -1;

    private void Update()
    {
        if (P2characterIndex != P2lastCharacterIndex)
        {
            UpdateP2Character();
            P2lastCharacterIndex = P2characterIndex;
        }

        if (player2Sprite != null)
        {
            player2Sprite.sprite = sprites[P2characterIndex].sprite;
        }

        Debug.Log("P2 Index: " + P2characterIndex);
    }

    private void UpdateP2Character()
    {
        for (int i = 0; i < sprites.Length; i++)
        {
            sprites[i].gameObject.SetActive(i == P2characterIndex);
        }
    }

    public void P2NextCharacter()
    {
        if (P2characterIndex < sprites.Length - 1) P2characterIndex++;
    }

    public void P2PreviousCharacter() 
    {
        if (P2characterIndex > 0) P2characterIndex--;
    }
}

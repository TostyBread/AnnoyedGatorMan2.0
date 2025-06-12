using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class P1CharacterSelectManager : MonoBehaviour
{
    [Header("Character References")]
    public SpriteRenderer player1Sprite;
    public SpriteRenderer[] sprites;

    [Header("Player1 References")]
    private static int P1characterIndex = 0;
    private static int P1lastCharacterIndex = -1;

    private void Update()
    {
        if (P1characterIndex != P1lastCharacterIndex)
        {
            UpdateP1Character();
            P1lastCharacterIndex = P1characterIndex;
        }

        if (player1Sprite != null)
        {
            player1Sprite.sprite = sprites[P1characterIndex].sprite;
        }

        Debug.Log("P1 Index: " + P1characterIndex);
    }

    private void UpdateP1Character()
    {
        for (int i = 0; i < sprites.Length; i++)
        {
            sprites[i].gameObject.SetActive(i == P1characterIndex);
        }
    }

    public void P1NextCharacter()
    {
        if (P1characterIndex < sprites.Length - 1) P1characterIndex++;
    }

    public void P1PreviousCharacter() 
    {
        if (P1characterIndex > 0) P1characterIndex--;
    }
}

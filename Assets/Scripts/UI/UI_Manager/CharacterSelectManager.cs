using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectManager : MonoBehaviour
{
    public int characterIndex = 0;
    public Image[] characters;

    private void Update()
    {
        for (int i = 0; i < characters.Length; i++)
        {
            characters[i].gameObject.SetActive(i == characterIndex);
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

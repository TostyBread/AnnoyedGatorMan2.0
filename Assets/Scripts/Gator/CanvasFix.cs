using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasFix : MonoBehaviour
{
    private CharacterFlip characterFlip;

    private void Start()
    {
        characterFlip = GetComponentInParent<CharacterFlip>();
    }
    private void LateUpdate()
    {
        if (characterFlip != null)
        {
            Vector3 scale = transform.localScale;
            scale.x = characterFlip.isFacingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }
}

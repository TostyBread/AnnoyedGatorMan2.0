using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorManager : MonoBehaviour
{
    public bool MidDoor;
    public GameObject rightDoor;
    public GameObject leftDoor;
    public GameObject midDoor;

    public GameObject hitBoxL;
    public GameObject hitBoxR;

    public bool PlayerAtTheRightPos;
    public GameObject DoNotHurt;
    private bool canHitDoor;

    Rigidbody2D rb2d;
    private void OnEnable()
    {
        rb2d = GetComponent<Rigidbody2D>();
        
        StartCoroutine(Invaruable(0.2f));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Fist") && canHitDoor)
        {
            bool playerPunch = collision.gameObject.GetComponent<Fist>().isPunching;

            if (playerPunch)
            {
                //if player at the box 
                if (PlayerAtTheRightPos)
                {
                    DoNotHurt = collision.gameObject.transform.parent?.parent?.gameObject;

                    if (MidDoor)
                    {
                        bool isFacingRight = collision.gameObject.GetComponentInParent<CharacterFlip>().isFacingRight;

                        if (isFacingRight)
                        {
                            leftDoor.SetActive(true);
                            hitBoxR.SetActive(true);
                        }
                        else if (!isFacingRight)
                        {
                            rightDoor.SetActive(true);
                            hitBoxL.SetActive(true);
                        }
                    }
                    else
                    {
                        midDoor.SetActive(true);

                        if (gameObject.name.Contains("Right"))
                        {
                            hitBoxL.SetActive(true);
                        }
                        else if (gameObject.name.Contains("Left"))
                        {
                            hitBoxR.SetActive(true);
                        }
                    }
                }
                else 
                {
                    if (MidDoor)
                    {
                        bool isFacingRight = collision.gameObject.GetComponentInParent<CharacterFlip>().isFacingRight;

                        if (isFacingRight)
                        {
                            leftDoor.SetActive(true);
                        }
                        else if (!isFacingRight)
                        {
                            rightDoor.SetActive(true);
                        }
                    }
                    else
                    {
                        midDoor.SetActive(true);
                    }
                }
            }

            ////handle the DoNotHurt = null at DoNotHurt script
            //if (DoNotHurt != null)
            //    DoNotHurt = null

            gameObject.SetActive(false);
        }
    }

    private IEnumerator Invaruable(float time)
    { 
        canHitDoor = false;

        yield return new WaitForSeconds(time);

        canHitDoor = true;
    }
}

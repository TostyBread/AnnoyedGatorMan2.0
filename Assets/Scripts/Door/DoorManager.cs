using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorManager : MonoBehaviour
{
    public bool MidDoor;
    public List<GameObject> Door = new List<GameObject>();
    public List<GameObject> hitBox = new List<GameObject>();
    public bool PlayerAtTheRightPos;
    public GameObject DoNotHurt;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Fist") && PlayerAtTheRightPos)
        {
            bool playerPunch = collision.gameObject.GetComponent<Fist>().isPunching;

            if (playerPunch)
            {
                DoNotHurt = collision.gameObject.transform.parent.transform.parent?.gameObject;

                if (MidDoor)
                {
                    bool isFacingRight = collision.gameObject.GetComponentInParent<CharacterFlip>().isFacingRight;

                    if (isFacingRight)
                    {
                        Door[1].SetActive(true);
                        hitBox[0].SetActive(true);
                    }
                    else if (!isFacingRight)
                    {
                        Door[0].SetActive(true);
                        hitBox[1].SetActive(true);
                    }
                }
                else 
                {
                    Door[2].SetActive(true);

                    if (gameObject.name.Contains("Right"))
                    {
                        hitBox[1].SetActive(true);
                    }
                    else if (gameObject.name.Contains("Left"))
                    {
                        hitBox[0].SetActive(true);
                    }
                }
            }


            ////handle the DoNotHurt = null at DoNotHurt script
            //if (DoNotHurt != null)
            //    DoNotHurt = null

            gameObject.SetActive(false);
        }
    }
}

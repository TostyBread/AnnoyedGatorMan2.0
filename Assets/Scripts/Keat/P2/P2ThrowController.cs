using UnityEngine;
using UnityEngine.InputSystem;

public class P2ThrowController : MonoBehaviour
{
    public P2PickSystem p2PickSystem;
    private P2AimSystem p2AimSystem;

    public GameObject ThrowDirection;
    public GameObject P2Player;
    private CharacterFlip characterFlip;

    public Transform ThrowPos;
    private bool once;

    public GameObject Range;
    public GameObject Arrow;

    [Header("Input")]
    public KeyCode up;
    public KeyCode down;
    public KeyCode left;
    public KeyCode right;
    public KeyCode ControlMode;

    public bool controlMode;

    // Start is called before the first frame update
    void Start()
    {
        if (p2PickSystem == null)
        {
            Debug.Log("P2 is null, so P2 will get P2PickSystem from its parent");
            p2PickSystem = GetComponentInParent<P2PickSystem>();
        }

        p2AimSystem = GetComponentInParent<P2AimSystem>();

        characterFlip = P2Player.GetComponent<CharacterFlip>();


        gameObject.transform.parent = null;

        if (characterFlip.isFacingRight == true)
        {
            transform.rotation = Quaternion.Euler(0, 0, 180);
        }
        else if (characterFlip.isFacingRight == false)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = P2Player.transform.position;

        ShowAndHideThrowDirection();

        ThrowContoller();

        if (Input.GetKeyDown(ControlMode))
        {
            controlMode = !controlMode;
        }

        if (controlMode)
            CannotPickWhenHeldingObject();
        else if (!controlMode)
            AimAtRangeTarget();
    }

    private void CannotPickWhenHeldingObject()
    {
        if (p2PickSystem.heldItem == null)
        {
            Range.SetActive(true);
            once = false;
        }
        else if (p2PickSystem.heldItem != null)
        {
            Range.SetActive(false);

            //reset the Hand Pos once
            if (!once)
            {
                if (characterFlip.isFacingRight == true)
                {
                    transform.rotation = Quaternion.Euler(0, 0, 0);
                }
                else if (characterFlip.isFacingRight == false)
                {
                    transform.rotation = Quaternion.Euler(0, 0, 180);
                }
                once = true;
            }
        }
    }

    private void AimAtRangeTarget()
    {
        Range.SetActive(true);

        if (p2AimSystem.NearestTarget() != null)
        {
            Rotation(p2AimSystem.NearestTarget().transform);
            once = false;
        }
        else if (p2AimSystem.NearestTarget() == null)
        {
            if (!once)
            {
                //here is where you set the hand pos after no target
                ThrowContoller2();
                once = true;
            }
        }
    }

    private void ShowAndHideThrowDirection()
    {
        if (ThrowDirection != null)
        {
            if (p2PickSystem.heldItem != null && Arrow.activeSelf == false)
            {
                ThrowDirection.SetActive(true);
            }
            else
            {
                ThrowDirection.SetActive(false);
            }
        }
    }

    private void Rotation(Transform Target)
    {
        Vector2 direction = Target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }


    private void ThrowContoller()
    {
        if (Input.GetKey(up) && Input.GetKey(right))
        {
            transform.rotation = Quaternion.Euler(0, 0, 45);
        }
        else if (Input.GetKey(up) && Input.GetKey(left))
        {
            transform.rotation = Quaternion.Euler(0, 0, 135);
        }
        else if (Input.GetKey(down) && Input.GetKey(left))
        {
            transform.rotation = Quaternion.Euler(0, 0, -135);
        }
        else if (Input.GetKey(down) && Input.GetKey(right))
        {
            transform.rotation = Quaternion.Euler(0, 0, -45);
        }
        else if (Input.GetKeyDown(up))
        {
            transform.rotation = Quaternion.Euler(0, 0, 90);
        }
        else if (Input.GetKeyDown(down))
        {
            transform.rotation = Quaternion.Euler(0, 0, -90);
        }
        else if (Input.GetKeyDown(left))
        {
            transform.rotation = Quaternion.Euler(0, 0, 180);
        }
        else if (Input.GetKeyDown(right))
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    //just to reset the Hand Pos, if not the hand will fly randomly
    private void ThrowContoller2()
    {
        if (Input.GetKey(up) && Input.GetKey(right))
        {
            transform.rotation = Quaternion.Euler(0, 0, 45);
        }
        else if (Input.GetKey(up) && Input.GetKey(left))
        {
            transform.rotation = Quaternion.Euler(0, 0, 135);
        }
        else if (Input.GetKey(down) && Input.GetKey(left))
        {
            transform.rotation = Quaternion.Euler(0, 0, -135);
        }
        else if (Input.GetKey(down) && Input.GetKey(right))
        {
            transform.rotation = Quaternion.Euler(0, 0, -45);
        }
        else if (Input.GetKey(up))
        {
            transform.rotation = Quaternion.Euler(0, 0, 90);
        }
        else if (Input.GetKey(down))
        {
            transform.rotation = Quaternion.Euler(0, 0, -90);
        }
        else if (Input.GetKey(left))
        {
            transform.rotation = Quaternion.Euler(0, 0, 180);
        }
        else if (Input.GetKey(right))
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }
}

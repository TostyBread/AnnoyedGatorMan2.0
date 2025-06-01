using UnityEngine;
using UnityEngine.InputSystem;


public class P3Cursor : MonoBehaviour
{
    P3Controls controls;
    public Vector2 P3AimMove;
    public Transform DefaultThrowPos;
    public float speed;

    private void Awake()
    {
        controls = new P3Controls();

        controls.Gameplay.AimMove.performed += context => P3AimMove = context.ReadValue<Vector2>();
        controls.Gameplay.AimMove.canceled += context => P3AimMove = Vector2.zero;
    }


    private void OnEnable()
    {
        //if no console detected...
        if (Gamepad.current == null)
        {
            //gameObject.SetActive(false);
        }

        transform.parent = null;
        if (DefaultThrowPos != null)
        {
            transform.position = DefaultThrowPos.position;
        }

        controls.Gameplay.Enable();
    }

    private void OnDisable()
    {
        if (DefaultThrowPos != null && Gamepad.current != null)
        {
            transform.position = DefaultThrowPos.position;
            //transform.parent = DefaultThrowPos;
        }
        controls.Gameplay.Disable();
    }

    private void Update()
    {
        Vector2 am = new Vector2(P3AimMove.x, P3AimMove.y) * speed * Time.deltaTime;
        transform.Translate(am);
    }

}

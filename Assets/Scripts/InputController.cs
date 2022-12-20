using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[Serializable]
public class BoardClickEvent : UnityEvent<Vector3> { }
[Serializable]
public class IndicatorEvent : UnityEvent<GameObject> { }
public class InputController : MonoBehaviour
{
    public BoardClickEvent destClickEvent;
    public IndicatorEvent indicatorEvent;

    private PlayerInput input;
    private Vector2 mousePosition;
    private Camera cam;
    private BattleManager comb;
    private void Awake()
    {
        input = new PlayerInput();
        mousePosition = new Vector2(0, 0);
        cam = Camera.main;
        comb = GetComponent<BattleManager>();
    }

    private void OnEnable()
    {
        input.Combat.Enable();
        input.Combat.Position.performed += ctx => mousePosition = ctx.ReadValue<Vector2>();
        input.Combat.Rotation.performed += OnRotatePerformed;
        input.Combat.Click.performed += OnClickPerformed;
    }
    private void OnDisable()
    {
        input.Combat.Disable();
    }

    private void OnClickPerformed(InputAction.CallbackContext ctx)
    {
        Ray ray = cam.ScreenPointToRay(mousePosition);
        RaycastHit hitPoint;
        LayerMask mask = LayerMask.GetMask("Board");

        if (Physics.Raycast(ray, out hitPoint, mask))
        {
            if (hitPoint.transform.tag == "Indicator")
                indicatorEvent.Invoke(hitPoint.collider.gameObject);
            else
                destClickEvent.Invoke(hitPoint.point);
        }
    }

    private void OnRotatePerformed(InputAction.CallbackContext ctx)
    {
        Vector2 angleVec = ctx.ReadValue<Vector2>() / 10f;
        cam.transform.RotateAround(Vector3.zero, Vector3.up, angleVec.x);

        angleVec.y = -angleVec.y;
        if ((cam.transform.rotation.eulerAngles.x > 30f && angleVec.y < 0) ||
            (cam.transform.rotation.eulerAngles.x < 60f && angleVec.y > 0))
        {
            cam.transform.RotateAround(Vector3.zero, new Vector3(-cam.transform.position.z, 0, cam.transform.position.x), angleVec.y);
        }
        
    }

}

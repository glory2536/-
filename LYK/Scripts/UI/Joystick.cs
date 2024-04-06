//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private Image joystickBackground;
    private Image joystickController;
    private Vector2 touchPosition;
    [SerializeField]
    private Player player;

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        joystickBackground = GetComponent<Image>();
        joystickController = transform.GetChild(0).GetComponent<Image>();
        if (!player) player = FindObjectOfType<Player>();
    }

    /// <summary> 터치시 1회 호출 </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        player.ChangeState(PlayerStates.Move);
    }

    /// <summary> 터치 상태일 떄 매 프레임 호출 </summary>
    public void OnDrag(PointerEventData eventData)
    {
        touchPosition = Vector2.zero;

        if(RectTransformUtility.ScreenPointToLocalPointInRectangle(joystickBackground.rectTransform,eventData.position,eventData.pressEventCamera, out touchPosition))
        {
            touchPosition.x = (touchPosition.x / joystickBackground.rectTransform.sizeDelta.x);
            touchPosition.y = (touchPosition.y / joystickBackground.rectTransform.sizeDelta.y);

            touchPosition = new Vector2(touchPosition.x * 2 - 1, touchPosition.y * 2 - 1); //=> 피봇이 좌하단 일때
            //touchPosition = new Vector2(touchPosition.x * 2, touchPosition.y * 2 - 1); //=> 피봇이 중앙하단
            //touchPosition = new Vector2(touchPosition.x * 2 - 1, touchPosition.y * 2 + 1); //=> 피봇이 우측하단 일때

            touchPosition = (touchPosition.magnitude > 1) ? touchPosition.normalized : touchPosition;

            //가상 조이스틱 컨트롤러 이미지 이동
            joystickController.rectTransform.anchoredPosition = new Vector2(
                touchPosition.x * joystickBackground.rectTransform.sizeDelta.x *0.5f,
                touchPosition.y * joystickBackground.rectTransform.sizeDelta.y * 0.5f);

            //Debug.Log("Touch & Drag : " + eventData);
        }
    }

    /// <summary> 터치 종료 시 1회 호출 </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        joystickController.rectTransform.anchoredPosition = Vector2.zero;
        touchPosition = Vector2.zero;

        player.ChangeState(PlayerStates.Idle);
    }

    //외부에서 x위치값 참조
    public float Horizontal()
    {
        return touchPosition.x;
    }
    //외부에서 y위치값 참조
    public float Vertical()
    {
        return touchPosition.y;
    }
}

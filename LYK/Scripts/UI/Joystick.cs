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

    /// <summary> ��ġ�� 1ȸ ȣ�� </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        player.ChangeState(PlayerStates.Move);
    }

    /// <summary> ��ġ ������ �� �� ������ ȣ�� </summary>
    public void OnDrag(PointerEventData eventData)
    {
        touchPosition = Vector2.zero;

        if(RectTransformUtility.ScreenPointToLocalPointInRectangle(joystickBackground.rectTransform,eventData.position,eventData.pressEventCamera, out touchPosition))
        {
            touchPosition.x = (touchPosition.x / joystickBackground.rectTransform.sizeDelta.x);
            touchPosition.y = (touchPosition.y / joystickBackground.rectTransform.sizeDelta.y);

            touchPosition = new Vector2(touchPosition.x * 2 - 1, touchPosition.y * 2 - 1); //=> �Ǻ��� ���ϴ� �϶�
            //touchPosition = new Vector2(touchPosition.x * 2, touchPosition.y * 2 - 1); //=> �Ǻ��� �߾��ϴ�
            //touchPosition = new Vector2(touchPosition.x * 2 - 1, touchPosition.y * 2 + 1); //=> �Ǻ��� �����ϴ� �϶�

            touchPosition = (touchPosition.magnitude > 1) ? touchPosition.normalized : touchPosition;

            //���� ���̽�ƽ ��Ʈ�ѷ� �̹��� �̵�
            joystickController.rectTransform.anchoredPosition = new Vector2(
                touchPosition.x * joystickBackground.rectTransform.sizeDelta.x *0.5f,
                touchPosition.y * joystickBackground.rectTransform.sizeDelta.y * 0.5f);

            //Debug.Log("Touch & Drag : " + eventData);
        }
    }

    /// <summary> ��ġ ���� �� 1ȸ ȣ�� </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        joystickController.rectTransform.anchoredPosition = Vector2.zero;
        touchPosition = Vector2.zero;

        player.ChangeState(PlayerStates.Idle);
    }

    //�ܺο��� x��ġ�� ����
    public float Horizontal()
    {
        return touchPosition.x;
    }
    //�ܺο��� y��ġ�� ����
    public float Vertical()
    {
        return touchPosition.y;
    }
}

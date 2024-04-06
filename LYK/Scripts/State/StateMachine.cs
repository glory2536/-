using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//���� ���� �̺�Ʈ ó�����ִ� �޼ҵ�
public class StateMachine<T> where T : class
{
    private T ownerEntity;//StateMachine ������
    private State<T> currentState;//���� ����

    public void Setup(T owner, State<T> entryState)
    {
        ownerEntity = owner;
        currentState = null;

        ChangeState(entryState);
    }

    public void Execute()
    {
        if (currentState != null)
        {
            currentState.Execute(ownerEntity);
        }
    }

    public void ChangeState(State<T> newState)
    {
        Debug.Log(currentState + " -> " + newState);

        //���� �ٲٷ��� ���°� ��������� ���¸� �ٲ��� �ʴ´�
        if (newState == null) return;

        //���� ������� ���°� ������ Exit() �޼ҵ� ȣ��
        if (currentState != null)
        {
            currentState.Exit(ownerEntity);
        }

        //���ο� ���·� �����ϰ�, ���� �ٲ� ������ Enter() �޼ҵ� ȣ��
        currentState = newState;
        currentState.Enter(ownerEntity);
    }

    public State<T> CurrentStateRetrun()
    {
        return currentState;
    }

}

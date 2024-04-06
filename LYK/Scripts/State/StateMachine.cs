using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//스탯 관련 이벤트 처리해주는 메소드
public class StateMachine<T> where T : class
{
    private T ownerEntity;//StateMachine 소유주
    private State<T> currentState;//현재 상태

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

        //새로 바꾸려는 상태가 비어있으면 상태를 바꾸지 않는다
        if (newState == null) return;

        //현재 재생중인 상태가 있으면 Exit() 메소드 호출
        if (currentState != null)
        {
            currentState.Exit(ownerEntity);
        }

        //새로운 상태로 변경하고, 새로 바뀐 상태의 Enter() 메소드 호출
        currentState = newState;
        currentState.Enter(ownerEntity);
    }

    public State<T> CurrentStateRetrun()
    {
        return currentState;
    }

}

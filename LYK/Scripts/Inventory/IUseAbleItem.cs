using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 사용 가능한 아이템(착용/소모)
/// </summary>
interface IUseAbleItem
{
    //아이템 사용 : 성공 여부 리턴
    bool Use();
}

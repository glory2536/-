using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using KeyType = System.String;

/*
 PoolObjectData    => 오브젝트 풀 공통 데이터
 ObjectPoolManager => PoolObjectData를 기반으로 풀링 데이터 생성 및 관리 + 싱글톤
ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ
오브젝트 풀 오브젝트의 컴포넌트로 들어가는 기본 클래스(개별 데이터)
PoolObject - ObejctPoolDropItem(드랍아이템UI)
           - DamagePopUp(데미지 폰트)
           - EnemyHpBar(적체력바)
 */
namespace Glory.ObjectPool
{

    /// <summary> 폴링 대상 오브젝트에 대한 정보 </summary>
    [System.Serializable]
    public class PoolObjectData
    {
        public KeyType key;//키값
        public GameObject prefab;//복제될 오브젝트
        public Transform cloneParentPosition;//복제될 오브젝트 부모 위치

        public int InitialObjectCount = 7;//오브젝트 초기 생성 개수
        public int maxObjectCount = 20;//큐 내에 보관할 수 있는 오브젝트 최대 개수

    }

    public class ObjectPoolManager : Singleton<ObjectPoolManager>
    {
        [SerializeField]
        private List<PoolObjectData> _poolObjectDataList = new List<PoolObjectData>();

        //복제될 오브젝트의 원본 딕셔너리
        private Dictionary<KeyType, PoolObject> _sampleDict;

        //폴링 정보 딕셔너리
        private Dictionary<KeyType, PoolObjectData> _dataDict;

        //폴 디셔너리
        private Dictionary<KeyType, Stack<PoolObject>> _poolDict;


        private void Start()
        {
            Init();
        }

        /// <summary> 초기설정값 </summary>
        private void Init()
        {
            int len = _poolObjectDataList.Count;//저장된 오브젝트 풀링 정보 개수
            if (len == 0) return;

            //1. Dictionary 생성
            _sampleDict = new Dictionary<KeyType, PoolObject>(len);
            _dataDict = new Dictionary<KeyType, PoolObjectData>(len);
            _poolDict = new Dictionary<KeyType, Stack<PoolObject>>(len);

            //2. Data로부터 새로운 Pool 오브젝트 정보 생성
            foreach (var data in _poolObjectDataList)
            {
                Register(data);
            }
        }

        /// <summary> Pool 데이터로부터 새로운 Pool 오브젝트 정보 등록 </summary>
        private void Register(PoolObjectData data)
        {
            //중복 키는 등록 불가능
            //ContainsKey=>지정한 키가 포함되어 있는지 여부를 확인(불리언 반환)
            if (_poolDict.ContainsKey(data.key)) return;

            //1. 샘플 게임오브젝트 생성, PoolObject 컴포넌트 존재 확인
            GameObject sample = Instantiate(data.prefab);
            if (!sample.TryGetComponent(out PoolObject po))//sample==po
            {
                po = sample.AddComponent<PoolObject>();
                po.key = data.key;
            }

            sample.SetActive(false);

            //2. Pool Dictionary에 풀 생성 + 풀에 미리 오브젝트를 만들어 담아놓기
            Stack<PoolObject> pool = new Stack<PoolObject>(data.maxObjectCount);
            for (int i = 0; i < data.InitialObjectCount; i++)
            {
                PoolObject clone = po.Clone(data.cloneParentPosition);
                pool.Push(clone);
            }

            //3.딕셔너리에 추가
            _sampleDict.Add(data.key, po);
            _dataDict.Add(data.key, data);
            _poolDict.Add(data.key, pool);
        }

        /// <summary> 풀에서 꺼내오기 </summary>
        public PoolObject Spawn(KeyType key)
        {
            //키가 존재하지 않는 경우 null 리턴
            if (!_poolDict.TryGetValue(key, out var pool)) return null;

            PoolObject po;

            //1. 풀에 재고가 있는 경우 : 꺼내오기
            if (pool.Count > 0)
            {
                po = pool.Pop();
            }
            else//2. 재고가 없는 경우 샘플로부터 복제
            {
                Debug.Log("test3");
                po = _sampleDict[key].Clone(_dataDict[key].cloneParentPosition);
            }

            po.Activate();

            return po;
        }

        /// <summary> 폴에 집어넣기 </summary>
        public void Despawn(PoolObject po)
        {
            //키가 존재하지 않는 경우 종료
            if (!_poolDict.TryGetValue(po.key, out var pool)) return;

            KeyType key = po.key;

            //1. 폴에 넣을 수 있는 경우 : 폴에 넣기
            if (pool.Count < _dataDict[key].maxObjectCount)
            {
                pool.Push(po);
                po.Deactivate();
            }
            else//2.폴의 한도가 가득 찬 경우 : 파괴하기
            {
                Destroy(po.gameObject);
            }
        }

        /// <summary> 드랍아이템UI 오브젝트 풀</summary>
        public void GetDropItemPool(Transform _dropStartPosition, DropItemInfo _dropItemInfo, Vector3? _endPos = null)
        {
            string key = "DropItem";

            if (_dropItemInfo.itemAmount > 0)
            {
                for (int i = 0; i < _dropItemInfo.itemAmount; i++)
                {
                    PoolObject dropItemPool = Spawn(key);
                    if (dropItemPool != null && dropItemPool is ObejctPoolDropItem dropItem)
                    {
                        dropItem.DropItem(_dropStartPosition, _dropItemInfo, _endPos);
                    }
                }
            }
        }

        /// <summary> 적 HP바 오브젝트 풀</summary>
        public EnemyHpBar GetHPBarPool()
        {
            string key = "HPBar";
            PoolObject HPBarPool = Spawn(key);
            if (HPBarPool.TryGetComponent(out EnemyHpBar hpbar)) return hpbar;

            return null;
        }

        /// <summary> 데미지 폰트 오브젝트 풀</summary>
        public void DamagePopUpPool(Vector3 _startPosition, int _damage)
        {
            string key = "DamagePopUp";
            PoolObject damagePopUpPool = Spawn(key);
            if (damagePopUpPool.TryGetComponent(out DamagePopUp _damagePopUpPool))
            {
                _damagePopUpPool.DamagePopUpEvent(_startPosition, _damage);
            }
        }
    }
}

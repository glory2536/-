using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using KeyType = System.String;

/*
 PoolObjectData    => ������Ʈ Ǯ ���� ������
 ObjectPoolManager => PoolObjectData�� ������� Ǯ�� ������ ���� �� ���� + �̱���
�ѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤ�
������Ʈ Ǯ ������Ʈ�� ������Ʈ�� ���� �⺻ Ŭ����(���� ������)
PoolObject - ObejctPoolDropItem(���������UI)
           - DamagePopUp(������ ��Ʈ)
           - EnemyHpBar(��ü�¹�)
 */
namespace Glory.ObjectPool
{

    /// <summary> ���� ��� ������Ʈ�� ���� ���� </summary>
    [System.Serializable]
    public class PoolObjectData
    {
        public KeyType key;//Ű��
        public GameObject prefab;//������ ������Ʈ
        public Transform cloneParentPosition;//������ ������Ʈ �θ� ��ġ

        public int InitialObjectCount = 7;//������Ʈ �ʱ� ���� ����
        public int maxObjectCount = 20;//ť ���� ������ �� �ִ� ������Ʈ �ִ� ����

    }

    public class ObjectPoolManager : Singleton<ObjectPoolManager>
    {
        [SerializeField]
        private List<PoolObjectData> _poolObjectDataList = new List<PoolObjectData>();

        //������ ������Ʈ�� ���� ��ųʸ�
        private Dictionary<KeyType, PoolObject> _sampleDict;

        //���� ���� ��ųʸ�
        private Dictionary<KeyType, PoolObjectData> _dataDict;

        //�� ��ųʸ�
        private Dictionary<KeyType, Stack<PoolObject>> _poolDict;


        private void Start()
        {
            Init();
        }

        /// <summary> �ʱ⼳���� </summary>
        private void Init()
        {
            int len = _poolObjectDataList.Count;//����� ������Ʈ Ǯ�� ���� ����
            if (len == 0) return;

            //1. Dictionary ����
            _sampleDict = new Dictionary<KeyType, PoolObject>(len);
            _dataDict = new Dictionary<KeyType, PoolObjectData>(len);
            _poolDict = new Dictionary<KeyType, Stack<PoolObject>>(len);

            //2. Data�κ��� ���ο� Pool ������Ʈ ���� ����
            foreach (var data in _poolObjectDataList)
            {
                Register(data);
            }
        }

        /// <summary> Pool �����ͷκ��� ���ο� Pool ������Ʈ ���� ��� </summary>
        private void Register(PoolObjectData data)
        {
            //�ߺ� Ű�� ��� �Ұ���
            //ContainsKey=>������ Ű�� ���ԵǾ� �ִ��� ���θ� Ȯ��(�Ҹ��� ��ȯ)
            if (_poolDict.ContainsKey(data.key)) return;

            //1. ���� ���ӿ�����Ʈ ����, PoolObject ������Ʈ ���� Ȯ��
            GameObject sample = Instantiate(data.prefab);
            if (!sample.TryGetComponent(out PoolObject po))//sample==po
            {
                po = sample.AddComponent<PoolObject>();
                po.key = data.key;
            }

            sample.SetActive(false);

            //2. Pool Dictionary�� Ǯ ���� + Ǯ�� �̸� ������Ʈ�� ����� ��Ƴ���
            Stack<PoolObject> pool = new Stack<PoolObject>(data.maxObjectCount);
            for (int i = 0; i < data.InitialObjectCount; i++)
            {
                PoolObject clone = po.Clone(data.cloneParentPosition);
                pool.Push(clone);
            }

            //3.��ųʸ��� �߰�
            _sampleDict.Add(data.key, po);
            _dataDict.Add(data.key, data);
            _poolDict.Add(data.key, pool);
        }

        /// <summary> Ǯ���� �������� </summary>
        public PoolObject Spawn(KeyType key)
        {
            //Ű�� �������� �ʴ� ��� null ����
            if (!_poolDict.TryGetValue(key, out var pool)) return null;

            PoolObject po;

            //1. Ǯ�� ��� �ִ� ��� : ��������
            if (pool.Count > 0)
            {
                po = pool.Pop();
            }
            else//2. ��� ���� ��� ���÷κ��� ����
            {
                Debug.Log("test3");
                po = _sampleDict[key].Clone(_dataDict[key].cloneParentPosition);
            }

            po.Activate();

            return po;
        }

        /// <summary> ���� ����ֱ� </summary>
        public void Despawn(PoolObject po)
        {
            //Ű�� �������� �ʴ� ��� ����
            if (!_poolDict.TryGetValue(po.key, out var pool)) return;

            KeyType key = po.key;

            //1. ���� ���� �� �ִ� ��� : ���� �ֱ�
            if (pool.Count < _dataDict[key].maxObjectCount)
            {
                pool.Push(po);
                po.Deactivate();
            }
            else//2.���� �ѵ��� ���� �� ��� : �ı��ϱ�
            {
                Destroy(po.gameObject);
            }
        }

        /// <summary> ���������UI ������Ʈ Ǯ</summary>
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

        /// <summary> �� HP�� ������Ʈ Ǯ</summary>
        public EnemyHpBar GetHPBarPool()
        {
            string key = "HPBar";
            PoolObject HPBarPool = Spawn(key);
            if (HPBarPool.TryGetComponent(out EnemyHpBar hpbar)) return hpbar;

            return null;
        }

        /// <summary> ������ ��Ʈ ������Ʈ Ǯ</summary>
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

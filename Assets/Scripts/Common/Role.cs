using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using WarGame.UI;

namespace WarGame
{
    //��ɫ����
    public struct RoleAttribute
    {
        public float hp; //Ѫ��
        public float attack; //����
        public float defense; //����
        public float attackDis; //��������
        public float moveDis; //�����ƶ�����
    }

    public class Role
    {
        private int _id;

        private RoleAttribute _attribute;

        private List<string> _path;

        private int _pathIndex;

        private float _lerpStep = 0;

        protected GameObject _gameObject;

        protected Animator _animator;

        protected string _hpHUDKey;

        protected float _speed = 5.0f;

        public string hexagonID;

        protected List<string> _numberHUDList = new List<string>(); 

        public int ID
        {
            set { }
            get { return _id; }
        }

        public RoleAttribute Attribute
        {
            get { return _attribute; }
        }



        public Role(int id, RoleAttribute attribute, string assetPath, string hexagonID)
        {
            this._id = id;
            this._attribute = attribute;
            this.hexagonID = hexagonID;

            var bornPoint = MapTool.Instance.GetPosFromCoor(MapManager.Instance.GetHexagon(hexagonID).coordinate);
            OnCreate(assetPath, bornPoint);//���ط�ʽ��ͬ����ʽ�����涼Ҫ��
        }

        protected virtual void OnCreate(string assetPath, Vector3 bornPoint)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            _gameObject = GameObject.Instantiate(prefab);
            _gameObject.transform.position = bornPoint;
            _gameObject.transform.localScale = Vector3.one * 0.6F;
            _animator = _gameObject.GetComponent<Animator>();
            _gameObject.GetComponent<RoleData>().ID = this._id;

            _hpHUDKey = _id + "_HP";
            HUDManager.Instance.AddHUD("HUD", "HUDRole", _hpHUDKey, _gameObject.transform.Find("hudPoint").gameObject);
        }

        private void UpdateHexagonID(string id)
        {
            hexagonID = id;
        }


        public virtual void Update()
        {
            if (null != _gameObject && null != _path && _path.Count > 0)
            {
                _lerpStep += (Time.deltaTime * _speed);

                var startHexagon = MapManager.Instance.GetHexagon(_path[_pathIndex]);
                var endHexagon = MapManager.Instance.GetHexagon(_path[_pathIndex + 1]);
                var startPos = MapTool.Instance.GetPosFromCoor(startHexagon.coordinate);
                var endPos = MapTool.Instance.GetPosFromCoor(endHexagon.coordinate);

                _gameObject.transform.position = Vector3.Lerp(startPos, endPos, _lerpStep);

                if (_lerpStep >= 1)
                {
                    _lerpStep = 0;
                    _pathIndex++;
                    UpdateHexagonID(_path[_pathIndex]);
                    if (_pathIndex >= _path.Count - 1)
                    {
                        _path = null;
                        _pathIndex = 0;
                        EventDispatcher.Instance.Dispatch(Enum.EventType.Hero_MoveEnd_Event);
                        Idle();
                    }
                }
            }
        }

        public virtual void Move(List<string> hexagons)
        {
            var count = hexagons.Count;
            if (count <= 0 || hexagons[count - 1] == hexagonID)
                return;

            this._path = hexagons;
            _animator.Play("Move");
        }

        public virtual void Attack()
        {
            _animator.Play("Attack");
        }

        public virtual void Attacked(float hurt)
        {
            _animator.Play("Attacked");
            UpdateHP(_attribute.hp - hurt);
        }

        public virtual void Dead()
        {
            _animator.Play("Die");
        }

        public virtual void Idle()
        {
            _animator.Play("Idle");
        }

        public virtual void UpdateHP(float hp)
        {
            var hurt = _attribute.hp - hp;
            _attribute.hp = hp;
            HUDRole hud = (HUDRole)HUDManager.Instance.GetHUD(_hpHUDKey);
            hud.UpdateHP(hp);

            var numberID = ID + "_HUDNumber_" + _numberHUDList.Count;
            var numberHUD = (HUDNumber)HUDManager.Instance.AddHUD("HUD", "HUDNumber", numberID, _gameObject.transform.Find("hudPoint").gameObject);
            _numberHUDList.Add(numberID);
            numberHUD.Show(hurt, ()=> {
                HUDManager.Instance.RemoveHUD(numberID);
                _numberHUDList.Remove(numberID);
            });
        }

        public virtual void Dispose()
        {
            if (null != _hpHUDKey)
            {
                HUDManager.Instance.RemoveHUD(_hpHUDKey);
                _hpHUDKey = null;
            }
            for (int i = _numberHUDList.Count - 1; i >= 0; i--)
            {
                HUDManager.Instance.RemoveHUD(_numberHUDList[i]);
            }
            _numberHUDList.Clear();
        }
    }
}

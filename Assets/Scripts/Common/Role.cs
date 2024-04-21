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

        protected string _hudKey;

        protected float _speed = 5.0f;

        public string hexagonID;

        public int ID
        {
            set { }
            get { return _id; }
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

            _hudKey = _id + "_HP";
            HUDManager.Instance.AddHUD("HUD", "HUDRole", _hudKey, _gameObject.transform.Find("hudPoint").gameObject);
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
            this._path = hexagons;

            _animator.Play("Move");
        }

        public virtual void Attack()
        {
        
        }

        public virtual void Dead()
        {
        
        }

        public virtual void Idle()
        {
            _animator.Play("Idle");
        }

        public virtual void Dispose()
        {
            if (null != _hudKey)
            {
                HUDManager.Instance.RemoveHUD(_hudKey);
                _hudKey = null;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    [System.Serializable]
    public class EnemyWithSpawn
    {
        [SerializeField]
        private GameObject _enemy;
        [SerializeField]
        private PositionWithParent[] _positions;

        
        public EnemyWithSpawn(GameObject enemy, PositionWithParent[] positions)
        {
            this._enemy = enemy;
            this._positions = positions;
        }

        public GameObject Enemy
        {
            get
            {
                return _enemy;
            }
        }

        public PositionWithParent[] Positions
        {
            get
            {
                return _positions;
            }
        }
    }
    [System.Serializable]
    public class PositionWithParent
    {
        [SerializeField]
        private Vector3 _position;
        [SerializeField]
        private GameObject _parent = null;

        public GameObject Parent
        {
            get
            {
                return _parent;
            }
        }

        public Vector3 Position
        {
            get
            {
                return _position;
            }
        }
    }
}


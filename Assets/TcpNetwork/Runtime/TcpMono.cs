using UnityEngine;
using System;

namespace LeeFramework.Tcp
{
    public class TcpMono : MonoBehaviour
    {
        private static TcpMono _Instance;
        private static GameObject _GameObject;
        public static TcpMono instance
        {
            get
            {
                if (_Instance == null)
                {
                    _GameObject = new GameObject(typeof(TcpMono).ToString());
                    _GameObject.hideFlags = HideFlags.HideAndDontSave;
                    DontDestroyOnLoad(_GameObject);
                    _Instance = _GameObject.AddComponent<TcpMono>();
                }
                return _Instance;
            }
        }

        public Action callback = null;

        private void Awake()
        {
            if (_Instance == null)
            {
                gameObject.name = (typeof(TcpMono).ToString());
                DontDestroyOnLoad(this);
                if (gameObject.GetComponent<TcpMono>() == null)
                {
                    _Instance = gameObject.AddComponent<TcpMono>();
                }
                else
                {
                    _Instance = gameObject.GetComponent<TcpMono>();
                }
            }
            else
            {
                Destroy(this);
            }
        }


        private void Update()
        {
            callback?.Invoke();
        }

        public virtual void Dispose()
        {
            _Instance = default;
            GameObject.Destroy(_GameObject.gameObject);
            GC.Collect();
        }

    } 
}


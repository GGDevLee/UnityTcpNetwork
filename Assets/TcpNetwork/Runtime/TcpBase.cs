using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace LeeFramework.Tcp
{
    public abstract class TcpBase : IDisposable
    {
        public Socket client => _Client;
        public bool isConnecting => _IsConnecting;
        public bool isConnected => _IsConnected;
        public bool isDisconnected => _IsDisconnected;
        public bool isDisconnecting => _IsDisconnecting;
        public bool isSending => _IsSending;


        /// <summary>
        /// ?¼??ص?
        /// </summary>
        public Action<bool, SocketAsyncEventArgs> onConnect;
        public Action<bool, SocketAsyncEventArgs> onDisconnect;
        public Action<bool, byte[]> onReceive;
        public Action<bool, SocketAsyncEventArgs> onSend;



        protected Action<bool, SocketAsyncEventArgs> _OnConnect;
        protected Action<bool, SocketAsyncEventArgs> _OnDisconnect;
        protected Action<bool, byte[]> _OnReceive;
        protected Action<bool, SocketAsyncEventArgs> _OnSend;


        protected Socket _Client = null;
        protected bool _IsConnecting = false;
        protected bool _IsConnected = false;
        protected bool _IsDisconnected = false;
        protected bool _IsDisconnecting = false;
        protected bool _IsSending = false;
        protected IPEndPoint _IPEndPoint = null;
        protected byte[] _ReceBuff = new byte[8192];
        protected Queue<SendItem> _SendQue = new Queue<SendItem>();
        protected SocketAsyncEventArgs _ConnArgs = new SocketAsyncEventArgs();
        protected SocketAsyncEventArgs _DisconnArgs = new SocketAsyncEventArgs();
        protected SocketAsyncEventArgs _RecvArgs = new SocketAsyncEventArgs();
        protected SocketAsyncEventArgs _SendArgs = new SocketAsyncEventArgs();
        protected SendItemPool _SendPool = new SendItemPool(20);

        protected void InitCb()
        {
            _OnConnect = (state, args) =>
            {
                MainThread.instance.Run(() =>
                {
                    onConnect?.Invoke(state, args);
                });
            };
            _OnDisconnect = (state, args) =>
            {
                MainThread.instance.Run(() =>
                {
                    onDisconnect?.Invoke(state, args);
                });
            };
            _OnReceive = (state, data) =>
            {
                MainThread.instance.Run(() =>
                {
                    onReceive?.Invoke(state, data);
                });
            };
            _OnSend = (state, args) =>
            {
                MainThread.instance.Run(() =>
                {
                    onSend?.Invoke(state, args);
                });
            };
        }

        /// <summary>
        /// ???ý??ջ???
        /// </summary>
        protected void ResetReceBuff()
        {
            _ReceBuff = new byte[8192];
            _RecvArgs = new SocketAsyncEventArgs();
            _RecvArgs.Completed += OnComplete;
            _RecvArgs.UserToken = _Client;
            _RecvArgs.SetBuffer(_ReceBuff, 0, _ReceBuff.Length);
        }

        protected virtual void OnComplete(object sender, SocketAsyncEventArgs args)
        { 
            
        }

        public abstract void ConnectAsync();

        public abstract void SendAsync(byte[] buffer);

        public abstract void Send(byte[] buffer);

        public abstract void DisconnectAsync();

        public void CloseSocket()
        {
            if (_Client != null)
            {
                return;
            }
            try
            {
                _Client.Shutdown(SocketShutdown.Both);
                _Client.Close();
            }
            catch (Exception e)
            {
                Debug.LogError("?ر?Socketʧ?ܣ?" + e.ToString());
            }
        }

        public void Dispose()
        {
            _Client = null;

            onConnect = null;
            onDisconnect = null;
            onReceive = null;
            onSend = null;

            _OnConnect = null;
            _OnDisconnect = null;
            _OnReceive = null;
            _OnSend = null;

            _ConnArgs.Dispose();
            _DisconnArgs.Dispose();
            _RecvArgs.Dispose();
            _SendArgs.Dispose();

            _ConnArgs = null;
            _DisconnArgs = null;
            _RecvArgs = null;
            _SendArgs = null;
        }
    } 
}

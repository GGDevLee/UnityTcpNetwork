using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;


namespace LeeFramework.Tcp
{
    public class TcpNetwork : TcpBase
    {
        public TcpNetwork(string ip, int port)
        {
            _IPEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            InitCb();
            InitComplete();
        }

        public TcpNetwork(IPEndPoint ip)
        {
            _IPEndPoint = ip;
            InitCb();
            InitComplete();
        }

        private void InitComplete()
        {
            _ConnArgs.Completed += OnComplete;
            _DisconnArgs.Completed += OnComplete;
            _SendArgs.Completed += OnComplete;
        }



        protected override void OnComplete(object sender, SocketAsyncEventArgs args)
        {
            switch (args.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    OnConnect(args);
                    break;
                case SocketAsyncOperation.Disconnect:
                    OnDisconnect(args);
                    break;
                case SocketAsyncOperation.Receive:
                    OnReceive(args);
                    break;
                case SocketAsyncOperation.Send:
                    OnSend(args);
                    break;
            }
        }



        private void OnConnect(SocketAsyncEventArgs args)
        {
            if (_Client == null)
            {
                return;
            }

            _IsConnected = false;
            _IsConnecting = false;
            _IsDisconnected = false;
            _IsDisconnecting = false;

            //连接失败
            if (args.SocketError != SocketError.Success)
            {
                CloseSocket();
                _OnConnect?.Invoke(false, args);
                return;
            }

            //连接成功
            _IsConnected = true;
            _OnConnect?.Invoke(true, args);

            ResetReceBuff();

            _SendArgs.UserToken = _Client;
            _SendArgs.RemoteEndPoint = _IPEndPoint;

            if (!_Client.ReceiveAsync(_RecvArgs))
            {
                OnReceive(args);
            }
        }

        private void OnDisconnect(SocketAsyncEventArgs args)
        {
            if (_Client == null)
            {
                return;
            }
            _IsDisconnected = true;

            _IsDisconnecting = false;
            _IsConnected = false;
            _IsConnecting = false;
            _IsSending = false;

            //断开连接失败
            if (args.SocketError != SocketError.Success)
            {
                CloseSocket();
                _OnDisconnect?.Invoke(false, args);
                return;
            }

            //断开连接成功
            CloseSocket();
            _OnDisconnect?.Invoke(true, args);
        }

        private void OnReceive(SocketAsyncEventArgs args)
        {
            if (_Client == null)
            {
                return;
            }
            if (!_IsConnected || _IsConnecting || _IsDisconnecting || _IsDisconnected)
            {
                return;
            }

            //接收失败
            if (args.SocketError != SocketError.Success || args.BytesTransferred == 0)
            {
                CloseSocket();
                _OnReceive?.Invoke(false, args.Buffer);
                return;
            }

            //接收完成
            Socket client = args.UserToken as Socket;
            if (client != null && client.Available == 0)
            {
                _OnReceive?.Invoke(true, args.Buffer);
            }

            if (!_Client.ReceiveAsync(_RecvArgs))
            {
                OnReceive(args);
            }
        }

        private void OnSend(SocketAsyncEventArgs args)
        {
            if (_Client == null)
            {
                return;
            }

            _IsSending = false;

            //发送失败
            if (args.SocketError != SocketError.Success)
            {
                CloseSocket();
                _OnSend?.Invoke(false, args);
                _SendQue.Clear();
                return;
            }

            //发送成功
            _OnSend?.Invoke(true, args);
            CheckSend();
        }


        /// <summary>
        /// 检查是否还有未发送的消息
        /// </summary>
        private void CheckSend()
        {
            if (_SendQue.Count > 0)
            {
                SendItem item = _SendQue.Dequeue();
                if (item != null)
                {
                    SendAsync(item.buff);
                    _SendPool.Recycle(item);
                }
            }
        }


        /// <summary>
        /// 异步连接
        /// </summary>
        public override void ConnectAsync()
        {
            if (_IsConnected || _IsConnecting || _IsDisconnecting)
            {
                return;
            }

            _Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _Client.NoDelay = true;
            _ConnArgs.UserToken = _Client;
            _ConnArgs.RemoteEndPoint = _IPEndPoint;
            _ConnArgs.Completed -= OnComplete;
            _ConnArgs.Completed += OnComplete;

            _IsConnecting = true;
            _IsConnected = false;

            //开始异步连接
            if (!_Client.ConnectAsync(_ConnArgs))
            {
                OnConnect(_ConnArgs);
            }
        }

        /// <summary>
        /// 异步发送消息
        /// </summary>
        public override void SendAsync(byte[] buffer)
        {
            if (_Client == null)
            {
                return;
            }

            if (!_IsConnected)
            {
                return;
            }

            //正在发送，添加到队列里
            if (_IsSending)
            {
                SendItem item = _SendPool.Spawn();
                item.buff = buffer;
                _SendQue.Enqueue(item);
                return;
            }
            
            try
            {
                _IsSending = true;
                _SendArgs.SetBuffer(buffer, 0, buffer.Length);
                if (!_Client.SendAsync(_SendArgs))
                {
                    OnSend(_SendArgs);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("发送失败：" + e.ToString());
                _IsSending = false;
            }
        }

        /// <summary>
        /// 不用经过消息队列，直接发送消息
        /// </summary>
        public override void Send(byte[] buffer)
        {
            if (_Client == null)
            {
                return;
            }

            if (!_IsConnected)
            {
                return;
            }
            try
            {
                SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();
                sendArgs.SetBuffer(buffer, 0, buffer.Length);
                sendArgs.Completed += OnComplete;
                if (!_Client.SendAsync(sendArgs))
                {
                    OnSend(sendArgs);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("发送失败：" + e.ToString());
            }
        }

        /// <summary>
        /// 异步断开连接
        /// </summary>
        public override void DisconnectAsync()
        {
            if (_Client == null)
            {
                return;
            }

            //正在连接，也不能断开连接
            if (_IsDisconnected || _IsConnecting)
            {
                return;
            }

            _IsDisconnected = false;
            _IsDisconnecting = true;

            if (!_Client.DisconnectAsync(_DisconnArgs))
            {
                OnDisconnect(_DisconnArgs);
            }
        }

    }
}

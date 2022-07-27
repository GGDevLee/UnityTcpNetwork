using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LeeFramework.Tcp;
using System.Net;
using System.Text;

public class Test : MonoBehaviour
{
    private TcpNetwork _TcpNetwork;
    private MainThread _Thread;

    private void Start()
    {
        _Thread = MainThread.instance;
        IPEndPoint point = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1000);

        _TcpNetwork = new TcpNetwork(point);

        TcpNetwork.onConnect = (state, value) =>
        {
            if (state)
            {
                Debug.Log("Connect : " + state.ToString());
            }
            else
            {
                Debug.LogError("Connect : " + state.ToString());
            }
        };

        TcpNetwork.onDisconnect = (state, value) =>
        {
            if (state)
            {
                Debug.Log("Disconnect : " + state.ToString());
            }
            else
            {
                Debug.LogError("Disconnect : " + state.ToString());
            }
        };

        TcpNetwork.onReceive = (state, buff) =>
        {

            if (state)
            {
                if (!isSend)
                {
                    return;
                }
                Debug.Log("Receive : " + state + "   " + Encoding.UTF8.GetString(buff));
                index++;
                Debug.Log("发送 ： " + index.ToString());
                _TcpNetwork.SendAsync(Encoding.UTF8.GetBytes("Hello : " + index.ToString()));
            }
            else
            {
                Debug.LogError("Receive : " + state + "   " + Encoding.UTF8.GetString(buff));
            }
        };

        TcpNetwork.onSend = (state, value) =>
          {
              if (state)
              {
                  Debug.Log("发送成功");
              }
              else
              {
                  Debug.LogError("发送失败");
              }
          };

        _TcpNetwork.ConnectAsync();

    }
    int index = 0;
    float timer = 0;
    float time = 1;
    bool isSend = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isSend = true;
            index = 0;
            timer = 0;
        }

        if (isSend)
        {
            if (_TcpNetwork.isConnected)
            {
                index++;
                Debug.Log("发送 ： " + index.ToString());
                _TcpNetwork.SendAsync(Encoding.UTF8.GetBytes("Hello : " + index.ToString()));
            }
            timer += Time.deltaTime;
            if (timer >= time)
            {
                isSend = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            _TcpNetwork.DisconnectAsync();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            _TcpNetwork.ConnectAsync();
        }
    }

    private void OnDestroy()
    {
        _TcpNetwork.Dispose();
    }
}

# TcpNetwork

**联系作者：419731519（QQ）**

### =================TcpNetwork介绍=================
#### 笔者一直寻求一种异步高性能的网络通讯方式，而在.Net中，微软提供了SocketAsyncEventArgs来实现IOCP
#### 关于IOCP的优点，笔者不过多阐述，我这里对SocketAsyncEventArgs，Socket进行完整的封装，直接监听即可连接，发送，接收数据等
#### 最终的性能，会比常规的BeginReceive，BeginSend高2倍以上！
#### 具体的网络业务，需要开发者自行处理

### =================关于异步接收消息=================
#### TcpNetwork.onConnect连接状态成功后，会立马开始接收消息
#### 所以开发者，只需要监听TcpNetwork.onReceive即可接收消息
#### 一般情况下，网络消息是在多线程下接收的，而多线程的消息，不能直接给Unity使用
#### 笔者使用了MainThread，使得多线程的消息，安全转移到Unity主线程中
#### 所以开发者可以安全的使用TcpNetwork.onReceive返回的消息，不需要再次转移到Unity主线程中

### =================关于异步发送消息=================
#### 笔者使用了消息队列的方式来处理异步发送的消息，即上一条消息未发送完成，就不会发送下一条消息
#### 以确保服务器所收到的消息，是有序

### =================使用方法=================
- manifest.json中添加插件路径 或 直接引用Release内的dll（二选一）
```json
{
  "dependencies": {
	"com.leeframework.tcpnetwork":"https://e.coding.net/ggdevlee/leeframework/TcpNetwork.git#1.0.1"
  }
}
```

- 引入命名空间
```csharp
using LeeFramework.Tcp;
```

- TcpNetwork初始化
```csharp
private TcpNetwork _TcpNetwork;

private void Start()
{    
	//ip与端口号
    _TcpNetwork = new TcpNetwork("127.0.0.1",1000);
    _TcpNetwork.onConnect = (state, args) =>
    {
        Debug.Log("Connect : " + state.ToString());
    };
    
    _TcpNetwork.onDisconnect = (state, args) =>
    {
        Debug.Log("Disconnect : " + state.ToString());
    };

    _TcpNetwork.onReceive = (state, buff) =>
    {

        if (state)
        {
            Debug.Log("Receive : " + state + "   " + Encoding.UTF8.GetString(buff));
        }
        else
        {
            Debug.LogError("Receive : " + state + "   " + Encoding.UTF8.GetString(buff));
        }
    };

    _TcpNetwork.onSend = (state, args) =>
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
}        
```

- 异步创建网络连接
- 异步连接成功与失败，都会触发到TcpNetwork.onConnect事件
- 异步连接成功后，会自动接收消息，接收到的消息，都会触发TcpNetwork.onReceive事件
```csharp

public void ConnectAsync()
{
    _TcpNetwork.ConnectAsync();
}

```

- 异步发送消息
- 异步发送成功与失败，都会触发到TcpNetwork.onSend事件
```csharp

public void SendAsync()
{
    _TcpNetwork.SendAsync(Encoding.UTF8.GetBytes("HelloWorld!"));
}

```

- 异步断开连接
- 异步断开连接成功与失败，都会触发到TcpNetwork.onDisconnect事件

```csharp

public void DisconnectAsync()
{
    _TcpNetwork.DisconnectAsync();
}

```

- TcpNetwork网络状态
```csharp

_TcpNetwork.client //Socket套接字

_TcpNetwork.isConnecting //正在网络连接

_TcpNetwork.isConnected //是否已经网络连接

_TcpNetwork.isDisconnecting //正在断开连接

_TcpNetwork.isDisconnected //是否已经断开连接

_TcpNetwork.isSending //正在发送消息

```
        
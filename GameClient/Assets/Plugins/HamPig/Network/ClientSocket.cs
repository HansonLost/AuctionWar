using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamPig.Network
{
    public static partial class SocketExtension
    {
        public static IAsyncResult BeginReceive(this Socket socket, ByteArray bytes, AsyncCallback callback, object state)
        {
            Int32 offset = bytes.offset + bytes.size;
            Int32 size = bytes.GetFreeLength();
            return socket.BeginReceive(bytes.buffer, offset, size, 0, callback, state);
        }

        public static IAsyncResult BeginSend(this Socket socket, ByteArray bytes, AsyncCallback callback, object state)
        {
            return socket.BeginSend(bytes.buffer, bytes.offset, bytes.size, 0, callback, state);
        }
    }

    public sealed class ClientSocket
    {
        public Listener<bool> onConnect { get; private set; }
        private ConnectEvent m_ConnectEvent;

        public Listener<byte[]> onReceive { get; private set; }
        private SocketReadBuffer m_ReadBuffer;
        private SocketWriteBuffer m_WriteBuffer;

        public Listener onForceClose { get; private set; }
        private ForceCloseEvent m_ForceCloseEvent;

        private Socket m_Socket;
        private bool m_IsClosing;

        public ClientSocket()
        {
            m_ReadBuffer = new SocketReadBuffer();
            m_WriteBuffer = new SocketWriteBuffer();
            onConnect = new Listener<bool>();
            onReceive = new Listener<byte[]>();
            onForceClose = new Listener();
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                // 2个 bufferSize 都用默认的
                NoDelay = true, // 为了游戏的实时性，关闭延时发送。
                // TTL 先不调
            };
            m_IsClosing = false;
        }

        public void Connect(string ip, int port)
        {
            m_Socket.BeginConnect(ip, port, ConnectCallback, m_Socket);
        }

        public void Tick()
        {
            // 连接事件
            if(m_ConnectEvent != null)
            {
                var isSucceed = m_ConnectEvent.isSucceed;
                onConnect.Invoke(isSucceed);
                m_ConnectEvent = null;
            }

            // 接收事件
            ByteArray data = m_ReadBuffer.GetData();
            while(data != null)
            {
                onReceive.Invoke(data.ToBytes());
                data = m_ReadBuffer.GetData();
            }

            // 远程强制关闭事件
            if(m_ForceCloseEvent != null)
            {
                onForceClose.Invoke();
                m_ForceCloseEvent = null;
            }
        }

        public void Send(byte[] data)
        {
            if (!m_Socket.Connected) return;

            ByteArray sendBytes = m_WriteBuffer.Add(data);
            if(sendBytes != null)
            {
                m_Socket.BeginSend(sendBytes, SendCallback, m_Socket);
            }
        }

        public void Close()
        {
            m_IsClosing = true;
            if (m_WriteBuffer.IsEmpty())
            {
                m_Socket.BeginDisconnect(false, DisconnectCallback, m_Socket);
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket socket = (Socket)ar.AsyncState;
                socket.EndConnect(ar);
                socket.BeginReceive(m_ReadBuffer.recvBuffer, ReceiveCallback, socket);

                m_ConnectEvent = new ConnectEvent
                {
                    isSucceed = true,
                    ex = null,
                };
            }
            catch (SocketException ex)
            {
                m_ConnectEvent = new ConnectEvent
                {
                    isSucceed = false,
                    ex = ex.ToString(),
                };
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                Socket socket = (Socket)ar.AsyncState;
                if (socket != null && !socket.Connected) return; // close 之后会中止 recv 线程，有可能会调用该 callback。
                int count = socket.EndReceive(ar);
                m_ReadBuffer.Update(count);
                socket.BeginReceive(m_ReadBuffer.recvBuffer, ReceiveCallback, socket);
            }
            catch (SocketException)
            {
                m_ForceCloseEvent = new ForceCloseEvent();
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket socket = (Socket)ar.AsyncState;
                int count = socket.EndSend(ar); // 只是把数据成功放到 send buffer。
                var sendBytes = m_WriteBuffer.Update(count);
                if (sendBytes != null)
                {
                    socket.BeginSend(sendBytes, SendCallback, socket);
                }
                else if (m_IsClosing)
                {
                    socket.BeginDisconnect(false, DisconnectCallback, socket);
                }
            }
            catch (SocketException ex)
            {
                UnityEngine.Debug.Log(ex.ToString());
            }
        }

        private void DisconnectCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.Close();
            m_IsClosing = false;
        }

        public class ConnectEvent
        {
            public bool isSucceed;
            public string ex;
        }

        public class ForceCloseEvent { }
    }
}

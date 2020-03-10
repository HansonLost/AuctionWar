using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace HamPig.Network
{
    public class ServerSocket
    {
        //private Dictionary<Socket, ClientState> m_OnlineClients = new Dictionary<Socket, ClientState>();
        private Socket m_Listenfd;

        public Listener<Socket, byte[]> onReceive { get; private set; }
        private List<Data> m_DataList = new List<Data>();
        private int m_DataCount = 0;

        public Listener<Socket> onAccept { get; private set; }
        private List<AcceptEvent> m_AcceptEventList = new List<AcceptEvent>();
        private int m_AcceptEventCount = 0;

        public Action<Socket> onClose { get; set; }
        private SocketEvent<CloseEvent> m_CloseEvents = new SocketEvent<CloseEvent>();

        public ServerSocket()
        {
            onReceive = new Listener<Socket, byte[]>();
            onAccept = new Listener<Socket>();
            m_CloseEvents.AddListener(delegate (CloseEvent msg)
            {
                if (onClose != null)
                {
                    onClose.Invoke(msg.cfd);
                }
                msg.cfd.Close();
            });
            m_Listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //m_Listenfd.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true); 先不弄端口复用
        }

        public void Bind(String ip, Int32 port)
        {
            IPAddress addr = IPAddress.Parse(ip);
            IPEndPoint ipEp = new IPEndPoint(addr, port);
            m_Listenfd.Bind(ipEp);
            m_Listenfd.Listen(0);   // 不限制待连接数
            m_Listenfd.BeginAccept(AcceptCallback, m_Listenfd);
        }
        public void Tick()
        {
            // 处理 accept 事件
            while(m_AcceptEventCount > 0)
            {
                AcceptEvent e = null;
                lock (m_AcceptEventList)
                {
                    e = m_AcceptEventList[0];
                    m_AcceptEventList.RemoveAt(0);
                    m_AcceptEventCount--;
                }
                onAccept.Invoke(e.cfd);
            }

            // 处理 receive 事件
            while (m_DataCount > 0)
            {
                Data msg = null;
                lock (m_DataList)
                {
                    msg = m_DataList[0];
                    m_DataList.RemoveAt(0);
                    m_DataCount--;
                }
                onReceive.Invoke(msg.clientfd, msg.byteData);
            }

            m_CloseEvents.InvokeEvent();
        }
        public void Send(Socket cfd, byte[] data)
        {
            if (!cfd.Connected) return;


            Int16 len = (Int16)data.Length;
            byte[] lenBytes = LittleEndianByte.GetBytes(len);
            byte[] sendBytes = lenBytes.Concat(data).ToArray();
            cfd.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, cfd);
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                Console.WriteLine("accept client.");
                Socket listenfd = (Socket)ar.AsyncState;
                Socket clientfd = listenfd.EndAccept(ar);

                ClientState state = new ClientState
                {
                    socket = clientfd,
                    readBuffer = new SocketReadBuffer(),
                };
                //m_OnlineClients.Add(clientfd, state);
                clientfd.BeginReceive(state.readBuffer.recvBuffer, ReceiveCallback, state);
                listenfd.BeginAccept(AcceptCallback, listenfd);

                lock (m_AcceptEventList)
                {
                    m_AcceptEventList.Add(new AcceptEvent { cfd = clientfd });
                    m_AcceptEventCount++;
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                ClientState state = (ClientState)ar.AsyncState;
                Socket cfd = state.socket;
                int count = cfd.EndReceive(ar);
                if (count == 0)
                {
                    m_CloseEvents.AddEvent(new CloseEvent { cfd = cfd });
                }
                else
                {
                    state.readBuffer.Update(count);

                    ByteArray data = state.readBuffer.GetData();
                    while (data != null)
                    {
                        lock (m_DataList)
                        {
                            m_DataList.Add(new Data
                            {
                                clientfd = cfd,
                                byteData = data.ToBytes(),
                            });
                            m_DataCount++;
                        }
                        data = state.readBuffer.GetData();
                    }

                    cfd.BeginReceive(state.readBuffer.recvBuffer, ReceiveCallback, state);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket socket = (Socket)ar.AsyncState;
                int count = socket.EndSend(ar); // 只是把数据成功放到 send buffer。
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public class SocketEvent<T>
            where T : class
        {
            private Queue<T> m_Events = new Queue<T>();
            private Int32 m_EventCount = 0;
            private Action<T> m_Action;

            public void AddEvent(T msg)
            {
                lock (m_Events)
                {
                    m_Events.Enqueue(msg);
                    ++m_EventCount;
                }
            }
            public void AddListener(Action<T> action)
            {
                m_Action += action;
            }
            public void RemoveListener(Action<T> action)
            {
                if (m_Action == null) return;
                m_Action -= action;
            }
            public void InvokeEvent()
            {
                while(m_EventCount > 0)
                {
                    T msg = null;
                    lock (m_Events)
                    {
                        msg = m_Events.Dequeue();
                        --m_EventCount;
                    }
                    if(m_Action != null)
                    {
                        m_Action.Invoke(msg);
                    }
                }
            }
        }

        private class ClientState
        {
            public Socket socket;
            public SocketReadBuffer readBuffer;
        }
        private class Data
        {
            public Socket clientfd;
            public byte[] byteData;
        }

        public class AcceptEvent
        {
            public Socket cfd;
        }
        public class CloseEvent
        {
            public Socket cfd;
        }
    }
}

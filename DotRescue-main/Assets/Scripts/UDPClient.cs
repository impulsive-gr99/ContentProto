using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using static System.Console;

using UnityEngine.Events;

public class UDPClient : MonoBehaviour
{
    public int      port    = 7777;
    public UnityEvent udpEvent;
    private Queue<Action> actionQueue = new Queue<Action>();

    UdpClient SendPort, ReceivePort;
    private void Start()
    {
        ReceivePort = new UdpClient(port);
        ReceivePort.BeginReceive(OnReceive, null);
    }
    private void Update()
    {
        while (actionQueue.Count > 0)
            actionQueue.Dequeue().Invoke();
    }

    public void Send(string msg, string send_ip)
    {
        SendPort = new UdpClient();
        byte[] datagram = Encoding.UTF8.GetBytes(msg);
        SendPort.Send(datagram, datagram.Length, send_ip, port);
        Debug.Log(string.Format("[Send] {0}:{1} 로 {2} 바이트 전송", send_ip, port, datagram.Length));
    }
    public void Send(string msg) { Send(msg, "127.0.0.1"); }

    public void OnReceive(IAsyncResult ar)
    {
        try{
            IPEndPoint iPEndPoint = null;
            byte[] data = ReceivePort.EndReceive(ar, ref iPEndPoint);
            string sdata = Encoding.UTF8.GetString(data);
            //Debug.Log(string.Format("[Receive] {0} 로부터 {1} 바이트 수신:{2}", iPEndPoint.ToString(), data.Length, sdata));

            int number;
            if (Int32.TryParse(sdata, out number))
            {
                switch (number){
                    case 0: Debug.Log("유지"); break;
                    case 1: Debug.Log("방향 전환"); actionQueue.Enqueue(() => { udpEvent.Invoke(); }); break;
                }
            }
        }
        catch (Exception e) { Debug.Log(e); }
        ReceivePort.BeginReceive(OnReceive, null);
    }

    private void OnDestroy() {
        if (ReceivePort != null) ReceivePort.Close();
        if (SendPort != null) SendPort.Close();
        ReceivePort = null; SendPort = null;
    }
}

﻿using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Events;

namespace MiniFramework
{
    public class TcpClientComponent : MonoSingleton<TcpClientComponent>
    {
        public string Address;
        public int Port;
        public int Timeout;
        public bool IsConnected;

        public UnityEvent ConnectSuccess;
        public UnityEvent ConnectTimeout;
        public UnityEvent ConnectAbort;

        private int maxBufferSize = 1024;
        private byte[] recvBuffer;
        private TcpClient tcpClient;
        private DataPacker dataPacker;

        void Start()
        {
            Connect();
        }
        [ContextMenu("连接服务器")]
        public void Connect()
        {
            if (IsConnected)
            {
                return;
            }
            recvBuffer = new byte[maxBufferSize];
            dataPacker = new DataPacker();
            tcpClient = new TcpClient();
            IPAddress ip = SocketUtil.ParseIP(Address);
            tcpClient.BeginConnect(ip, Port, ConnectResult, tcpClient);
            Debug.Log("开始连接服务器：" + ip + " 端口：" + Port);
            CheckConnectState();
        }
        private void CheckConnectState()
        {
            SequenceNode waitForSuccess = null;
            SequenceNode waitForFail = null;
            //连接成功
            waitForSuccess = this.Sequence().Until(() => IsConnected).Event(() =>
            {
                ConnectSuccess.Invoke();
                waitForFail.Stop();
            }).Begin();
            //连接失败
            waitForFail = this.Sequence().Delay(Timeout).Event(() =>
            {
                if (!IsConnected)
                {
                    Debug.LogError("连接超时!");
                    tcpClient.Close();
                    ConnectTimeout.Invoke();
                    waitForSuccess.Stop();
                }
            }).Begin();

        }
        private void ConnectResult(IAsyncResult ar)
        {
            tcpClient = (TcpClient)ar.AsyncState;
            tcpClient.EndConnect(ar);
            if (tcpClient.Connected)
            {
                NetworkStream stream = tcpClient.GetStream();
                stream.BeginRead(recvBuffer, 0, recvBuffer.Length, ReadResult, tcpClient);
                IsConnected = true;
                Debug.Log("客户端连接成功!");
            }
        }
        private void ReadResult(IAsyncResult ar)
        {
            tcpClient = (TcpClient)ar.AsyncState;
            NetworkStream stream = tcpClient.GetStream();
            int recvLength = stream.EndRead(ar);
            if (recvLength > 0)
            {
                byte[] recvBytes = new byte[recvLength];
                Array.Copy(recvBuffer, 0, recvBytes, 0, recvLength);
                dataPacker.UnPack(recvBytes);
                stream.BeginRead(recvBuffer, 0, recvBuffer.Length, ReadResult, tcpClient);
            }
            else
            {
                Debug.LogError("网络中断!");
                Close();
            }
        }
        public void Send(int msgID, byte[] bodyData)
        {
            if (IsConnected)
            {
                byte[] sendData = dataPacker.Packer(msgID, bodyData);
                tcpClient.GetStream().BeginWrite(sendData, 0, sendData.Length, SendResult, tcpClient);
                Debug.Log("发送消息ID：" + msgID + " 大小：" + sendData.Length + "字节");
            }
        }
        private void SendResult(IAsyncResult ar)
        {
            tcpClient = (TcpClient)ar.AsyncState;
            NetworkStream stream = tcpClient.GetStream();
            stream.EndWrite(ar);
        }
        [ContextMenu("断开服务器")]
        public void Close()
        {
            if (tcpClient != null && IsConnected)
            {
                tcpClient.Close();
                IsConnected = false;
                Debug.LogError("主动断开连接");
                ConnectAbort.Invoke();
            }
        }
        void OnDestroy()
        {
            Close();
        }
    }
}
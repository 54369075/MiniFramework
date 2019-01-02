﻿using System;
using System.Runtime.InteropServices;
using UnityEngine;
namespace MiniFramework
{
    /// <summary>
    /// 消息头
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct PackHead
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public int MsgID;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public int BodyLength;
    }
    public class DataPacker
    {
        public int HeadLength = 8;
        public byte[] OtherBytes;
        public byte[] Packer(PackHead head, byte[] bodyData)
        {
            byte[] headData = SerializeFactory.Binary.SerializeByMarshal(head);
            byte[] packData = new byte[headData.Length + bodyData.Length];
            Array.Copy(headData, packData, headData.Length);
            Array.Copy(bodyData,0,packData,headData.Length,bodyData.Length);
            return packData;
        }

        public void UnPack(byte[] data)
        {
            byte[] totalData = new byte[OtherBytes.Length + data.Length];
            Array.Copy(OtherBytes, totalData, OtherBytes.Length);
            Array.Copy(data,0,totalData,OtherBytes.Length,data.Length);

            if (totalData.Length < HeadLength)
            {
                Debug.Log("接收消息头不足！继续接收");
                OtherBytes = totalData;
                return;
            }
            byte[] headData = new byte[HeadLength];
            Array.Copy(totalData, headData, HeadLength);
            PackHead head = SerializeFactory.Binary.DeserializeByMarshal<PackHead>(headData);
            if (totalData.Length < head.BodyLength + HeadLength)
            {
                Debug.Log("接收消息体不足！继续接收");
                OtherBytes = totalData;
                return;
            }
            byte[] bodyData = new byte[head.BodyLength];
            Array.Copy(totalData, HeadLength, bodyData, 0, bodyData.Length);
            SendPack(head, bodyData);
            OtherBytes = new byte[0];
            int leftLength = totalData.Length - HeadLength - bodyData.Length;
            if (leftLength > HeadLength)
            {
                Debug.Log("进行拆包");
                byte[] leftBytes = new byte[leftLength];
                Array.Copy(totalData, HeadLength + head.BodyLength, leftBytes, 0, leftLength);
                UnPack(leftBytes);
                return;
            }

            OtherBytes = new byte[totalData.Length - HeadLength - bodyData.Length];
            Array.Copy(totalData, HeadLength + bodyData.Length, OtherBytes, 0, OtherBytes.Length);
        }

        public void SendPack(PackHead head, byte[] bodyData)
        {
            MsgManager.Instance.SendMsg(head.MsgID + "", bodyData);
        }
    }
}

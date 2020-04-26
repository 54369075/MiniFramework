﻿using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace MiniFramework.WebRequest
{
    public partial class WebRequestManager
    {
        private class Downloader : IDownloader
        {
            public float downloadProgress { get; set; }
            /// <summary>
            /// 下载速度 单位（kb/s）
            /// </summary>
            public int downloadSpeed { get; set; }
            public string downloadFileName { get; set; }

            public string downloadFilePath { get; set; }

            public string downloadSaveDir { get; set; }

            public long curlength { get; set; }

            public long totalLength { get; set; }

            public bool isError { get; set; }
            public bool isCompleted { get; set; }

            public event Action onDownloadCompleted;

            public event Action onDownloadError;

            private float time;
            private int unitTotalLength;
            public Downloader(string dir)
            {
                downloadSaveDir = dir;
            }

            public IEnumerator Get(string url)
            {
                string[] txts = url.Split('?');
                downloadFileName = txts[0].Substring(url.LastIndexOf('/') + 1);
                downloadFilePath = downloadSaveDir + "/" + downloadFileName;
                string tempFilePath = downloadFilePath + ".tmp";
                curlength = FileUtil.GetFileLength(tempFilePath);
                isError = false;
                using (UnityWebRequest headRequest = UnityWebRequest.Head(url))
                {
                    yield return headRequest.SendWebRequest();
                    if (headRequest.isHttpError || headRequest.isNetworkError)
                    {
                        Debug.LogError("下载文件：" + downloadFileName + " " + headRequest.error);
                        onDownloadError?.Invoke();
                        isError = true;
                        yield break;
                    }
                    string contentLength = headRequest.GetResponseHeader("Content-Length");
                    if (!string.IsNullOrEmpty(contentLength))
                    {
                        totalLength = long.Parse(contentLength);
                    }
                    else
                    {
                        Debug.LogError("无法获取资源大小");
                        onDownloadError?.Invoke();
                        isError = true;
                        yield break;
                    }
                }
                Debug.Log("开始下载:" + downloadFileName + " 大小:" + UnitConvert.ByteConvert(totalLength) + " 已下载:" + UnitConvert.ByteConvert(curlength));
                using (UnityWebRequest request = UnityWebRequest.Get(url))
                {
                    request.SetRequestHeader("Range", "bytes=" + curlength + "-");
                    request.SendWebRequest();
                    using (FileStream fileStream = new FileStream(tempFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        fileStream.Seek(curlength, SeekOrigin.Begin);
                        int index = 0;
                        while (!request.isDone)
                        {
                            yield return null;

                            byte[] data = request.downloadHandler.data;
                            int writeLength = data.Length - index;
                            fileStream.Write(data, index, writeLength);
                            index = data.Length;
                            curlength += writeLength;
                            downloadProgress = curlength * 1.0f / totalLength;

                            time += Time.deltaTime;
                            unitTotalLength += writeLength;
                            if (time > 1f)
                            {
                                downloadSpeed = unitTotalLength / 1024;
                                time = 0;
                                unitTotalLength = 0;
                            }
                        }

                        if (request.isHttpError || request.isNetworkError)
                        {
                            Debug.LogError(request.error);
                            onDownloadError?.Invoke();
                            isError = true;
                            yield break;
                        }
                    }
                    if (File.Exists(downloadFilePath))
                    {
                        File.Delete(downloadFilePath);
                    }
                    File.Move(tempFilePath, downloadFilePath);
                    Debug.Log("下载成功:" + downloadFileName);
                    isCompleted = true;
                    onDownloadCompleted?.Invoke();
                }
            }
        }
    }
}
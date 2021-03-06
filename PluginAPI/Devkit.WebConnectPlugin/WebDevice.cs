﻿using System;
using System.Net;
using System.Text;
using Devkit.Interfaces;

namespace Devkit.WebConnectPlugin
{
    public class WebDevice : MemoryDevice
    {
        public const ushort DefaultPort = 0xa000;

        public enum ResultCode : ushort
        {
            End = 0xf000,
            Error = 0xff00
        }

        private ushort _port;
        private StringBuilder _downloadUrl;
        private byte[] _webData;
        private int _webDataPtr = 0;

        public WebDevice(ushort port = DefaultPort) : base(port, port)
        {
            this._port = port;
        }

        public override void Reset()
        {
            this._webData = null;
            this._webDataPtr = 0;
        }

        public override ushort Read(ushort address)
        {
            // no data ready? return error code
            if (this._webData == null) return (ushort)ResultCode.Error;

            // read all available data? return end code
            if (this._webDataPtr >= this._webData.Length) return (ushort)ResultCode.End;

            // read the next byte of data
            return this._webData[this._webDataPtr++];
        }

        public override void Write(ushort address, ushort data)
        {
            // if 0 is written, issue request to queued-up url
            if (data == 0)
            {
                try
                {
                    var url = this._downloadUrl.ToString();
                    this._webData = new WebClient().DownloadData(url);
                }
                catch
                {
                    // any errors at all, simply indicate no data available for simplicity
                    this._webData = null;
                }
                _webDataPtr = 0;
                _downloadUrl = null;
                return;
            }

            // append to the url
            if (this._downloadUrl == null) this._downloadUrl = new StringBuilder();
            this._downloadUrl.Append((char)data);
        }
    }
}

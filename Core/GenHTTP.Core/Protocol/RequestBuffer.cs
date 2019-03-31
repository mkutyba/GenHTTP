﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GenHTTP.Core.Protocol
{

    internal class RequestBuffer : IDisposable
    {
        private static readonly Encoding ENCODING = Encoding.GetEncoding("ISO-8859-1");
        
        #region Get-/Setters
        
        public MemoryStream Data { get; }

        private StringBuilder? StringBuffer { get; set; }
                
        #endregion

        #region Initialization

        public RequestBuffer()
        {
            Data = new MemoryStream();
            StringBuffer = null;
        }

        public RequestBuffer(byte[] data)
        {
            Data = new MemoryStream(data);
            StringBuffer = null;
        }

        #endregion

        #region Functionality

        public async Task Append(byte[] data, int bytesRead)
        {
            var position = Data.Position;

            Data.Seek(0, SeekOrigin.End);

            await Data.WriteAsync(data, 0, bytesRead);

            Data.Seek(position, SeekOrigin.Begin);

            StringBuffer = null;
        }
        
        public async Task<string> GetString()
        {
            if (StringBuffer != null)
            {
                return StringBuffer.ToString();
            }

            var position = Data.Position;
            var result = "";
            
            using (var reader = new StreamReader(Data, ENCODING, false, 1024, true))
            {
                result = await reader.ReadToEndAsync();
            }

            Data.Seek(position, SeekOrigin.Begin);

            StringBuffer = new StringBuilder(result);

            return result;
        }

        public void Advance(ushort bytes)
        {
            Data.Seek(bytes, SeekOrigin.Current);
            StringBuffer?.Remove(0, bytes);
        }

        public async Task<int> Migrate(Stream target, long maxBytes)
        {
            var available = (int)Math.Min(Data.Length - Data.Position, maxBytes);

            if (available > 0)
            {
                var buffer = new byte[available];

                await Data.ReadAsync(buffer, 0, available);

                await target.WriteAsync(buffer, 0, available);

                StringBuffer = null;

                return available;
            }

            return 0;
        }

        public async Task<RequestBuffer> GetNext()
        {
            if (Data.Position < Data.Length)
            {
                var remaining = new byte[Data.Length - Data.Position];
                await Data.ReadAsync(remaining, 0, remaining.Length);

                return new RequestBuffer(remaining);
            }

            return new RequestBuffer();
        }

        #endregion

        #region IDisposable Support

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    Data.Dispose();
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

    }

}
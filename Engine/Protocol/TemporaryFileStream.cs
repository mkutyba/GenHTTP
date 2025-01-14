﻿using System;
using System.IO;

namespace GenHTTP.Engine.Protocol
{

    internal sealed class TemporaryFileStream : FileStream
    {

        #region Get-/Setters

        internal string TemporaryFile { get; }

        #endregion

        #region Initialization

        private TemporaryFileStream(string file) : base(file, FileMode.CreateNew, FileAccess.ReadWrite)
        {
            TemporaryFile = file;
        }

        internal static Stream Create()
        {
            return new TemporaryFileStream(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".genhttp.tmp"));
        }

        #endregion

        #region Lifecycle

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                File.Delete(TemporaryFile);
            }
        }

        #endregion

    }

}

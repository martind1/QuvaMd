﻿/*
Copyright (c) 2018-2020 Rossmann-Engineering
Permission is hereby granted, free of charge, 
to any person obtaining a copy of this software
and associated documentation files (the "Software"),
to deal in the Software without restriction, 
including without limitation the rights to use, 
copy, modify, merge, publish, distribute, sublicense, 
and/or sell copies of the Software, and to permit 
persons to whom the Software is furnished to do so, 
subject to the following conditions:

The above copyright notice and this permission 
notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, 
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Quva.Services.Devices.EasyModbus
{
    /// <summary>
    /// Store Log-Data in a File
    /// </summary>
    public sealed class StoreLogData
    {
        private string? filename = null;
        private static volatile StoreLogData? instance;
        private static object syncObject = new object();

        /// <summary>
        /// Private constructor; Ensures the access of the class only via "instance"
        /// </summary>
        private StoreLogData()
        {
        }

        /// <summary>
        /// Returns the instance of the class (singleton)
        /// </summary>
        /// <returns>instance (Singleton)</returns>
        public static StoreLogData Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncObject)
                    {
                        if (instance == null)
                            instance = new StoreLogData();
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Store message in Log-File
        /// </summary>
        /// <param name="message">Message to append to the Log-File</param>
        public void Store(string message)
        {
            if (filename == null)
                return;

            using StreamWriter file =
                new StreamWriter(Filename, true);
            file.WriteLine(message);
        }

        /// <summary>
        /// Store message in Log-File including Timestamp
        /// </summary>
        /// <param name="message">Message to append to the Log-File</param>
        /// <param name="timestamp">Timestamp to add to the same Row</param>
        public void Store(string message, DateTime timestamp)
        {
            try
            {
                using StreamWriter file =
                    new StreamWriter(Filename, true);
                file.WriteLine(timestamp.ToString("dd.MM.yyyy H:mm:ss.ff ") + message);
            }
            catch
            {

            }
        }

        /// <summary>
        /// Gets or Sets the Filename to Store Strings in a File
        /// </summary>
        public string Filename
        {
            get
            {
                return filename ?? "null";
            }
            set
            {
                filename = value;
            }
        }
    }
}
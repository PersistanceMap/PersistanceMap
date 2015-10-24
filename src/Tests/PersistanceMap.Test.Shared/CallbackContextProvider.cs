﻿using PersistanceMap.Tracing;
using System;
using System.Data;

namespace PersistanceMap.Test
{
    public delegate void ContextProviderCallbackHandler(string query);

    /// <summary>
    /// Represents a IContextProvider that only compares the generated sql string to a expected sql string withou executing to a database
    /// </summary>
    public class CallbackContextProvider : IContextProvider
    {
        public CallbackContextProvider()
        {
        }

        public CallbackContextProvider(Action<string> callback)
        {
            ConnectionProvider = new CallbackConnectionProvider((s) => callback(s));
        }

        public IConnectionProvider ConnectionProvider { get; private set; }

        public virtual DatabaseContext Open()
        {
            return new DatabaseContext(ConnectionProvider, new LoggerFactory());
        }

        #region IDisposeable Implementation

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        internal bool IsDisposed { get; private set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Releases resources held by the object.
        /// </summary>
        public virtual void Dispose(bool disposing)
        {
            lock (this)
            {
                if (disposing && !IsDisposed)
                {
                    ConnectionProvider.Dispose();

                    IsDisposed = true;
                    GC.SuppressFinalize(this);
                }
            }
        }

        /// <summary>
        /// Releases resources before the object is reclaimed by garbage collection.
        /// </summary>
        ~CallbackContextProvider()
        {
            Dispose(false);
        }

        #endregion
        
        public class CallbackReaderContext : IReaderContext
        {
            public System.Data.IDataReader DataReader
            {
                get
                {
                    return new CallbackDataReader();
                }
            }

            public void Close()
            {
            }

            public void Dispose()
            {
            }
        }

        public class CallbackDataReader : IDataReader
        {

            public void Close()
            {
            }

            public int Depth
            {
                get { return 0; }
            }

            public DataTable GetSchemaTable()
            {
                throw new NotImplementedException();
            }

            public bool IsClosed
            {
                get { return true; }
            }

            public bool NextResult()
            {
                return false;
            }

            public bool Read()
            {
                return false;
            }

            public int RecordsAffected
            {
                get { return 0; }
            }

            public void Dispose()
            {
            }

            public int FieldCount
            {
                get { return 0; }
            }

            #region Not Implemented

            public bool GetBoolean(int i)
            {
                throw new NotImplementedException();
            }

            public byte GetByte(int i)
            {
                throw new NotImplementedException();
            }

            public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
            {
                throw new NotImplementedException();
            }

            public char GetChar(int i)
            {
                throw new NotImplementedException();
            }

            public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
            {
                throw new NotImplementedException();
            }

            public IDataReader GetData(int i)
            {
                throw new NotImplementedException();
            }

            public string GetDataTypeName(int i)
            {
                throw new NotImplementedException();
            }

            public DateTime GetDateTime(int i)
            {
                throw new NotImplementedException();
            }

            public decimal GetDecimal(int i)
            {
                throw new NotImplementedException();
            }

            public double GetDouble(int i)
            {
                throw new NotImplementedException();
            }

            public Type GetFieldType(int i)
            {
                throw new NotImplementedException();
            }

            public float GetFloat(int i)
            {
                throw new NotImplementedException();
            }

            public Guid GetGuid(int i)
            {
                throw new NotImplementedException();
            }

            public short GetInt16(int i)
            {
                throw new NotImplementedException();
            }

            public int GetInt32(int i)
            {
                throw new NotImplementedException();
            }

            public long GetInt64(int i)
            {
                throw new NotImplementedException();
            }

            public string GetName(int i)
            {
                throw new NotImplementedException();
            }

            public int GetOrdinal(string name)
            {
                throw new NotImplementedException();
            }

            public string GetString(int i)
            {
                throw new NotImplementedException();
            }

            public object GetValue(int i)
            {
                throw new NotImplementedException();
            }

            public int GetValues(object[] values)
            {
                throw new NotImplementedException();
            }

            public bool IsDBNull(int i)
            {
                throw new NotImplementedException();
            }

            public object this[string name]
            {
                get { throw new NotImplementedException(); }
            }

            public object this[int i]
            {
                get { throw new NotImplementedException(); }
            }

            #endregion
        }

        public class CallbackConnectionProvider : IConnectionProvider
        {
            public ContextProviderCallbackHandler Callback;
            private bool _callbackCalled = false;

            public CallbackConnectionProvider(ContextProviderCallbackHandler callback)
            {
                Callback = callback;
                CheckCallbackCall = true;
            }

            public bool CheckCallbackCall { get; set; }

            public string Database { get; set; }

            private IQueryCompiler _queryCompiler;
            public virtual IQueryCompiler QueryCompiler
            {
                get
                {
                    if (_queryCompiler == null)
                        _queryCompiler = new QueryCompiler();

                    return _queryCompiler;
                }
            }

            public IReaderContext Execute(string query)
            {
                ExecuteNonQuery(query);

                return new CallbackReaderContext();
            }

            public void ExecuteNonQuery(string query)
            {
                if (Callback == null)
                    throw new ArgumentNullException("Callback was not set prior to execution");

                Callback(query);

                _callbackCalled = true;
            }

            #region IDisposeable Implementation

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        internal bool IsDisposed { get; private set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Releases resources held by the object.
        /// </summary>
        public virtual void Dispose(bool disposing)
        {
            lock (this)
            {
                if (disposing && !IsDisposed)
                {
                    if (CheckCallbackCall && _callbackCalled == false)
                        throw new Exception("Callback was not called by client");

                    IsDisposed = true;
                    GC.SuppressFinalize(this);
                }
            }
        }

        /// <summary>
        /// Releases resources before the object is reclaimed by garbage collection.
        /// </summary>
        ~CallbackConnectionProvider()
        {
            Dispose(false);
        }

        #endregion
        }
    }
}
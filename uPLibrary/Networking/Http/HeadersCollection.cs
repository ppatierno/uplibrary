using System;
using System.Collections;

namespace uPLibrary.Networking.Http
{
    /// <summary>
    /// Headers collection (key/value pairs collection specialized string/string)
    /// </summary>
    public class HeadersCollection : IEnumerable
    {
        #region Fields...

        private Hashtable headers;

        #endregion

        #region Properties...

        /// <summary>
        /// Number of key/value pairs into collection
        /// </summary>
        public int Count
        {
            get { return this.headers.Count; }
        }

        /// <summary>
        /// Gets or sets the multiplier to use to increase the collection size during a rehash.
        /// </summary>
        //public double GrowthFactor 
        //{
        //    get { return this.headers.GrowthFactor; }
        //    set { this.headers.GrowthFactor = value; }
        //}

        /// <summary>
        /// Gets a value indicating whether the collection has a fixed size
        /// </summary>
        public bool IsFixedSize
        {
            get { return this.headers.IsFixedSize; }
        }

        /// <summary>
        /// Gets a value indicating whether the collection is read-only
        /// </summary>
        public bool IsReadOnly
        {
            get { return this.headers.IsReadOnly; }
        }

        /// <summary>
        /// Gets a value indicating whether access to the collection is synchronized (thread safe)
        /// </summary>
        public bool IsSynchronized
        {
            get { return this.headers.IsSynchronized; }
        }

        /// <summary>
        /// Indexer for the collection
        /// </summary>
        /// <param name="key">>Key for access value</param>
        /// <returns>Value for the provided key</returns>
        public string this[string key]
        {
            get { return (string)this.headers[key]; }
            set { this.headers[key] = value; }
        }

        /// <summary>
        /// Keys collection
        /// </summary>
        public ICollection Keys
        {
            get { return this.headers.Keys; }
        }

        /// <summary>
        /// Gets or sets the load factor that results in a rehash with a greater number of buckets
        /// </summary>
        //public int MaxLoadFactor
        //{
        //    get { return this.headers.MaxLoadFactor; }
        //    set { this.headers.MaxLoadFactor = value; }
        //}

        /// <summary>
        /// Gets an object that can be used to synchronize access to the collection
        /// </summary>
        public object SyncRoot
        {
            get { return this.headers.SyncRoot; }
        }

        /// <summary>
        /// Values collection
        /// </summary>
        public ICollection Values
        {
            get { return this.headers.Values; }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public HeadersCollection()
        {
            this.headers = new Hashtable();
        }

        /// <summary>
        /// Add a new value into collection with the specified key
        /// </summary>
        /// <param name="key">Key for value</param>
        /// <param name="value">Value to insert into collection</param>
        public void Add(string key, string value)
        {
            if (this.Contains(key))
            {
                // manage multiple message-header fields with same name (RFC2616, sec 4.2)
                // http://www.w3.org/Protocols/rfc2616/rfc2616-sec4.html#sec4.2
                this.headers[key] = this.headers[key] + "," + value;
            }
            else
            {
                this.headers.Add(key, value);
            }
        }

        /// <summary>
        /// Clear the collection
        /// </summary>
        public void Clear()
        {
            this.headers.Clear();
        }

        /// <summary>
        /// Verify if collection contains a key
        /// </summary>
        /// <param name="key">Key to verifiy into collection</param>
        /// <returns>Key is into collection or not</returns>
        public bool Contains(string key)
        {
            if ((key == null) || (key == String.Empty))
                throw new ArgumentException("key is null or empty");
            return this.headers.Contains(key);
        }

        /// <summary>
        /// Copies the elements
        /// </summary>
        /// <param name="array">Array destination of the DictionaryEntry objects copied</param>
        /// <param name="index">The zero-based index in array at which copying begins</param>
        public void CopyTo(Array array, int index)
        {
            this.headers.CopyTo(array, index);
        }

        /// <summary>
        /// Remove an element with the specified key from collection
        /// </summary>
        /// <param name="key">Key for value to remove</param>
        public void Remove(string key)
        {
            if (!this.Contains(key))
                throw new ArgumentException("key doesn't exist into collection");
            this.headers.Remove(key);
        }

        public IEnumerator GetEnumerator()
        {
            return this.headers.GetEnumerator();
        }
    }
}

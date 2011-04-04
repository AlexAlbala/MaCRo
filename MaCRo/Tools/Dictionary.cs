using System;
using System.Collections;

namespace MaCRo.Tools
{
    public class Dictionary
    {
        /*
        Credit for primes table: Aaron Krowne
         http://planetmath.org/encyclopedia/GoodHashTablePrimes.html
        */
        protected static uint[] primes = {
            29, 53, 97, 193, 389,769, 1543, 3079, 6151,12289, 24593, 49157, 98317,
            196613, 393241, 786433, 1572869,3145739, 6291469, 12582917, 25165843,            
            50331653, 100663319, 201326611, 402653189,805306457, 1610612741            
            };
        private static int ListLength = 5;

        protected ArrayList[] Table;
        private int count;
        private byte PrimeIndex;

        public Dictionary()
        {
            Table = new ArrayList[primes[0]];
            PrimeIndex = 0;
            count = 0;

            for (int i = 0; i < Table.Length; i++)
            {
                Table[i] = new ArrayList();
            }
        }

        public int Count
        {
            get { return count; }
        }

        public void Clear()
        {
            for (int i = 0; i < Table.Length; i++)
            {
                Table[i].Clear();
            }
        }

        public void Add(object key, object value)
        {
            if (key == null) throw new ArgumentNullException("Key cannot be null");

            uint hash = (uint)key.GetHashCode();
            uint pos = (uint)(hash % Table.Length);
            KeyValuePair kvp = new KeyValuePair(key, value);

            // Option #1: Add pair to a bucket
            if (Table[pos].Count < ListLength)
            {
                if (ExistsInBucket(pos, key))
                    throw new ArgumentException("Key already found");
                else
                    Table[pos].Add(kvp);
            }
            else
            {
                // Option #2: Resize of the HasTable is needed
                Resize(PrimeIndex);
                PrimeIndex++;
                pos = (uint)(hash % Table.Length);
                if (ExistsInBucket(pos, key))
                    throw new ArgumentException("Key already found");
                else
                    Table[pos].Add(kvp);
            }

            count++;
        }

        public bool Remove(object Key)
        {
            uint pos = (uint)((uint)Key.GetHashCode() % Table.Length);
            int val = FindInBucket(pos, Key);

            if (val < 0)
                return false;
            else
            {
                Table[pos].RemoveAt(val);
                count--;
                return true;
            }
        }

        public bool TryGetValue(object Key, out object Value)
        {
            uint pos = (uint)((uint)Key.GetHashCode() % Table.Length);
            KeyValuePair kvp;

            for (int i = 0; i < Table[pos].Count; i++)
            {
                kvp = (KeyValuePair)Table[pos][i];
                if (kvp.Key.Equals(Key))
                {
                    Value = kvp.Value;
                    return true;
                }
            }

            Value = null;
            return false;
        }

        public bool ContainsKey(object Key)
        {
            uint pos = (uint)((uint)Key.GetHashCode() % Table.Length);
            return ExistsInBucket(pos, Key);
        }

        public object this[object Key]
        {
            get { return Get(Key); }
            set { Set(Key, value); }
        }

        public object Get(object Key)
        {
            object Value;
            if (TryGetValue(Key, out Value))
            {
                return Value;
            }
            else
            {
                throw new ArgumentException("Key NOT found");
            }
        }

        public void Set(object Key, object Value)
        {
            Add(Key, Value);
        }

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return (IEnumerator)new DictionaryEnumerator(this);
        }

        #endregion

        public IEnumerable Keys
        {
            get
            {
                return new DictionaryCollection(
                  new DictionaryEnumerator(this, DictionaryEnumerator.IterateOn.KEYS)
                    );
            }
        }

        public IEnumerable Values
        {
            get
            {
                return new DictionaryCollection(
                  new DictionaryEnumerator(this, DictionaryEnumerator.IterateOn.VALUES)
                );
            }
        }

        private class DictionaryCollection : IEnumerable
        {
            IEnumerator enumerator;

            public DictionaryCollection(DictionaryEnumerator e)
            {
                enumerator = e;
            }

            public IEnumerator GetEnumerator()
            {
                return enumerator;
            }
        }

        private class DictionaryEnumerator : IEnumerator
        {
            public enum IterateOn { KEYS, VALUES, BOTH };
            private Dictionary d;
            private int bucket;
            private int index;
            private IterateOn iterateOn = IterateOn.BOTH;

            public DictionaryEnumerator(Dictionary d)
            {
                this.d = d;
                bucket = 0;
                index = 0;
            }

            public DictionaryEnumerator(Dictionary d, IterateOn it)
            {
                this.d = d;
                bucket = 0;
                index = 0;
                iterateOn = it;
            }

            #region IEnumerator Members

            public void Reset()
            {
                bucket = 0;
                index = 0;
            }

            public object Current
            {
                get
                {
                    switch (iterateOn)
                    {
                        case IterateOn.KEYS:
                            return ((KeyValuePair)d.Table[bucket][index]).Key;
                        case IterateOn.VALUES:
                            return ((KeyValuePair)d.Table[bucket][index]).Value;
                        default: //IterateOn.BOTH:
                            return d.Table[bucket][index];
                    }
                }
            }

            public bool MoveNext()
            {
                if (index < d.Table[bucket].Count - 1)
                {
                    index++;
                }
                else
                {
                    index = 0;
                    while (bucket < d.Table.Length - 1)
                    {
                        bucket++;
                        if (d.Table[bucket].Count > 0)
                            return true;
                    }
                }

                if (bucket == d.Table.Length - 1)
                    return false;
                else
                    return true;
            }

            #endregion
        }

        private void Resize(byte index)
        {
            if (index >= primes.Length) throw new Exception("Dictionary exceeded maximum size");

            ArrayList[] newTable = new ArrayList[primes[index]];
            uint hash;

            // Initialize new hash table
            for (int i = 0; i < newTable.Length; i++)
            {
                newTable[i] = new ArrayList();
            }

            // Redistribute items
            KeyValuePair kvp;
            for (int i = 0; i < Table.Length; i++)
            {
                for (int j = 0; j < Table[i].Count; j++)
                {
                    kvp = (KeyValuePair)Table[i][j];
                    hash = (uint)kvp.Key.GetHashCode();
                    newTable[hash % newTable.Length].Add(kvp);
                }
            }

            // Use new hash table
            Table = newTable;
        }

        private bool ExistsInBucket(uint bucket, object key)
        {
            if (FindInBucket(bucket, key) < 0)
                return false;
            else
                return true;
        }

        private int FindInBucket(uint bucket, object key)
        {
            KeyValuePair kvp;

            for (int i = 0; i < Table[bucket].Count; i++)
            {
                kvp = (KeyValuePair)Table[bucket][i];
                if (kvp.Key.Equals(key))
                    return i;
            }

            return -1;
        }
    }

    public class KeyValuePair
    {
        public object Key;
        public object Value;

        public KeyValuePair()
        {
            Key = null;
            Value = null;
        }

        public KeyValuePair(object Key, object Value)
        {
            this.Key = Key;
            this.Value = Value;
        }

        public override String ToString()
        {
            return Key + "=>" + Value;
        }
    }
}

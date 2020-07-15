namespace WebRequest.Elegant.Offline
{
    public static class StringHash
    {
        public static long RSHash(string str)
        {
            const int b = 378551;
            int a = 63689;
            int hash = 0;

            for (int i = 0; i < str.Length; i++)
            {
                hash = hash * a + str[i];
                a = a * b;
            }

            return hash;
        }

        public static long JSHash(string str)
        {
            long hash = 1315423911;

            for (int i = 0; i < str.Length; i++)
            {
                hash ^= (hash << 5) + str[i] + (hash >> 2);
            }

            return hash;
        }

        public static long ELFHash(string str)
        {
            long hash = 0;

            for (int i = 0; i < str.Length; i++)
            {
                hash = (hash << 4) + str[i];

                long x;
                if ((x = hash & 0xF0000000L) != 0)
                {
                    hash ^= x >> 24;
                }

                hash &= ~x;
            }

            return hash;
        }

        public static long BKDRHash(string str)
        {
            const long seed = 131; // 31 131 1313 13131 131313 etc..
            long hash = 0;

            for (int i = 0; i < str.Length; i++)
            {
                hash = (hash * seed) + str[i];
            }

            return hash;
        }

        public static long SDBMHash(string str)
        {
            long hash = 0;

            for (int i = 0; i < str.Length; i++)
            {
                hash = str[i] + (hash << 6) + (hash << 16) - hash;
            }

            return hash;
        }

        public static long DJBHash(string str)
        {
            long hash = 5381;

            for (int i = 0; i < str.Length; i++)
            {
                hash = ((hash << 5) + hash) + str[i];
            }

            return hash;
        }

        public static long DEKHash(string str)
        {
            long hash = str.Length;

            for (int i = 0; i < str.Length; i++)
            {
                hash = ((hash << 5) ^ (hash >> 27)) ^ str[i];
            }

            return hash;
        }

        public static long BPHash(string str)
        {
            long hash = 0;

            for (int i = 0; i < str.Length; i++)
            {
                hash = hash << 7 ^ str[i];
            }

            return hash;
        }

        public static long FNVHash(string str)
        {
            long fnv_prime = 0x811C9DC5;
            long hash = 0;

            for (int i = 0; i < str.Length; i++)
            {
                hash *= fnv_prime;
                hash ^= str[i];
            }

            return hash;
        }

        public static long APHash(string str)
        {
            long hash = 0xAAAAAAAA;

            for (int i = 0; i < str.Length; i++)
            {
                if ((i & 1) == 0)
                {
                    hash ^= (hash << 7) ^ str[i] * (hash >> 3);
                }
                else
                {
                    hash ^= ~((hash << 11) + str[i] ^ (hash >> 5));
                }
            }

            return hash;
        }
    }
}

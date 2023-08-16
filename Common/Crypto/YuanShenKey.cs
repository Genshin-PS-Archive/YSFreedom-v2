using System;
using System.Linq;
using BinaryEncoding;
using System.Net;
using System.Text;

// This class contains an implementation of miHoYo's awful XOR & mt64 based
// cryptography routines. Truly awful. I wonder if the developers suffered from terminal stupidity.

namespace YSFreedom.Common.Crypto
{
    public class YuanShenKey
    {
        public static readonly YuanShenKey NoOp = new YuanShenKey(new byte[4096]);
        public const int LEN = 4096;
        public byte[] Bytes;

        public YuanShenKey()
        {
            Bytes = new byte[LEN];
        }

        public YuanShenKey(byte[] buffer)
        {
            if (buffer.Length != LEN)
                throw new ArgumentException("Key must be 4096 bytes", "buffer");
            Bytes = buffer.ToArray();
        }

        public YuanShenKey(ulong seed)
        {
            Bytes = new byte[LEN];
            Bytes = GenerateKey(seed);
        }

        public void Crypt(byte[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] ^= Bytes[i % LEN];
        }

        public static YuanShenKey FromBase64(string text)
        {
            return new YuanShenKey(Convert.FromBase64String(text));
        }

        public static byte[] GenerateKey(ulong source)
        {
            var generator = new MT19937_64();
            generator.Initialize(source);
            var nextGenerator = new MT19937_64();
            nextGenerator.Initialize(generator.GenerateULong());
            nextGenerator.GenerateULong();
            var key = new byte[0x1000];
            for (var i = 0; i < key.Length; i += sizeof(ulong))
            {
                var value = nextGenerator.GenerateULong();
                Binary.BigEndian.Set(value, key, i);
            }
            return key;
        }

        private class MT19937_64
        {
            public const int N = 312;
            private const int M = 156;
            private const ulong MatrixA = 0xB5026F5AA96619E9UL;

            private readonly ulong[] mt = new ulong[N * 2];
            private uint mti = N;

            public void Initialize(ulong[] src, uint mtiSrc = 0)
            {
                if (src.Length != mt.Length)
                {
                    throw new ArgumentException("NN", nameof(src));
                }
                Array.Copy(src, 0, mt, 0, src.Length);
                mti = mtiSrc;
            }

            public void Initialize(ulong seed)
            {
                var mt = this.mt;
                mt[0] = seed;
                for (mti = 1; mti < N; mti++)
                {
                    mt[mti] = (6364136223846793005uL * (mt[mti - 1] ^ (mt[mti - 1] >> 62)) + mti);
                }
            }

            public ulong GetMag01(ulong val)
            {
                return (val & 1) != 0 ? MatrixA : 0;
            }

            public void Regenerate1()
            {
                for (var i = N; i < N * 2; i++)
                {
                    var v9 = i - N;
                    var v14 = mt[v9] ^ ((mt[v9 + 1] ^ mt[v9]) & 0x7FFFFFFF);
                    mt[i] = GetMag01(v14) ^ (v14 >> 1) ^ mt[i + (M - N)];
                }
            }

            public void Regenerate2()
            {
                var i = 0;
                for (; i < N - M; i++)
                {
                    var v9 = i + N;
                    var v13 = mt[v9] ^ ((mt[v9 + 1] ^ mt[v9]) & 0x7FFFFFFF);
                    mt[i] = GetMag01(v13) ^ (v13 >> 1) ^ mt[i + N + M];
                }
                for (; i < N - 1; i++)
                {
                    var v21 = i + N;
                    var v25 = mt[v21] ^ ((mt[v21 + 1] ^ mt[v21]) & 0x7FFFFFFF);
                    mt[i] = GetMag01(v25) ^ (v25 >> 1) ^ mt[i + (M - N)];
                }
                var v36 = mt[i + N] ^ ((mt[0] ^ mt[i + N]) & 0x7FFFFFFF);
                mt[i] = GetMag01(v36) ^ (v36 >> 1) ^ mt[M - 1];
                mti = 0;
            }

            public ulong GenerateULong()
            {
                if (mti == N)
                {
                    Regenerate1();
                }
                else
                {
                    if (2 * N <= mti)
                    {
                        Regenerate2();
                    }
                }

                var x = mt[mti++];

                var v12 = ((((x >> 29) & 0x555555555uL ^ x) & 0x38EB3FFFF6D3uL) << 17) ^ (x >> 29) & 0x555555555uL ^ x;
                x = ((v12 & 0xFFFFFFFFFFFFBF77uL) << 37) ^ v12 ^ ((((v12 & 0xFFFFFFFFFFFFBF77uL) << 37) ^ v12) >> 43);

                return x;
            }
        }
    }
}

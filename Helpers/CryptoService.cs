using System.Collections;
using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace vkr_bank.Helpers
{
    public class CryptoService
    {
        // для srp
        public string getSha512(string data)
        {
            byte[] byteData = Encoding.UTF8.GetBytes(data);
            byte[] hash;

            using (SHA256 shaM = new SHA256Managed())
            {
                hash = shaM.ComputeHash(byteData);
            }

            return BitConverter.ToString(hash).Replace("-", "");
        }
        public byte[] getSha256(string data)
        {
            byte[] byteData = Encoding.UTF8.GetBytes(data);
            byte[] hash;

            using (SHA256 shaM = new SHA256Managed())
            {
                hash = shaM.ComputeHash(byteData);
            }
            return hash;
        }
        public int getRandomInt(int length)
        {
            int randomInt = 0;
            while (randomInt <= 0)
            {
                Random rnd = new Random();
                Byte[] bytes = new Byte[length];
                rnd.NextBytes(bytes);
                randomInt = BitConverter.ToInt32(bytes, 0);
            }
            return randomInt;
        }
        public int hashToInt(string hash)
        {
            var bytes = Encoding.UTF8.GetBytes(hash);
            return BitConverter.ToInt32(bytes, 0);
        }
        public string bytesToBin(byte[] data)
        {
            var res = string.Join(" ", data.Select(byt => Convert.ToString(byt, 2).PadLeft(8, '0')));
            return res.Replace(" ", "");
        }
        public string getXor(string data1, string data2)
        {
            string res = string.Empty;
            for (int i = 0; i < data1.Length; i++)
                res += ((Convert.ToInt32(data1[i]) + Convert.ToInt32(data2[i])) % 2).ToString();
            return res;
        }
        private BigInteger Mod(BigInteger x, BigInteger m) { return (x % m + m) % m; }
        public string to_Bin(BigInteger number)
        {
            BigInteger delimoe = number; string result = "";
            while (true)
            {
                BigInteger chastnoe = delimoe / 2;
                BigInteger ostatok = delimoe % 2;
                delimoe = chastnoe;
                result += ostatok.ToString();
                if (chastnoe == 0)
                    break;
            }
            char[] charArray = result.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
        public BigInteger ModPow(BigInteger a, BigInteger e, BigInteger N)
        {
            string bin = to_Bin(e);
            BigInteger[] resArr = new BigInteger[bin.Length];
            for (int i = 0; i < bin.Length; i++)
            {
                if (i == 0)
                {
                    resArr[i] = a;
                    continue;
                }
                if (bin[i] == '0')
                    resArr[i] = Mod(resArr[i - 1] * resArr[i - 1], N);
                if (bin[i] == '1')
                    resArr[i] = Mod(resArr[i - 1] * resArr[i - 1] * a, N);
            }
            return resArr[resArr.Length - 1];
        }
        public BigInteger byteArrayToBigInt(byte[] byteArray)
        {
            BigInteger value = 0;
            for (int i = byteArray.Length - 1; i >= 0; i--)
                value = (value * 256) + byteArray[i];
            return value;
        }

        // для поиска примитивного элемента
        static bool isPrime(int n)
        {
            // Corner cases
            if (n <= 1)
            {
                return false;
            }
            if (n <= 3)
            {
                return true;
            }

            // This is checked so that we can skip
            // middle five numbers in below loop
            if (n % 2 == 0 || n % 3 == 0)
            {
                return false;
            }

            for (int i = 5; i * i <= n; i = i + 6)
            {
                if (n % i == 0 || n % (i + 2) == 0)
                {
                    return false;
                }
            }

            return true;
        }
        static int power(int x, int y, int p)
        {
            int res = 1;     // Initialize result

            x = x % p; // Update x if it is more than or
                       // equal to p

            while (y > 0)
            {
                // If y is odd, multiply x with result
                if (y % 2 == 1)
                {
                    res = (res * x) % p;
                }

                // y must be even now
                y = y >> 1; // y = y/2
                x = (x * x) % p;
            }
            return res;
        }
        static void findPrimefactors(HashSet<int> s, int n)
        {
            // Print the number of 2s that divide n
            while (n % 2 == 0)
            {
                s.Add(2);
                n = n / 2;
            }

            // n must be odd at this point. So we can skip
            // one element (Note i = i +2)
            for (int i = 3; i <= Math.Sqrt(n); i = i + 2)
            {
                // While i divides n, print i and divide n
                while (n % i == 0)
                {
                    s.Add(i);
                    n = n / i;
                }
            }

            // This condition is to handle the case when
            // n is a prime number greater than 2
            if (n > 2)
            {
                s.Add(n);
            }
        }
        public int findPrimitive(int n)
        {
            HashSet<int> s = new HashSet<int>();

            // Check if n is prime or not
            if (isPrime(n) == false)
            {
                return -1;
            }

            // Find value of Euler Totient function of n
            // Since n is a prime number, the value of Euler
            // Totient function is n-1 as there are n-1
            // relatively prime numbers.
            int phi = n - 1;

            // Find prime factors of phi and store in a set
            findPrimefactors(s, phi);

            // Check for every number from 2 to phi
            for (int r = 2; r <= phi; r++)
            {
                // Iterate through all prime factors of phi.
                // and check if we found a power with value 1
                bool flag = false;
                foreach (int a in s)
                {

                    // Check if r^((phi)/primefactors) mod n
                    // is 1 or not
                    if (power(r, phi / (a), n) == 1)
                    {
                        flag = true;
                        break;
                    }
                }

                // If there was no power with value 1.
                if (flag == false)
                {
                    return r;
                }
            }

            // If no primitive root found
            return -1;
        }


        // для дешифрования
        public string Decrypt(string ciphertext, string passphrase)
        {
            Span<byte> encryptedData = Convert.FromBase64String(ciphertext).AsSpan();
            Span<byte> salt = encryptedData[..16]; // Fix 1: consider salt (and apply the correct parameters)
            Span<byte> nonce = encryptedData[16..(16 + 12)];
            Span<byte> data = encryptedData[(16 + 12)..^16];
            Span<byte> tag = encryptedData[^16..];

            using Rfc2898DeriveBytes pbkdf2 = new(Encoding.UTF8.GetBytes(passphrase), salt.ToArray(), 25000, HashAlgorithmName.SHA256); // Fix 2: apply the same iteration count

            using AesGcm aes = new(pbkdf2.GetBytes(32)); // Fix 3: use the same key size (e.g. 32 bytes for AES-256)
            Span<byte> result = new byte[data.Length];
            aes.Decrypt(nonce, data, tag, result);

            return Encoding.UTF8.GetString(result);
        }
    }
}

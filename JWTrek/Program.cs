using System;
using System.Collections;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.Globalization;
using System.Numerics;
using System.IO;
using System.Collections.Generic;

namespace JWTest
{
    class Program
    {
        public static Timer timer1;
        public static List<int> pile;
        public static int algo;
        public static string algoDisplay;
        public static int count;
        public static int tick = 0;
        public static bool found = false;
        public static bool live = false;
        public static string version = "v2.1";
        public static string defaultCharset = "abcdefghijklmnopqrstuvwxyz0123456789";
        public static int defaultLength = 6;       

        static void Main(string[] args)
        {           
            PrintBanner();
            bool rawTokenOk = false;
            bool rawTokenIsSupported = false;
            string rawToken = "";
            while (!rawTokenOk)
            {
                Console.WriteLine("\r\n[?] Please enter JWT token OR file path contains token (prefered for long token)");
                rawToken = Console.ReadLine();
                if (File.Exists(rawToken))
                {                    
                    rawToken = File.ReadAllText(rawToken);
                    rawToken = rawToken.Trim();
                }
                try
                {
                    byte[] btab = Base64UrlDcode(rawToken.Split('.')[0]);
                    var tokenHeader = Encoding.UTF8.GetString(btab, 0, btab.Length);                   
                    if (tokenHeader.IndexOf("hs128", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        rawTokenIsSupported = true;
                        algo = 1;
                        algoDisplay = "HS128";
                    }
                    else if (tokenHeader.IndexOf("hs256", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        rawTokenIsSupported = true;
                        algo = 2;
                        algoDisplay = "HS256";
                    }
                    else if (tokenHeader.IndexOf("hs384", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        rawTokenIsSupported = true;
                        algo = 3;
                        algoDisplay = "HS384";
                    }
                    else if (tokenHeader.IndexOf("hs512", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        rawTokenIsSupported = true;
                        algo = 4;
                        algoDisplay = "HS512";
                    }
                    if (!rawTokenIsSupported)
                    {
                        Console.WriteLine("[!] Algorithm not supported");
                    }
                }
                catch (Exception ex) 
                {
                    Console.WriteLine(@"/!\ Token parsing error : " + ex.Message);
                }
                rawTokenOk = rawTokenIsSupported && (rawToken != string.Empty) && (rawToken.Split('.').Length == 3);
            }           

            Console.WriteLine("\r\n[?] Please enter custom charset [default: "+ defaultCharset + "]");
            string charset = Console.ReadLine();
            if ((charset == null) || (charset == string.Empty))
            {
                charset = defaultCharset;
            }

            Console.WriteLine("\r\n[?] Please enter length [default: "+ defaultLength + "]");
            if (!int.TryParse(Console.ReadLine(), out int realLength))
            {
                realLength = defaultLength;
            }

            Console.WriteLine("\r\n[?] Live monitoring ? y/n");
            live = Console.ReadLine().Equals("y");     

            Console.Clear();
            PrintBanner();
            Console.WriteLine("[TOKEN] > " + rawToken);
            Console.WriteLine("[CHARSET] > " + charset);
            Console.WriteLine("[LENGTH] > " + realLength);
            Console.WriteLine();
            try
            {
                double sum = Math.Pow(charset.Length, realLength);
                NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;
                BigInteger intSum = Convert.ToInt64(sum);
                Console.WriteLine("[Total combinations] > " + sum.ToString("N", nfi).Replace(".00", ""));
                Console.WriteLine("[Duration stat. bitrate  80 000/s] > {0:%d} days {0:%h} hours {0:%m} minutes {0:%s} seconds", StatDuration(80000, intSum));
                Console.WriteLine("[Duration stat. bitrate 150 000/s] > {0:%d} days {0:%h} hours {0:%m} minutes {0:%s} seconds", StatDuration(150000, intSum));
                Console.WriteLine("[Duration stat. bitrate 200 000/s] > {0:%d} days {0:%h} hours {0:%m} minutes {0:%s} seconds", StatDuration(200000, intSum));
                Console.WriteLine();
                Console.WriteLine("####################################");
                Console.WriteLine("\r\n[*] Started on " + DateTime.Now.ToString("dddd , dd MMM yyyy, HH:mm:ss"));
                Console.WriteLine("\r\n[*] Signature algorithm detected : " + algoDisplay);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[!] Failed condition... : " + ex.Message);
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(0);
            }
            
            try
            {
                var jwt = rawToken.Split('.');
                var phash = jwt[0] + "." + jwt[1];
                var hash = Base64UrlDcode(jwt[2]);
                if (live)
                {
                    SetTimer();
                }
                else
                {
                    Console.WriteLine("[*] Enumeration in progress...");
                }
                pile = new List<int>();
                Enumerate(realLength, charset, phash, hash);
                if (!found)
                {
                    Console.WriteLine("[!] Done, not found...");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[!] Error - invalid token : "+ ex.Message);
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(0);
            }
            Console.WriteLine("[*] Ended on " + DateTime.Now.ToString("dddd , dd MMM yyyy, HH:mm:ss"));
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void Enumerate(int length, string charset, string phash, byte[] hash)
        {
            var q = charset.Select(x => x.ToString());
            for (int i = 0; i < length - 1; i++)
            {
                q = q.SelectMany(x => charset, (x, y) => x + y);
            }           
            // Multithread
            Parallel.ForEach(q, (item) =>
               Compute(item, phash, hash)
            );           
        }

        private static void Compute(string item, string phash, byte[] hash)
        {
            HMAC hm;
            byte[] bytes = new byte[1];
            switch (algo)
            {
                case 1:
                    hm = new HMACSHA1
                    {
                        Key = Encoding.UTF8.GetBytes(item)
                    };
                    bytes = hm.ComputeHash(Encoding.UTF8.GetBytes(phash));
                    break;
                case 2:
                    hm = new HMACSHA256
                    {
                        Key = Encoding.UTF8.GetBytes(item)
                    };
                    bytes = hm.ComputeHash(Encoding.UTF8.GetBytes(phash));
                    break;
                case 3:
                    hm = new HMACSHA384
                    {
                        Key = Encoding.UTF8.GetBytes(item)
                    };
                    bytes = hm.ComputeHash(Encoding.UTF8.GetBytes(phash));
                    break;
                case 4:
                    hm = new HMACSHA512
                    {
                        Key = Encoding.UTF8.GetBytes(item)
                    };
                    bytes = hm.ComputeHash(Encoding.UTF8.GetBytes(phash));
                    break;               
            }           
            if (StructuralComparisons.StructuralEqualityComparer.Equals(bytes, hash))
            {
                if (live)
                {
                    timer1.Stop();
                }
                Console.WriteLine("");
                Console.WriteLine("\r\n[!] Done ! > Secret found: " + item + "\r\n");
                if (pile != null && pile.Count > 0)
                {
                    Console.WriteLine("[i] Average bitrate : " + Math.Round(pile.Average()) + "/s");
                }                
                found = true;
            }
            tick++;
            count++;
        }     

        #region Display
        private static void PrintBanner()
        {
            Console.WriteLine("####################################");
            Console.WriteLine("#              JWTrek         " + version + " #");
            Console.WriteLine("#       JWT Token Bruteforcer      #");
            Console.WriteLine("#    HS128  HS256  HS384  HS512    #");
            Console.WriteLine("#      by jo @ Georges Taupin      #");
            Console.WriteLine("#       C# .NET 4.5.2 - 2019       #");
            Console.WriteLine("# https://github.com/jo555/JWTrek  #");
            Console.WriteLine("####################################");
        }
        #endregion

        #region Utilities
        private static TimeSpan StatDuration(int bitrate, BigInteger total)
        {
            BigInteger seconds = total / bitrate;
            TimeSpan ts = TimeSpan.FromSeconds((double)seconds);
            return ts;
        }

        private static byte[] Base64UrlDcode(string s)
        {
            s = s.Replace('-', '+');
            s = s.Replace('_', '/');
            s += "==".Substring(0, s.Length % 3);
            return Convert.FromBase64String(s);
        }
        #endregion

        #region Monitoring
        private static void SetTimer()
        {
            timer1 = new System.Timers.Timer(1000);
            timer1.Elapsed += OnTimedEvent;
            timer1.AutoReset = true;
            timer1.Enabled = true;
        }

        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Console.Write("\r{0}", "[*] Enumeration in progress > [Bitrate : " + count + "/s] [Total tested :" + tick + "]");
            pile.Add(count);
            count = 0;
        }
        #endregion
    }
}
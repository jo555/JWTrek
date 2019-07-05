using System;
using System.Collections;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.Globalization;
using System.Numerics;

namespace JWTest
{
    class Program
    {
        public static Timer timer1;
        public static int count;
        public static int tick = 0;
        public static bool found = false;
        public static bool live = false;
        public static string version = "v1.1";
        public static string defaultCharset = "abcdefghijklmnopqrstuvwxyz0123456789";
        public static int defaultLength = 6;

        static void Main(string[] args)
        {
            printBanner();
            bool rawTokenOk = false;
            string rawToken = "";
            while (!rawTokenOk)
            {
                Console.WriteLine("\r\n[?] Please enter token");
                rawToken = Console.ReadLine();
                rawTokenOk = (rawToken != string.Empty) && (rawToken.Split('.').Length == 3);
            }           

            Console.WriteLine("\r\n[?] Please enter custom charset ["+ defaultCharset + "]");
            string charset = Console.ReadLine();
            if ((charset == null) || (charset == string.Empty))
            {
                charset = defaultCharset;
            }

            Console.WriteLine("\r\n[?] Please enter length ["+ defaultLength + "]");
            int realLength;
            if (!int.TryParse(Console.ReadLine(), out realLength))
            {
                realLength = defaultLength;
            }

            Console.WriteLine("\r\n[?] Live monitoring ? y/n");
            live = Console.ReadLine().Equals("y");     

            Console.Clear();
            printBanner();
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
                Console.WriteLine("[Duration stat. bitrate  80 000/s] > {0:%d} days {0:%h} hours {0:%m} minutes {0:%s} seconds", statDuration(80000, intSum));
                Console.WriteLine("[Duration stat. bitrate 150 000/s] > {0:%d} days {0:%h} hours {0:%m} minutes {0:%s} seconds", statDuration(150000, intSum));
                Console.WriteLine("[Duration stat. bitrate 200 000/s] > {0:%d} days {0:%h} hours {0:%m} minutes {0:%s} seconds", statDuration(200000, intSum));
                Console.WriteLine();
                Console.WriteLine("####################################");
                Console.WriteLine("\r\n[*] Started on " + DateTime.Now.ToString("dddd , dd MMM yyyy, HH:mm:ss"));
            }
            catch (Exception ex)
            {
                Console.WriteLine("[!] Failed condition... : " + ex.Message);
                Console.WriteLine("Press a key to continue...");
                Console.ReadKey();
                Environment.Exit(0);
            }
            
            try
            {
                var jwt = rawToken.Split('.');
                var phash = jwt[0] + "." + jwt[1];
                var hash = base64UrlDcode(jwt[2]);
                if (live)
                {
                    setTimer();
                }
                else
                {
                    Console.WriteLine("[*] Enumeration in progress...");
                }    
                enumerate(realLength, charset, phash, hash);
                if (!found)
                {
                    Console.WriteLine("[*] Ended on " + DateTime.Now.ToString("dddd , dd MMM yyyy, HH:mm:ss"));
                    Console.WriteLine("[!] Done, not found...");
                    Console.WriteLine("Press a key to continue...");
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[!] Error - invalid token : "+ ex.Message);
                Console.WriteLine("Press a key to continue...");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }

        private static void enumerate(int length, string charset, string phash, byte[] hash)
        {
            var q = charset.Select(x => x.ToString());
            for (int i = 0; i < length - 1; i++)
            {
                q = q.SelectMany(x => charset, (x, y) => x + y);
            }
            // OLD monothread code
            /*HMACSHA256 hm = new HMACSHA256();
            foreach (var item in q)
            {
                
                hm.Key = Encoding.UTF8.GetBytes(item);
                var h = hm.ComputeHash(Encoding.UTF8.GetBytes(phash));
                if (StructuralComparisons.StructuralEqualityComparer.Equals(h, hash))
                {
                    timer1.Stop();
                    Console.WriteLine("\r\n\r\n[!] Done ! > Secret was: " + item);
                    Console.WriteLine("\r\n[*] Ended on " + DateTime.Now.ToString("dddd , dd MMM yyyy, HH:mm:ss"));
                    break;
                }
                //Console.Write("\r Tested combinaisons : {0}", tick);
                tick++;
                count++;
                
            }*/
            // Multithread
            Parallel.ForEach(q, (item) =>
               compute(item, phash, hash)
            );           
        }

        private static void compute(string item, string phash, byte[] hash)
        {
            HMACSHA256 hm = new HMACSHA256();
            hm.Key = Encoding.UTF8.GetBytes(item);
            if (StructuralComparisons.StructuralEqualityComparer.Equals(hm.ComputeHash(Encoding.UTF8.GetBytes(phash)), hash))
            {
                if (live)
                {
                    timer1.Stop();
                }
                Console.WriteLine("\r\n[!] Done ! > Secret was: " + item);
                Console.WriteLine("\r\n[*] Ended on " + DateTime.Now.ToString("dddd , dd MMM yyyy, HH:mm:ss"));
            }
            tick++;
            count++;
        }     

        #region Display
        private static void printBanner()
        {
            Console.WriteLine("####################################");
            Console.WriteLine("#              JWTrek         " + version + " #");
            Console.WriteLine("#  JWT Token Bruteforcer  (HS256)  #");
            Console.WriteLine("#      by jo @ Georges Taupin      #");
            Console.WriteLine("#       C# .NET 4.5.2 - 2019       #");
            Console.WriteLine("# https://github.com/jo555/JWTrek  #");
            Console.WriteLine("####################################");
        }
        #endregion

        #region Utilities
        private static TimeSpan statDuration(int bitrate, BigInteger total)
        {
            BigInteger seconds = total / bitrate;
            TimeSpan ts = TimeSpan.FromSeconds((double)seconds);
            return ts;
        }

        private static byte[] base64UrlDcode(string s)
        {
            s = s.Replace('-', '+');
            s = s.Replace('_', '/');
            s = s + "==".Substring(0, s.Length % 3);
            return Convert.FromBase64String(s);
        }
        #endregion

        #region Monitoring
        private static void setTimer()
        {
            timer1 = new System.Timers.Timer(1000);
            timer1.Elapsed += OnTimedEvent;
            timer1.AutoReset = true;
            timer1.Enabled = true;
        }

        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Console.Write("\r{0}", "[*] Enumeration in progress > [Bitrate : " + count + "/s] [Total tested :" + tick + "]");
            count = 0;
        }
        #endregion
    }
}

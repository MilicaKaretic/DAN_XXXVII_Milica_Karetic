using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DAN_XXXVII_Milica_Karetic
{
    class Program
    {
        private static object locker = new object();
        static Random rnd = new Random();
        public static List<int> list = new List<int>(1000);
        public static string fileName = "FileRoutes.txt";
        public static List<int> routes = new List<int>(10);

        public static void GenerateNumbers()
        {
            int num;

            lock (fileName)
            {
                using (StreamWriter sw = File.CreateText(fileName))
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        num = rnd.Next(1, 5001);
                        list.Add(num);
                        sw.WriteLine(num);
                    }
                }
                Monitor.Pulse(fileName);
            }
        }

        public static void PickRoutes()
        {
            List<int> tempList = new List<int>();
            lock (fileName)
            {
                Monitor.Wait(fileName, 3000);
                using (StreamReader sr = File.OpenText(fileName))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if(int.Parse(line) % 3 == 0)
                            tempList.Add(int.Parse(line));
                    }
                }

                tempList.Sort();
                for (int i = 0; i < 10; i++)
                {
                    routes.Add(tempList[i]);
                }
                Console.WriteLine("Routes picked. You can start loading. After loading you can go.");
                Console.WriteLine("Routes:");
                for (int i = 0; i < routes.Count; i++)
                {
                    Console.Write(routes[i] + " ");
                }
            }

        }

        static void Main(string[] args)
        {
            Thread t1 = new Thread(GenerateNumbers)
            {
                Name = "GenerateNumbers"
            };
            Thread t2 = new Thread(PickRoutes)
            {
                Name = "PickRoutes"
            };

            //start 1. i 2. threads
            t1.Start();
            t2.Start();

            t1.Join();
            t2.Join();

            Console.ReadKey();
        }
    }
}

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
        public static List<Thread> trucks = new List<Thread>();

        public static Semaphore semaphore = new Semaphore(2, 2);

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
                IEnumerable<int> distinctRoutes = tempList.Distinct();
                int a = 0;
                foreach (int route in distinctRoutes)
                {
                    if (a < 10)
                    {
                        routes.Add(route);
                        a++;
                    }
                    else
                        break;                   
                }

                Console.WriteLine("Routes picked. You can start loading. After loading you can go.");
                Console.WriteLine("Best routes:");
                for (int i = 0; i < routes.Count; i++)
                {
                    Console.Write(routes[i] + " ");
                }
                Console.WriteLine();
                Console.WriteLine();
            }

        }
        static int restartCount = 0, count2 = 0, count = 0;
        static int numAvailable = 2;
        
        public static void TwoTrucksLoading()
        {
            while (true)
            {
                lock (locker)
                {
                    count2++;
                    if (count2 > 2)
                        Thread.Sleep(0);
                    else
                    {
                        restartCount++;
                        break;
                    }
                }
            }
        }

        public static void Loading(object route)
        {

            TwoTrucksLoading();

            int loadingTime = rnd.Next(500, 5000);

            semaphore.WaitOne();
 
            Console.WriteLine(Thread.CurrentThread.Name + " is loading " + loadingTime + " ms.");
            Thread.Sleep(loadingTime);

            Console.WriteLine(Thread.CurrentThread.Name + " is loaded...");

            semaphore.Release();

            restartCount--;
            if(restartCount == 0)
            {
                count2 = 0;
            }


            lock (locker)
            {
                count++;
            }
            while(count < 10)
            {
                Thread.Sleep(0);
            }

            Console.WriteLine(Thread.CurrentThread.Name + " will drive on route " + route);

            Console.WriteLine(Thread.CurrentThread.Name + "'s on his way. You can expect delivery between 500 ms and 5 sec");

            int deliveryTime = rnd.Next(500, 5000);

            if(deliveryTime > 3000)
            {
                Thread.Sleep(3000);
                Console.WriteLine("Delivery canceled beacuse expected delivery time was " + deliveryTime + ". " + Thread.CurrentThread.Name + " returns to the starting point for 3000 ms.");
                Thread.Sleep(3000);
                Console.WriteLine(Thread.CurrentThread.Name + " returned to the starting point.");
            }
            else
            {
                Thread.Sleep(deliveryTime);
                Console.WriteLine(Thread.CurrentThread.Name + " arrived to the destination for " + deliveryTime + " ms.");

                int unloadingTime = Convert.ToInt32(loadingTime / 1.5);
                Console.WriteLine(Thread.CurrentThread.Name + " is unloading " + unloadingTime + " ms.");
                Thread.Sleep(unloadingTime);

                Console.WriteLine(Thread.CurrentThread.Name + " is unloaded...");
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

            for (int i = 0; i < 10; i++)
            {
                Thread t = new Thread(Loading)
                {
                    Name = string.Format("Truck_{0}", i + 1)
                };
                trucks.Add(t);
            }
            for (int i = 0; i < trucks.Count; i++)
            {
                trucks[i].Start(routes[i]);
            }
            for (int i = 0; i < trucks.Count; i++)
            {
                trucks[i].Join();
            }

            Console.ReadKey();
        }
    }
}

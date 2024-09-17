using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace ParallelImageProcessor {
    internal class Program {
        static void Main(string[] args) {
            List<Thread> threads = new List<Thread>();
            bool run = true;
            int imagesCount = 0;
            using(ImageProcessor imageProcessor = new ImageProcessor(5)) {
                for(int i = 0; i < 20; i++) {
                    Thread thread = new Thread(() => {
                        while(run) {
                            using(Bitmap bmp = new Bitmap(10, 10))
                                imageProcessor.ProcessImage(bmp);
                            Interlocked.Increment(ref imagesCount);
                        }
                    });
                    threads.Add(thread);
                    thread.Start();
                }

                Thread.Sleep(5000);
                run = false;
                foreach(var thr in threads)
                    thr.Join();

                Console.WriteLine($"{imagesCount} images processed");
            }
            Console.WriteLine("Press any key");
            Console.ReadKey();
        }
    }
}

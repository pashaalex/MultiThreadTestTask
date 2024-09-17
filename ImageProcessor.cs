using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace ParallelImageProcessor {
    public class ImageProcessor : IDisposable {
        private const int GPUThreshold = 10;
        private readonly Queue<JobItem> imagesQueue = new Queue<JobItem>();
        private readonly object queueLocker = new object();
        private readonly List<CPUProcessor> cpuProcessors;
        private readonly GPUProcessor gpuProcessor;
        /// <summary>
        /// Отдельный сигнал для CPU
        /// </summary>
        private readonly ManualResetEvent cpuSignalEvent = new ManualResetEvent(false);
        /// <summary>
        /// Отдельный сигнал для GPU
        /// </summary>
        private readonly ManualResetEvent gpuSignalEvent = new ManualResetEvent(false);

        public ImageProcessor(int maxCPUProcessors) {
            cpuProcessors = new List<CPUProcessor>(maxCPUProcessors);
            for(int i = 0; i < maxCPUProcessors; i++)
                cpuProcessors.Add(new CPUProcessor(imagesQueue, cpuSignalEvent, queueLocker));

            gpuProcessor = new GPUProcessor(imagesQueue, gpuSignalEvent, queueLocker, GPUThreshold);
        }

        /// <summary>
        /// Публичный метод для обработки изображений
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public List<byte[]> ProcessImage(Bitmap image) {
            using(var request = new JobItem(image)) {
                lock(queueLocker) {
                    imagesQueue.Enqueue(request);
                    if(imagesQueue.Count >= GPUThreshold)
                        gpuSignalEvent.Set(); // Если пакет достаточно большой - сообщаем GPU-обработчику
                }
                cpuSignalEvent.Set(); // В любом случае сообщаем обработчикам на процессоре, что что-то появилось в очереди
                request.autoResetEvent.WaitOne(); // Ждём пока нашу задачу обработают
                return request.result;
            }
        }

        public void Dispose() {
            foreach(var t in cpuProcessors)
                t.Dispose();
            gpuProcessor.Dispose();
            cpuSignalEvent.Dispose();
            gpuSignalEvent.Dispose();
        }
    }
}
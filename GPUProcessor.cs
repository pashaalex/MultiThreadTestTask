using System.Collections.Generic;
using System.Threading;

namespace ParallelImageProcessor {
    public class GPUProcessor : BaseProcessor {

        public GPUProcessor(Queue<JobItem> imagesQueue, ManualResetEvent manualResetEvent, object queueLocker, int GPUThreshold) : base(imagesQueue, manualResetEvent, queueLocker) {
            this.batchSize = GPUThreshold;
        }

        /// <summary>
        /// Заглушка метода
        /// </summary>
        /// <param name="batch"></param>
        protected override void ProcessBatch(List<JobItem> batch) {
            Thread.Sleep(100);
            foreach(JobItem item in batch)
                item.autoResetEvent.Set();
        }
    }
}

using System.Collections.Generic;
using System.Threading;

namespace ParallelImageProcessor {
    public class CPUProcessor : BaseProcessor {
        public CPUProcessor(Queue<JobItem> imagesQueue, ManualResetEvent manualResetEvent, object queueLocker) : base(imagesQueue, manualResetEvent, queueLocker) {
            this.batchSize = 1;
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

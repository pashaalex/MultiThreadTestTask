using System;
using System.Collections.Generic;
using System.Threading;

namespace ParallelImageProcessor {
    public abstract class BaseProcessor : IDisposable {
        /// <summary>
        /// Сигнал, что пора остановиться
        /// </summary>
        ManualResetEvent needStop = new ManualResetEvent(false);

        /// <summary>
        /// Ссылка на очередь JobItem-ов, которые обрабатываем
        /// </summary>
        Queue<JobItem> imagesQueue;
        object queueLocker;

        /// <summary>
        /// Этот эвент не диспозим!!! Т.к. это ссылка на эвент в самом глобальном обработчике
        /// </summary>
        ManualResetEvent queueEvent;

        /// <summary>
        /// Время, на которое мы засыпаем после неудачной попытки 
        /// Просыпаемся только если сработает queueEvent или needStop или выйдет таймаут
        /// </summary>
        TimeSpan sleepAfterFailTime = TimeSpan.FromSeconds(5);
        Thread thread;

        /// <summary>
        /// Кол-во изображений, которые мы обрабатываем за одну транзакцию
        /// </summary>
        protected int batchSize = 1;

        public BaseProcessor(Queue<JobItem> imagesQueue, ManualResetEvent manualResetEvent, object queueLocker) {
            this.imagesQueue = imagesQueue;
            this.queueEvent = manualResetEvent;
            this.queueLocker = queueLocker;

            thread = new Thread(() => ThreadProc());
            thread.Start();
        }

        protected virtual void ThreadProc() {
            while(!needStop.WaitOne(1)) {
                if(TryGetImages(out List<JobItem> batch, batchSize)) {
                    ProcessBatch(batch);
                }
                else {
                    // Ожидаем, пока в очереди что-то появится или пока не сработает событие "надо завершаться"
                    WaitHandle.WaitAny(new WaitHandle[] { queueEvent, needStop }, sleepAfterFailTime);
                }
            }
        }

        protected abstract void ProcessBatch(List<JobItem> batch);

        public void Dispose() {
            needStop.Set();
            if(!thread.Join(3000))
                thread.Abort();
            needStop.Dispose();
        }

        protected virtual bool TryGetImages(out List<JobItem> result, int count) {
            result = new List<JobItem>(count);
            lock(this.queueLocker) {
                if(this.imagesQueue.Count >= count) {
                    for(int i = 0; i < count; i++)
                        result.Add(this.imagesQueue.Dequeue());

                    return true;
                }
                // Сообщаем, что попытка не удалась
                queueEvent.Reset();
            }
            return false;
        }
    }
}

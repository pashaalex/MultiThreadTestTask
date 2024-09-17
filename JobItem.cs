using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace ParallelImageProcessor {
    public class JobItem : IDisposable {
        public Bitmap Image { get; }
        public AutoResetEvent autoResetEvent = new AutoResetEvent(false);
        public List<byte[]> result = null;

        public JobItem(Bitmap image) {
            Image = image;
        }

        public void Dispose() {
            autoResetEvent.Dispose();
        }
    }
}

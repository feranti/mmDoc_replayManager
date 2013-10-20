using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Replays;
using System.Diagnostics;

namespace MMDoCHistoryV2
{
    public sealed class AutoUpdate
    {
        public AutoUpdate(Loader loader)
        {
            this.Loader = loader;
        }

        public readonly Loader Loader;

        private readonly Stopwatch timer = new Stopwatch();

        public int Interval = 10000;

        private long lastMs = -1;

        public bool IsRunning
        {
            get
            {
                return timer.IsRunning;
            }
        }

        public void Start()
        {
            if(timer.IsRunning)
                return;

            timer.Start();
        }

        public void Stop()
        {
            timer.Stop();
        }

        public void Update()
        {
            if(!timer.IsRunning)
                return;

            // This is done like this because interval may change while updating.
            if(lastMs < 0 || timer.ElapsedMilliseconds - lastMs > this.Interval)
            {
                if(this.Loader.IsLoading)
                {
                    // This is correct! we have cooldown that starts after loading is completed.
                    lastMs = timer.ElapsedMilliseconds;
                    return;
                }

                // These parameters must be set if we want to update.
                this.Loader.LoadMissingOnly = true;
                this.Loader.ReloadUnfinishedReplays = true;

                // Start loading again.
                this.Loader.StartLoad(false);

                // Set this anyway now in case we don't catch the loader loading.
                lastMs = timer.ElapsedMilliseconds;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using Verse;

namespace Materia
{
    public class Ticker : IExposable
    {
        public int Ticks = 100;

        protected Action _OnTick;
        public int CurrentTick = 0;
        public bool IsRunning { private set; get; }
        public bool IsFinished { private set; get; } = false;
        public int CurrentRepeatCount { get; private set; } = 0;

        private int RepeatAmount = -1;

        public Ticker()
        {

        }

        public Ticker(int ticks, Action onTick, bool startAutomatically = true, int repeatCount = -1)
        {
            Ticks = ticks;
            RepeatAmount = repeatCount;
            _OnTick = onTick;
            CurrentTick = 0;
            if (startAutomatically) Start();
        }

        public virtual void Tick()
        {
            CurrentTick++;

            if (CurrentTick >= Ticks)
            {
                CurrentRepeatCount++;
                _OnTick?.Invoke();

                if (RepeatAmount > 0 && CurrentRepeatCount >= RepeatAmount)
                {
                    Stop();
                }
                else
                {
                    Reset();
                }
            }
        }

        public virtual void Reset()
        {
            CurrentTick = 0;
            IsRunning = true;
            IsFinished = false;
        }

        public virtual void Start()
        {
            IsRunning = true;
            IsFinished = false;
        }

        public virtual void Stop(bool reset = false)
        {
            IsRunning = false;
            IsFinished = true;
            if (reset) Reset();
        }

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref CurrentTick, "timerCurrentTick", 0);
        }
    }
}

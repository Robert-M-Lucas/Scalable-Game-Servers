namespace Shared;

using System.Diagnostics;

public class Timer{
    Stopwatch s = new Stopwatch();
    public Timer(){
        s.Start();
    }

    public long GetMsAndReset(){
        long ms = s.ElapsedMilliseconds;
        s.Restart();
        return ms;
    }

    public long GetMs() { return s.ElapsedMilliseconds; }

    public void Reset(){ s.Reset(); }
    public void Restart() {s.Restart(); }
    public void Start() { s.Start(); }
}
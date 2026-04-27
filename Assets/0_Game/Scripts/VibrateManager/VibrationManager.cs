using System;
using DG.Tweening;

public static class VibrationManager
{
    public static bool isVibrateOn = true;

    public static void Vibrate(float second, float delay = 0)
    {
        if(!isVibrateOn) return;
        if (delay > 0)
        {
            DOVirtual.DelayedCall(delay,()=> Vibration.Vibrate(TimeSpan.FromSeconds(second).Milliseconds));
            return;
        }
        Vibration.Vibrate(TimeSpan.FromSeconds(second).Milliseconds);
    }

    public static void StopVibrate()
    {
        Vibration.Cancel();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace BackgroundLocationSample.Droid
{
    /// <summary>
    /// Receive notification that the device has booted
    /// </summary>
    /// <remarks>
    /// See https://developer.android.com/training/articles/direct-boot for details  on direct boot
    /// </remarks>
    [BroadcastReceiver(Enabled=true,Exported = true,DirectBootAware = false)]
    [IntentFilter(new string[]{Intent.ActionBootCompleted,Intent.ActionLockedBootCompleted })]
    public class BootCompletedIntentReceiver:BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            BootService.Enqueue(context, intent);
        }
    }
}
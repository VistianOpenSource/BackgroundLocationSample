using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

namespace BackgroundLocationSample.Droid
{
    /// <summary>
    /// The device has been booted, lets look to start the background service
    /// </summary>
    [Service(Permission = "android.permission.BIND_JOB_SERVICE")]
    public class BootService:JobIntentService
    {
        public const int JobId = 1;
        protected override void OnHandleWork(Intent intent)
        {
            // setup the application environment
            Environment.Current.Setup(this);

            // and boot the background service
            BackgroundService.Start(this);
        }

        public static void Enqueue(Context context,Intent intent)
        {
            JobIntentService.EnqueueWork(context, Java.Lang.Class.FromType(typeof(BootService)), JobId, new Intent());
        }
    }
}
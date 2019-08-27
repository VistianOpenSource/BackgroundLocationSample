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
    public class HandleNotifications
    {
        //private static int Stop

        private static int StopActionIcon = Resource.Drawable.ic_mtrl_chip_checked_black;
        private static int SmallIcon = Resource.Drawable.abc_ic_star_black_16dp;

        private static int NotificationId = NotificationCode;

        public enum Command
        {
            Stop = 0,
            Start = 1,
            Invalid = -1
        };

        public const int NotificationCode = 1110;
        public const int StopServiceRequestCode = 1111;
        public const int LaunchRequestCode = 1112;

        private PendingIntent GetStopService(Service context)
        {
            var iStopService = new MyIntentBuilder(context).SetCommand((int)Command.Stop).Build();

            var piStopService = PendingIntent.GetService(context, GetRandomNumber(), iStopService,0);

            return piStopService;
        }

        private int GetRandomNumber()
        {
            var rnd = new Random();

            return rnd.Next(100000);
        }

        private PendingIntent GetLaunchActivity(Service context)
        {
            var iLaunchMainActivity = new Intent(context,typeof(MainActivity));

            var piLaunchMainActivity = PendingIntent.GetActivity(context, LaunchRequestCode, iLaunchMainActivity, 0);

            return piLaunchMainActivity;
        }

        public void CreateNotification(Service context)
        {
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                var channelId = CreateChannel(context);

                Notification notification = BuildNotification(context, channelId);

                context.StartForeground(NotificationId, notification);
            }
            else
            {
                var notification = BuildNotificationPreO(context);

                context.StartForeground(NotificationId,notification);
            }
        }


        
        public Notification BuildNotificationPreO(Service context)
        {
            var piLaunchMainActivity = GetLaunchActivity(context);
            var stopService = GetStopService(context);

           
            // Action to stop the service
            NotificationCompat.Action stopAction = new NotificationCompat.Action.Builder(StopActionIcon,GetStopNotificationText,stopService).Build();

            if (Android.OS.Build.VERSION.SdkInt <= Android.OS.BuildVersionCodes.O)
            {
                // create notification
                var notification = new NotificationCompat.Builder(context).SetContentTitle(GetNotificationTitle)
                    .SetContentText(GetNotificationContext).SetSmallIcon(SmallIcon)
                    .SetContentIntent(piLaunchMainActivity).AddAction(stopAction)
                    .SetStyle(new NotificationCompat.BigTextStyle()).Build();

                return notification;
            }

            return null;
        }
        public Notification BuildNotification(Service context,string channelId)
        {
            var piLaunchMainActivity = GetLaunchActivity(context);
            var stopService = GetStopService(context);

            // Action to stop the service
            var stopAction = new NotificationCompat.Action.Builder(StopActionIcon,GetStopNotificationText,stopService).Build();

            // create notification
            var notification = new NotificationCompat.Builder(context,channelId).
                SetContentTitle(GetNotificationTitle).
                SetContentText(GetNotificationContext).
                SetSmallIcon(SmallIcon).
                SetContentIntent(piLaunchMainActivity).
                AddAction(stopAction).
                SetStyle(new NotificationCompat.BigTextStyle()).Build();

            return notification;
        }

        private static int ChannelId = 2705;
        
        private string CreateChannel(Service context)
        {
            NotificationManager notificationManager = NotificationManager.FromContext(context);//.GetSystemService(Context.NotificationService));

            var importance = Android.App.NotificationImportance.Default;
            var channelName = "Playback channel";

            NotificationChannel notificationChannel = new NotificationChannel(ChannelId.ToString(),channelName,importance);

            notificationManager.CreateNotificationChannel(notificationChannel);

            return ChannelId.ToString();

        }

        private string GetStopNotificationText => "Stop";

        private string GetNotificationTitle => "TITLE";

        private string GetNotificationContext => "Content";
    }
}
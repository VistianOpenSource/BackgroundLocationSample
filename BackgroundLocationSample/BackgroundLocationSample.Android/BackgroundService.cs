using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace BackgroundLocationSample.Droid
{
    [Service]
    public class BackgroundService:Service
    {
        public static bool IsRunning = false;
        private IDisposable _sensorDisposable;
        
        public static void Start(Context context)
        {
            if (!IsRunning)
            {
                using (var serviceIntent = new Intent(context, typeof(BackgroundService)))
                {
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    {
                        context.StartForegroundService(serviceIntent);
                    }
                    else
                    {
                        context.StartService(serviceIntent);
                    }
                }
            }
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            if (MyIntentBuilder.ContainsCommand(intent))
            {
                var command = MyIntentBuilder.GetCommand(intent);

                if (command == (int)HandleNotifications.Command.Stop)
                {
                    ShutdownService();
                    return StartCommandResult.NotSticky;
                }
            }

            IsRunning = true;

            var hn = new HandleNotifications();

            hn.CreateNotification(this);

            PermissionsContainer.RequestPermissions(this, new[] {Manifest.Permission.AccessFineLocation}).
                ToObservable()
                .Do((p) =>
                {
                    var locationPermissionState = p.FirstOrDefault(per => per.Name == Manifest.Permission.AccessFineLocation);

                    // if we have the relevant permissions, then setup the sensor monitoring
                    if (locationPermissionState != null && locationPermissionState.Granted)
                    {
                        SetupSensorMonitoring();
                    }
                    else
                    {
                        // inform the application root that we have missing permissions
                        ApplicationRoot.MissingPermissions(p.Where(permission => !permission.Granted).Select(per => per.Name));
                    }
                }).
                Subscribe();

            return StartCommandResult.Sticky;


        }

        private void SetupSensorMonitoring()
        {
            var sensorMonitoring = new SensorMonitoring();

            _sensorDisposable = sensorMonitoring.Setup((e) =>
            {
               
                Log.Info("BackgroundLocationSample",$"Location Event Received {e.Date}:{e.Location.Longitude},{e.Location.Latitude}");
            }
          );
        }

        private void ShutdownService()
        {
            _sensorDisposable?.Dispose();
            IsRunning = false;
            StopForeground(true);
            StopSelf();
        }


        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
    }
}
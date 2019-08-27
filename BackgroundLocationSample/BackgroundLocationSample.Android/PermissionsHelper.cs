using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using TaskStackBuilder = Android.App.TaskStackBuilder;

namespace BackgroundLocationSample.Droid
{
    public sealed class PermissionState
    {
        public string Name { get; }

        public bool Granted { get; }

        public PermissionState(string name, bool granted)
        {
            Name = name;
            Granted = granted;
        }
    }

    public class PermissionsHelper
    {
        public const string KeyPermissions = "Permissions";
        public const string KeyGrants = "Grants";
        public const string KeyRequestCode = "RequestCode";
        public const string KeyResultsReceiver = "ResultsReceiver";

        public async Task<List<PermissionState>> CheckAndRequestPermissionAsync(Context context, string[] permissions)
        {
            // get list of those permissions already granted
            var grantedPermissions = permissions.Where(p => context.CheckCallingPermission(p) == Permission.Granted);

            // get those which are currently denied
            var requestRequiredPermissions =permissions.Where(p => context.CheckCallingPermission(p) == Permission.Denied).ToArray();

            if (requestRequiredPermissions.Length > 0)
            {
                var requestedPermissionsResult = await RequestPermissions(context, requestRequiredPermissions,1234);

                requestedPermissionsResult.AddRange(grantedPermissions.Select(gp => new PermissionState(gp, true)));

                return requestedPermissionsResult;
            }
            else
            {
                return grantedPermissions.Select(gp => new PermissionState(gp, true)).ToList();
            }
        }


        public Task<List<PermissionState>> RequestPermissions(Context context,string[] permissions,int requestCode)
        {
            var tcs = new TaskCompletionSource<List<PermissionState>>();

            var rc = new MyResultReceiver(tcs,new Handler(Looper.MainLooper));


            var permissionIntent = new Intent(context,typeof(PermissionsRequestActivity));

            permissionIntent.PutExtra(PermissionsHelper.KeyResultsReceiver, rc);
            permissionIntent.PutExtra(PermissionsHelper.KeyPermissions, permissions);
            permissionIntent.PutExtra(PermissionsHelper.KeyRequestCode, requestCode);

            CreateChannel(context, "2");

            var stackBuilder=TaskStackBuilder.Create(context);
            stackBuilder.AddNextIntent(permissionIntent);

            var permPendingIntent = stackBuilder.GetPendingIntent(0, PendingIntentFlags.UpdateCurrent);

            var builder = new NotificationCompat.Builder(context,"2");

            builder = builder.SetSmallIcon(Resource.Drawable.abc_ic_search_api_material)
                .SetContentText("this is the test for the permissions notification")
                .SetContentTitle("this is the title")
                .SetOngoing(true)
                .SetAutoCancel(true)
                .SetWhen(0)
                .SetContentIntent(permPendingIntent)
                .SetStyle(null);

            var notificationManager = NotificationManager.FromContext(context);

            notificationManager.Notify(requestCode,builder.Build());

            return tcs.Task;
        }

        private string CreateChannel(Context context,string channelId)
        {
            var notificationManager = NotificationManager.FromContext(context);

            var importance = Android.App.NotificationImportance.Default;
            var channelName = "Permissions channel";

            var notificationChannel = new NotificationChannel(channelId,channelName,importance);

            notificationManager.CreateNotificationChannel(notificationChannel);

            return channelId;
        }
    }


    public class MyResultReceiver : ResultReceiver
    {
        private readonly TaskCompletionSource<List<PermissionState>> _tcs;

        public MyResultReceiver(TaskCompletionSource<List<PermissionState>> tcs,Handler handler) : base(handler)
        {
            _tcs = tcs;
        }

        protected override void OnReceiveResult(int resultCode, Bundle resultData)
        {
            base.OnReceiveResult(resultCode, resultData);

            var permissions = resultData.GetStringArray(PermissionsHelper.KeyPermissions);
            var grantResults = resultData.GetIntArray(PermissionsHelper.KeyGrants);

            var permissionState = permissions.Select((t, i) => new PermissionState(t, grantResults[i] == (int) Permission.Granted)).ToList();

            _tcs.SetResult(permissionState);
        }
    }



    [Activity(Theme = "@style/MainTheme")]
    public class PermissionsRequestActivity : AppCompatActivity
    {
        private ResultReceiver _resultReceiver;
        private int _requestCode;

        protected override void OnStart()
        {
            base.OnStart();

            _resultReceiver = (ResultReceiver)this.Intent.GetParcelableExtra(PermissionsHelper.KeyResultsReceiver);

            var permissions = this.Intent.GetStringArrayExtra(PermissionsHelper.KeyPermissions);
            _requestCode = this.Intent.GetIntExtra(PermissionsHelper.KeyRequestCode, 0);

            ActivityCompat.RequestPermissions(this,permissions,_requestCode);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            var resultData = new Bundle();

            resultData.PutStringArray(PermissionsHelper.KeyPermissions,permissions);
            resultData.PutIntArray(PermissionsHelper.KeyGrants,grantResults.Select(gr => (int)gr).ToArray());
            _resultReceiver.Send((Result)_requestCode, resultData);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            Finish();
        }

    }

    public static class PermissionsContainer
    {
        public static AppCompatActivity ActiveActivity = null;

        // All permissions are requested through static APIs
        // Depending upon whether there is an active activity or not will dictate whether
        // a fallback approach using a dedicated activity is used or not.
        // if no current active activity then a notifications route with the dedicated activity is used


        private const int ActivityRequestCode = 1234;
        private const int NotificationRequestCode = 1234;
        public static async Task<List<PermissionState>> RequestPermissions(Context context, string[] permissions)
        {
            var grantedPermissions = permissions.Where(p => ContextCompat.CheckSelfPermission(context,p) == Permission.Granted).ToList();

            // get those which are currently denied
            var requestRequiredPermissions =permissions.Where(p => ContextCompat.CheckSelfPermission(context,p) == Permission.Denied).ToArray();

            if (requestRequiredPermissions.Length > 0)
            {
                List<PermissionState> requestedPermissionsResult = null;


                if (ActiveActivity != null)
                {
                    requestedPermissionsResult = await RequestPermissionsFromActivity(permissions, ActivityRequestCode);
                }
                else
                {
                    requestedPermissionsResult =
                        await RequestPermissionsViaNotification(context, permissions, NotificationRequestCode);
                }

                requestedPermissionsResult.AddRange(grantedPermissions.Select(gp => new PermissionState(gp, true)));
                return requestedPermissionsResult;
            }
            else
            {
                return grantedPermissions.Select(p => new PermissionState(p, true)).ToList();
            }


        }

        private static readonly Dictionary<Context,TaskCompletionSource<List<PermissionState>>> _requestResultTasks = new Dictionary<Context, TaskCompletionSource<List<PermissionState>>>();

        public static Task<List<PermissionState>> RequestPermissionsFromActivity(string[] permissions, int requestCode)
        {
            var tcs = new TaskCompletionSource<List<PermissionState>>();

            _requestResultTasks[ActiveActivity] = tcs;

            ActivityCompat.RequestPermissions(ActiveActivity, permissions, requestCode);

            // this is completed by the handle code 
            return tcs.Task;
        }

        public static void HandleRequestPermissionsResult(AppCompatActivity activity,int requestCode, string[] permissions,Permission[] grantResults)
        {
            var permissionState = permissions.Select((t, i) => new PermissionState(t, grantResults[i] == (int) Permission.Granted)).ToList();

            var tcs = _requestResultTasks[activity];

            _requestResultTasks.Remove(activity);

            tcs.SetResult(permissionState);
        }

        public static Task<List<PermissionState>> RequestPermissionsViaNotification(Context context,string[] permissions,int requestCode)
        {
            var tcs = new TaskCompletionSource<List<PermissionState>>();

            var rc = new MyResultReceiver(tcs,new Handler(Looper.MainLooper));


            var permissionIntent = new Intent(context,typeof(PermissionsRequestActivity));

            permissionIntent.PutExtra(PermissionsHelper.KeyResultsReceiver, rc);
            permissionIntent.PutExtra(PermissionsHelper.KeyPermissions, permissions);
            permissionIntent.PutExtra(PermissionsHelper.KeyRequestCode, requestCode);

            CreateChannel(context, "2");

            var stackBuilder=TaskStackBuilder.Create(context);
            stackBuilder.AddNextIntent(permissionIntent);

            var permPendingIntent = stackBuilder.GetPendingIntent(0, PendingIntentFlags.UpdateCurrent);

            var builder = new NotificationCompat.Builder(context,"2");

            builder = builder.SetSmallIcon(Resource.Drawable.abc_ic_search_api_material)
                .SetContentText("this is the test for the permissions notification")
                .SetContentTitle("this is the title")
                .SetOngoing(true)
                .SetAutoCancel(true)
                .SetWhen(0)
                .SetContentIntent(permPendingIntent)
                .SetStyle(null);

            var notificationManager = NotificationManager.FromContext(context);

            notificationManager.Notify(requestCode,builder.Build());

            return tcs.Task;
        }

        /// <summary>
        /// All permissions are requested through static app
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public static bool IsPermissionIntent(this AppCompatActivity activity)
        {
            return activity.Intent.HasExtra(PermissionsHelper.KeyPermissions);
        }

        public static void RaisePermissionsIfRequested(AppCompatActivity activity)
        {
            var resultReceiver = (ResultReceiver) activity.Intent.GetParcelableExtra(PermissionsHelper.KeyResultsReceiver);

            var permissions = activity.Intent.GetStringArrayExtra(PermissionsHelper.KeyPermissions);
            var requestCode = activity.Intent.GetIntExtra(PermissionsHelper.KeyRequestCode, 0);


            ActivityCompat.RequestPermissions(activity, permissions, requestCode);
        }
        private static string CreateChannel(Context context,string channelId)
        {
            var notificationManager = NotificationManager.FromContext(context);

            var importance = Android.App.NotificationImportance.Default;
            var channelName = "Permissions channel";

            var notificationChannel = new NotificationChannel(channelId,channelName,importance);

            notificationManager.CreateNotificationChannel(notificationChannel);

            return channelId;

        }

    }
}
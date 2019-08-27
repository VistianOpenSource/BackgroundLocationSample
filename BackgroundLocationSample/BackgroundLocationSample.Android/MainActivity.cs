using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace BackgroundLocationSample.Droid
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// LaunchMode of SingleTask due to wanting the notification to just bring to the front an existing instance
    /// see https://stackoverflow.com/questions/19039189/intent-if-activity-is-running-bring-it-to-front-else-start-a-new-one-from-n/36150120
    /// </remarks>
    [Activity(Label = "BackgroundLocationSample", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,LaunchMode = LaunchMode.SingleTask)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            PermissionsContainer.ActiveActivity = this;

            // setup the application environment
            Environment.Current.Setup(this);

            // start our main background service which is used to monitoring of sensors etc...
            BackgroundService.Start(BaseContext);

            global::Xamarin.Forms.Forms.SetFlags("Shell_Experimental", "Visual_Experimental", "CollectionView_Experimental", "FastRenderers_Experimental");
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }
    }
}
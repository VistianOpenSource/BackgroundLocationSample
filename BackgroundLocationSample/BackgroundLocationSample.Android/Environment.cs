using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Essentials;

namespace BackgroundLocationSample.Droid
{
    public class Environment
    {
        private static readonly Lazy<Environment> _environment = new Lazy<Environment>(() => new Environment());
        public static Environment Current => _environment.Value;

        private bool _isSetup = false;
        public void Setup(Context context)
        {
            if (_isSetup) return;

            try
            {
                SetupGeo(context);
                SetupBoot(context);

                _isSetup = true;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private void SetupGeo(Context context)
        {
            var lp = new LocationProvider(context);
            ILocationProviderMixins.Provider = lp;
        }

        private void SetupBoot(Context context)
        {
            var bootServiceClass = Java.Lang.Class.FromType(typeof(BootService));

            var receiver = new ComponentName(context,bootServiceClass);
            var pm = context.PackageManager;

            pm.SetComponentEnabledSetting(receiver,ComponentEnabledState.Enabled,ComponentEnableOption.DontKillApp);
        }

    }
}
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
    public class MyIntentBuilder
    {
        private readonly Context _context;
        private string _message;
        private int? _command;

        private const string KeyMessage = "msg";
        private const string KeyCommand = "cmd";

        public MyIntentBuilder(Context context)
        {
            _context = context;
        }

        public MyIntentBuilder SetMessage(string message)
        {
            _message = message;
            return this;
        }

        public MyIntentBuilder SetCommand(int command)
        {
            _command = command;
            return this;
        }


        public static Boolean ContainsCommand(Intent intent)
        {
            return intent.Extras?.ContainsKey(KeyCommand)??false;
        }

        public static int GetCommand(Intent intent)
        {
            return intent.GetIntExtra(KeyCommand,-1);
        }


        public Intent Build()
        {
            var intent = new Intent(_context,typeof(BackgroundService));

            if (!string.IsNullOrEmpty(_message))
            {
                intent.PutExtra(KeyMessage, _message);
            }

            if (_command.HasValue)
            {
                intent.PutExtra(KeyCommand, _command.Value);
            }

            return intent;
        }
    }
}
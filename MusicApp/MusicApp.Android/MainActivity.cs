using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media.Session;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Support.V4.Content;
using MediaManager;
using MusicApp.Framework;
using MusicApp.ViewModel;


namespace MusicApp.Droid
{
    [Activity(Label = "MusicApp", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, ILoginRequester
    {
        public static Activity Self;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            Self = this;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;  
            
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            
            CrossMediaManager.Current.Init();
            LoadApplication(new App(this, new Logger()));
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }


        private static void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs unobservedTaskExceptionEventArgs)
        {
            var newExc = new Exception("TaskSchedulerOnUnobservedTaskException", unobservedTaskExceptionEventArgs.Exception);
            LogUnhandledException(newExc);
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            var newExc = new Exception("CurrentDomainOnUnhandledException", unhandledExceptionEventArgs.ExceptionObject as Exception);
            LogUnhandledException(newExc);
        }

        internal static void LogUnhandledException(Exception exception)
        {
            try
            {
                const string errorFileName = "YandexMusicKids_Fatal.log";
                var errorFilePath = Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads)?.AbsolutePath??"/data", errorFileName);
                var errorMessage = $"Time: {DateTime.Now}\r\nError: Unhandled Exception\r\n{exception}\r\n";
                using (var file = File.Open(errorFilePath, FileMode.Append))
                {
                    using (var text = new StreamWriter(file))
                    {
                        text.Write(errorMessage);
                    }
                }

                // Log to Android Device Logging.
                Android.Util.Log.Error("Crash Report", errorMessage);
            }
            catch
            {
                // just suppress any error logging exceptions
            }
        }


        private TaskCompletionSource<string> _loginTask;
        public Task<string> RequestLogin(LoginModel model)
        {
            if (_loginTask?.Task.IsCompleted==false) _loginTask.SetCanceled();
            
            var intent = new Intent(this, typeof(LoginActivity));
            intent.PutExtra(LoginActivity.EXTRA_LOGIN_INTENT,model.Login);
            _loginTask = new TaskCompletionSource<string>();
            
            StartActivityForResult(intent, 1);

            return _loginTask.Task;
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            
            var token = data.GetStringExtra(LoginActivity.EXTRA_TOKEN);
            if (token != null)
                _loginTask.SetResult(token);
            
            base.OnActivityResult(requestCode, resultCode, data);
        }

    }
}
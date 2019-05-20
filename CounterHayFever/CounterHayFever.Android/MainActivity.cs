using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Plugin.CurrentActivity;
using Plugin.Permissions;
using Android.App.Job;
using Plugin.LocalNotifications;
using Android.Support.V4.App;
using TaskStackBuilder = Android.Support.V4.App.TaskStackBuilder;
using Xamarin.Forms;
using CounterHayFever.Utils;

namespace CounterHayFever.Droid
{
    [Activity(Label = "Counter Hay-Fever", Icon = "@drawable/logo", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        #region Constants
        public static readonly int NOTIFICATION_ID = 1000;
        public static readonly string CHANNEL_ID = "CounterHayFeverNotification";

        static readonly int JOB_ID = 152;

        // Refresh data every 8 hours
        static readonly int REFRESH_INTERVAL_MILLISECONDS = 8 * 60 * 60 * 1000;

        // In case of failures, retry after 2 minutes.
        static readonly int RETRY_INTERVAL_MILLISECONDS = 2 * 60 * 1000;
        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            Xamarin.FormsMaps.Init(this, savedInstanceState);
            CrossCurrentActivity.Current.Init(this, savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            LoadApplication(new App());

            // Set Icon for notifications
            LocalNotificationsImplementation.NotificationIconId = Resource.Drawable.logo;
            CreateNotificationChannel();


            #region Initialize Scheduler for notifications

            MessagingCenter.Subscribe<NotificationMessage>(this, "setupNotification", message =>
            {
                var jobBuilder = this.CreateJobBuilderUsingJobId<DailyUpdateJob>(JOB_ID);

                // Set interval between refresh job being called.                
                jobBuilder.SetPeriodic(REFRESH_INTERVAL_MILLISECONDS);

                //Persists over phone restarts
                jobBuilder.SetPersisted(true);

                //If Fails re-try each 2 mins
                jobBuilder.SetBackoffCriteria(RETRY_INTERVAL_MILLISECONDS, BackoffPolicy.Linear);

                // Need a network connection to run job.
                jobBuilder.SetRequiredNetworkType(NetworkType.Any);
                var jobInfo = jobBuilder.Build();

                var jobScheduler = (JobScheduler)GetSystemService(JobSchedulerService);

                if (string.IsNullOrEmpty(Xamarin.Forms.Application.Current.Properties["WorkLocality"].ToString()) 
                    && string.IsNullOrEmpty(Xamarin.Forms.Application.Current.Properties["HomeLocality"].ToString()))
                {
                    jobScheduler.CancelAll();
                }
                else
                {
                    var scheduleResult = jobScheduler.Schedule(jobInfo);

                    if (JobScheduler.ResultSuccess == scheduleResult)
                    {
                        Console.WriteLine("Successfully created a job.");
                    }
                    else
                    {
                        Console.WriteLine("Could not create job.");
                    }
                }
            });
            #endregion
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification
                // channel on older versions of Android.
                return;
            }

            var name = Resources.GetString(Resource.String.channel_name);
            var description = GetString(Resource.String.channel_description);
            var channel = new NotificationChannel(CHANNEL_ID, name, NotificationImportance.Default)
            {
                Description = description
            };

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }
    }
}
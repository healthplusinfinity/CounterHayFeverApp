using System;
using System.Threading.Tasks;
using Android.App;
using Android.App.Job;
using Android.Content;
using Android.Support.V4.App;
using CounterHayFever.Models;
using Newtonsoft.Json;
using Android.Media;

namespace CounterHayFever.Droid
{
    /// <summary>
    /// Daily update job to fetch risk levels.
    /// </summary>
    [Service(Name = "com.healthplusinfinity.counterhayfever.DailyUpdateJob",
         Permission = "android.permission.BIND_JOB_SERVICE")]
    public class DailyUpdateJob : JobService
    {
        public override bool OnStartJob(JobParameters @params)
        {
            // Get data in an asynchronous task.
            Task.Run(async () =>
            {
                int notificationID = 1;
                if (Xamarin.Forms.Application.Current.Properties.ContainsKey("NotificationID"))
                {
                    notificationID = (int)Xamarin.Forms.Application.Current.Properties["NotificationID"] + 1;
                }

                string homeLocality = string.Empty;
                Task<UserRatingsModel> getHomeLocalityRisk = null;
                Task<UserRatingsModel> getWorkLocalityRisk = null;
                if (Xamarin.Forms.Application.Current.Properties.ContainsKey("HomeLocality")
                    && !string.IsNullOrEmpty(Xamarin.Forms.Application.Current.Properties["HomeLocality"].ToString()))
                {
                    homeLocality = Xamarin.Forms.Application.Current.Properties["HomeLocality"].ToString();
                    getHomeLocalityRisk = Data.GetRiskLevelsForSuburbAsync(homeLocality);
                }

                string workLocality = string.Empty;
                if (Xamarin.Forms.Application.Current.Properties.ContainsKey("WorkLocality")
                    && !string.IsNullOrEmpty(Xamarin.Forms.Application.Current.Properties["WorkLocality"].ToString()))
                {
                    workLocality = Xamarin.Forms.Application.Current.Properties["WorkLocality"].ToString();
                    getWorkLocalityRisk = Data.GetRiskLevelsForSuburbAsync(workLocality);
                }

                string notificationText = string.Empty;
                if (getHomeLocalityRisk != null)
                {
                    string homeRiskText = $"Risk level at {homeLocality}: ";
                    UserRatingsModel model = await getHomeLocalityRisk;
                    if (model != null)
                    {
                        if (model.AverageRating > 0)
                        {
                            if (model.AverageRating < 2)
                            {
                                homeRiskText += "Low";
                            }
                            else if (model.AverageRating < 4)
                            {
                                homeRiskText += "Medium";
                            }
                            else if (model.AverageRating <= 5)
                            {
                                homeRiskText += "High";
                            }
                        }
                        else
                        {
                            homeRiskText = $"No data available for {homeLocality}.";
                        }
                    }
                    else
                    {
                        homeRiskText = $"No data available for {homeLocality}.";
                    }
                    notificationText += homeRiskText + "\n";
                }

                if (getWorkLocalityRisk != null)
                {
                    string workLocalityRiskText = $"Risk level at {workLocality}: ";
                    UserRatingsModel model = await getWorkLocalityRisk;
                    if (model != null)
                    {
                        if (model.AverageRating > 0)
                        {
                            if (model.AverageRating < 2)
                            {
                                workLocalityRiskText += "Low";
                            }
                            else if (model.AverageRating < 4)
                            {
                                workLocalityRiskText += "Medium";
                            }
                            else if (model.AverageRating <= 5)
                            {
                                workLocalityRiskText += "High";
                            }
                        }
                        else
                        {
                            workLocalityRiskText = $"No data available for {workLocality}.";
                        }
                    }
                    else
                    {
                        workLocalityRiskText = $"No data available for {workLocality}.";
                    }
                    notificationText += workLocalityRiskText;
                }

                var builder = new NotificationCompat.Builder(Application.Context, MainActivity.CHANNEL_ID)
                    .SetSmallIcon(Resource.Drawable.logo)
                    .SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Ringtone))
                    .SetAutoCancel(true);

                // Instantiate the Inbox style:
                NotificationCompat.InboxStyle inboxStyle = new NotificationCompat.InboxStyle();

                // Set the title and text of the notification:
                builder.SetContentTitle("Current conditions");
                builder.SetContentText("The current conditions are:");

                // Generate a message summary for the body of the notification:
                string[] lines = notificationText.Split("\n");
                foreach (var line in lines)
                {
                    inboxStyle.AddLine(line);
                }

                // Plug this style into the builder:
                builder.SetStyle(inboxStyle);

                var notificationManager = NotificationManagerCompat.From(Application.Context);
                notificationManager.Notify(notificationID, builder.Build());

                Xamarin.Forms.Application.Current.Properties["NotificationID"] = notificationID;
                await Xamarin.Forms.Application.Current.SavePropertiesAsync();

                //Intent intent = new Intent(Application.Context, typeof(DataReceiver));
                //intent.PutExtra("data", notificationText);
                //SendBroadcast(intent);
                JobFinished(@params, false);
            });

            return true;
        }

        public override bool OnStopJob(JobParameters @params)
        {
            return true;
        }
    }
}

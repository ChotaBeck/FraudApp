using Android.App;
using Android.OS;
using Android.Widget;
using System.Threading.Tasks;

namespace FraudApp
{
    [Activity(Label = "Message Detail")]
    public class MessageDetailActivity : Activity
    {
        private FraudCheckService _fraudCheckService;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_message_detail);

            _fraudCheckService = new FraudCheckService();

            var fromTextView = FindViewById<TextView>(Resource.Id.detailFromTextView);
            var bodyTextView = FindViewById<TextView>(Resource.Id.detailBodyTextView);
            var fraudStatusTextView = FindViewById<TextView>(Resource.Id.fraudStatusTextView);

            string from = Intent.GetStringExtra("From") ?? "Unknown";
            string body = Intent.GetStringExtra("Body") ?? "No message";

            fromTextView.Text = $"From: {from}";
            bodyTextView.Text = body;

            CheckFraudStatus(body, fraudStatusTextView);
        }

        private async Task CheckFraudStatus(string messageBody, TextView statusTextView)
        {
            statusTextView.Text = "Checking fraud status...";
            string fraudStatus = await _fraudCheckService.CheckMessageFraudAsync(messageBody);
            RunOnUiThread(() =>
            {
                switch (fraudStatus.ToLower())
                {
                    case "fraudulent":
                        statusTextView.Text = "WARNING: This message may be fraudulent!";
                        statusTextView.SetTextColor(Android.Graphics.Color.Red);
                        break;
                    case "legitimate":
                        statusTextView.Text = "This message appears to be legitimate.";
                        statusTextView.SetTextColor(Android.Graphics.Color.Green);
                        break;
                    default:
                        statusTextView.Text = "Unable to determine fraud status.";
                        statusTextView.SetTextColor(Android.Graphics.Color.Gray);
                        break;
                }
            });
        }
    }
}
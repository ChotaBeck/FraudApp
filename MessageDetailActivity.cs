using Android.App;
using Android.OS;
using Android.Widget;
using System.Threading.Tasks;
using Android.Telephony;

namespace FraudApp
{
    [Activity(Label = "Message Detail")]
    public class MessageDetailActivity : Activity
    {
        private FraudCheckService _fraudCheckService;
        private EditText _recipientEditText;
        private EditText _messageEditText;
        private Button _sendButton;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_message_detail);

            _fraudCheckService = new FraudCheckService();

            var fromTextView = FindViewById<TextView>(Resource.Id.detailFromTextView);
            var bodyTextView = FindViewById<TextView>(Resource.Id.detailBodyTextView);
            var fraudStatusTextView = FindViewById<TextView>(Resource.Id.fraudStatusTextView);

            _recipientEditText = FindViewById<EditText>(Resource.Id.recipientEditText);
            _messageEditText = FindViewById<EditText>(Resource.Id.messageEditText);
            _sendButton = FindViewById<Button>(Resource.Id.sendButton);

            string from = Intent.GetStringExtra("From") ?? "Unknown";
            string body = Intent.GetStringExtra("Body") ?? "No message";

            fromTextView.Text = $"From: {from}";
            bodyTextView.Text = body;

            CheckFraudStatus(body, fraudStatusTextView);

            _sendButton.Click += SendButton_Click;
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

        private void SendButton_Click(object sender, System.EventArgs e)
        {
            string recipient = _recipientEditText.Text;
            string message = _messageEditText.Text;

            if (string.IsNullOrEmpty(recipient) || string.IsNullOrEmpty(message))
            {
                Toast.MakeText(this, "Please enter both recipient and message", ToastLength.Short).Show();
                return;
            }

            try
            {
                SmsManager.Default.SendTextMessage(recipient, null, message, null, null);
                Toast.MakeText(this, "Message sent successfully", ToastLength.Short).Show();
                
                // Clear input fields after sending
                _recipientEditText.Text = string.Empty;
                _messageEditText.Text = string.Empty;
            }
            catch (System.Exception ex)
            {
                Toast.MakeText(this, $"Failed to send message: {ex.Message}", ToastLength.Long).Show();
            }
        }
    }
}
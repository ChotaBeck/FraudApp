using Android.App;
using Android.OS;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using System.Collections.Generic;
using Android.Views;
using Android.Content;
using System.Linq;
using Android.Util;
using Android.App;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace FraudApp
{
  [Activity(Label = "Chat")]
    public class ChatActivity : Activity
    {
        private RecyclerView _recyclerView;
        private EditText _messageEditText;
        private Button _sendButton;
        private ChatAdapter _adapter;
        private List<SmsMessage> _messages;
        private string _contactNumber;
        private FraudCheckService _fraudCheckService;
        private const string TAG = "ChatActivity";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_chat);

            _contactNumber = Intent.GetStringExtra("ContactNumber") ?? "";
            Title = _contactNumber;

            _recyclerView = FindViewById<RecyclerView>(Resource.Id.chatRecyclerView);
            _messageEditText = FindViewById<EditText>(Resource.Id.messageEditText);
            _sendButton = FindViewById<Button>(Resource.Id.sendButton);

            _fraudCheckService = new FraudCheckService();
            _messages = new List<SmsMessage>();
            _adapter = new ChatAdapter(this, _messages);
            _adapter.ItemClick += OnMessageClick;

            _recyclerView.SetLayoutManager(new LinearLayoutManager(this));
            _recyclerView.SetAdapter(_adapter);

            LoadMessages();

            _sendButton.Click += SendButton_Click;
        }

        private void LoadMessages()
        {
            Log.Debug(TAG, "LoadMessages called");
            var app = ApplicationContext as MyApplication;

            if (app != null)
            {
                // Load messages for this contact from the application's message list
                _messages = app.MessageList
                    .Where(m => m.From == _contactNumber)
                    .ToList();
                
                Log.Debug(TAG, $"Loaded {_messages.Count} messages for contact {_contactNumber}");
                
                _adapter.UpdateMessages(_messages);
                _recyclerView.ScrollToPosition(_messages.Count - 1);
            }
            else
            {
                Log.Error(TAG, "Application is null or not of type MyApplication");
            }
        }
         private async void OnMessageClick(object sender, int position)
        {
            var message = _messages[position];
            await CheckFraudAndShowResults(message.Body);
        }

         private async Task CheckFraudAndShowResults(string messageBody)
        {
            // Show loading dialog
            var progressDialog = ProgressDialog.Show(this, "Checking", "Analyzing message for fraud...", true);

            try
            {
                var result = await _fraudCheckService.CheckMessageFraudAsync(messageBody);
                progressDialog.Dismiss();

                if (result != "Error")
                {
                    ShowFraudMetricsDialog(result);
                }
                else
                {
                    Toast.MakeText(this, "Error checking fraud. Please try again.", ToastLength.Short).Show();
                }
            }
            catch (System.Exception ex)
            {
                progressDialog.Dismiss();
                Log.Error(TAG, $"Error checking fraud: {ex.Message}");
                Toast.MakeText(this, "An error occurred. Please try again.", ToastLength.Short).Show();
            }
        }

         private void ShowFraudMetricsDialog(string fraudResult)
{
    var builder = new AlertDialog.Builder(this);
    builder.SetTitle("Fraud Detection Results");

    try
    {
        // Parse the JSON string
        var jsonResult = JObject.Parse(fraudResult);
        //var jsonResult = JsonConvert.DeserializeObject<Dictionary<string, object>>(fraudResult);


        // Extract values from JSON
        string message = jsonResult["message"]?.ToString();
        string fraudPercentage = jsonResult["fraud_percentage"]?.ToString();
        string fraudProbability = jsonResult["fraud_probability"]?.ToString();
        string prediction = jsonResult["prediction"]?.ToString();

        // Create a formatted message
        string formattedMessage = $"Message: {message}\n" +
                                  $"Fraud Percentage: {fraudPercentage}\n" +
                                  $"Fraud Probability: {fraudProbability}\n" +
                                  $"Prediction: {prediction}";

        builder.SetMessage(formattedMessage);
    }
    catch (Exception ex)
    {
        // Handle any JSON parsing errors
        builder.SetMessage($"Error parsing result: {ex.Message}");
    }

    builder.SetPositiveButton("OK", (sender, args) => { });
    builder.Create().Show();
}

        private async void SendButton_Click(object sender, System.EventArgs e)
        {
            string messageBody = _messageEditText.Text;
            if (string.IsNullOrEmpty(messageBody)) return;

            SmsMessage newMessage = new SmsMessage
            {
                From = "Me",
                Body = messageBody
            };

            _messages.Add(newMessage);
            _adapter.NotifyItemInserted(_messages.Count - 1);
            _recyclerView.ScrollToPosition(_messages.Count - 1);

            _messageEditText.Text = "";

            // Send the message
            try
            {
                Android.Telephony.SmsManager.Default.SendTextMessage(_contactNumber, null, messageBody, null, null);
                Toast.MakeText(this, "Message sent", ToastLength.Short).Show();
            }
            catch (System.Exception ex)
            {
                Log.Error(TAG, $"Failed to send message: {ex.Message}");
                Toast.MakeText(this, $"Failed to send message: {ex.Message}", ToastLength.Long).Show();
            }

            // Check for fraud
            string fraudStatus = await _fraudCheckService.CheckMessageFraudAsync(messageBody);
            if (fraudStatus.ToLower() == "fraudulent")
            {
                RunOnUiThread(() => {
                    Toast.MakeText(this, "Warning: This message may be fraudulent!", ToastLength.Long).Show();
                });
            }
        }
    }

    public class ChatAdapter : RecyclerView.Adapter
{
    private List<SmsMessage> _messages;
    private Context _context;
    public event EventHandler<int> ItemClick;

    public ChatAdapter(Context context, List<SmsMessage> messages)
    {
        _context = context;
        _messages = messages;
    }

    public override int ItemCount => _messages.Count;

    public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
    {
        SmsMessage message = _messages[position];
        ChatViewHolder chatViewHolder = holder as ChatViewHolder;

        chatViewHolder.MessageTextView.Text = message.Body;

        if (message.From == "Me")
        {
            chatViewHolder.ItemView.SetBackgroundResource(Resource.Drawable.outgoing_message_background);
            chatViewHolder.MessageTextView.SetTextColor(Android.Graphics.Color.Black);
        }
        else
        {
            chatViewHolder.ItemView.SetBackgroundResource(Resource.Drawable.incoming_message_background);
            chatViewHolder.MessageTextView.SetTextColor(Android.Graphics.Color.Black);
        }

        chatViewHolder.ItemView.Click += (sender, e) => ItemClick?.Invoke(this, position);
    }

    public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
    {
        View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.chat_item, parent, false);
        return new ChatViewHolder(itemView);
    }

    public void UpdateMessages(List<SmsMessage> messages)
    {
        _messages = messages;
        NotifyDataSetChanged();
    }
}

public class ChatViewHolder : RecyclerView.ViewHolder
{
    public TextView MessageTextView { get; private set; }

    public ChatViewHolder(View itemView) : base(itemView)
    {
        MessageTextView = itemView.FindViewById<TextView>(Resource.Id.messageTextView);
    }
}
}
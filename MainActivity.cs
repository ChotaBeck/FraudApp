using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using System.Collections.Generic;

namespace FraudApp
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private const int RequestReadSms = 0;
        private ListView messageListView;
        private List<SmsMessage> messageList = new List<SmsMessage>();
        private MessageAdapter adapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_main);

            messageListView = FindViewById<ListView>(Resource.Id.messageListView);
            adapter = new MessageAdapter(this, messageList);
            messageListView.Adapter = adapter;

             messageListView.ItemClick += MessageListView_ItemClick;

            if (ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.ReadSms) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new string[] { Android.Manifest.Permission.ReadSms }, RequestReadSms);
            }
            else
            {
                ReadSms();
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if (requestCode == RequestReadSms)
            {
                if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                {
                    ReadSms();
                }
                else
                {
                    Toast.MakeText(this, "SMS read permission denied", ToastLength.Short).Show();
                }
            }
        }

        private void MessageListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var selectedMessage = messageList[e.Position];
            var intent = new Intent(this, typeof(MessageDetailActivity));
            intent.PutExtra("From", selectedMessage.From);
            intent.PutExtra("Body", selectedMessage.Body);
            StartActivity(intent);
        }


        private void ReadSms()
        {
            string[] reqCols = new string[] { Telephony.Sms.InterfaceConsts.Address, Telephony.Sms.InterfaceConsts.Body };
            ContentResolver contentResolver = ContentResolver;
            Android.Net.Uri uri = Telephony.Sms.ContentUri;

            using (var cursor = contentResolver.Query(uri, reqCols, null, null, null))
            {
                if (cursor.MoveToFirst())
                {
                    do
                    {
                        var address = cursor.GetString(cursor.GetColumnIndex(Telephony.Sms.InterfaceConsts.Address));
                        var body = cursor.GetString(cursor.GetColumnIndex(Telephony.Sms.InterfaceConsts.Body));
                        messageList.Add(new SmsMessage { From = address, Body = body });
                    } while (cursor.MoveToNext());
                }
            }

            RunOnUiThread(() => adapter.NotifyDataSetChanged());
        }
    }

    public class SmsMessage
    {
        public string From { get; set; }
        public string Body { get; set; }
    }

    public class MessageAdapter : BaseAdapter<SmsMessage>
    {
        private List<SmsMessage> messages;
        private Context context;

        public MessageAdapter(Context context, List<SmsMessage> messages)
        {
            this.context = context;
            this.messages = messages;
        }

        public override SmsMessage this[int position] => messages[position];

        public override int Count => messages.Count;

        public override long GetItemId(int position) => position;

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;
            if (view == null)
            {
                view = LayoutInflater.From(context).Inflate(Resource.Layout.message_item, null);
            }

            var message = messages[position];
            var fromTextView = view.FindViewById<TextView>(Resource.Id.textViewFrom);
            var bodyTextView = view.FindViewById<TextView>(Resource.Id.textViewMessage);

            fromTextView.Text = $"From: {message.From}";
            bodyTextView.Text = message.Body;

            return view;
        }
    }
}
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
using System.Linq;
using Android.Util;
using Android.Graphics;

namespace FraudApp
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private const int RequestReadSms = 0;
        private ListView contactListView;
        private List<string> contactList = new List<string>();
        private BlackTextArrayAdapter adapter;
        private const string TAG = "MainActivity";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Log.Debug(TAG, "OnCreate called");

            SetContentView(Resource.Layout.activity_main);
            Log.Debug(TAG, "SetContentView called");

            contactListView = FindViewById<ListView>(Resource.Id.contactListView);
            if (contactListView == null)
            {
                Log.Error(TAG, "contactListView is null");
            }
            else
            {
                Log.Debug(TAG, "contactListView found");
            }

            adapter = new BlackTextArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, contactList);
            contactListView.Adapter = adapter;

            TextView emptyStateTextView = FindViewById<TextView>(Resource.Id.emptyStateTextView);
            contactListView.EmptyView = emptyStateTextView;

            contactListView.ItemClick += ContactListView_ItemClick;

            if (ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.ReadSms) != Permission.Granted)
            {
                Log.Debug(TAG, "Requesting SMS permission");
                ActivityCompat.RequestPermissions(this, new string[] { Android.Manifest.Permission.ReadSms }, RequestReadSms);
            }
            else
            {
                Log.Debug(TAG, "SMS permission already granted, reading SMS");
                ReadSms();
            }
        }

        private void ContactListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var selectedContact = contactList[e.Position];
            Log.Debug(TAG, $"Contact clicked: {selectedContact}");
            var intent = new Intent(this, typeof(ChatActivity));
            intent.PutExtra("ContactNumber", selectedContact);
            StartActivity(intent);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if (requestCode == RequestReadSms)
            {
                if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                {
                    Log.Debug(TAG, "SMS permission granted, reading SMS");
                    ReadSms();
                }
                else
                {
                    Log.Warn(TAG, "SMS permission denied");
                    Toast.MakeText(this, "SMS read permission denied", ToastLength.Short).Show();
                }
            }
        }

        private void ReadSms()
        {
            Log.Debug(TAG, "ReadSms called");
            string[] reqCols = new string[] { Telephony.Sms.InterfaceConsts.Address, Telephony.Sms.InterfaceConsts.Body };
            ContentResolver contentResolver = ContentResolver;
            Android.Net.Uri uri = Telephony.Sms.ContentUri;

            var messageList = new List<SmsMessage>();

            using (var cursor = contentResolver.Query(uri, reqCols, null, null, null))
            {
                if (cursor != null && cursor.MoveToFirst())
                {
                    do
                    {
                        var address = cursor.GetString(cursor.GetColumnIndex(Telephony.Sms.InterfaceConsts.Address));
                        var body = cursor.GetString(cursor.GetColumnIndex(Telephony.Sms.InterfaceConsts.Body));
                        messageList.Add(new SmsMessage { From = address, Body = body });
                    } while (cursor.MoveToNext());
                }
                else
                {
                    Log.Warn(TAG, "No SMS messages found or cursor is null");
                }
            }

            // Store the messageList in the custom Application class
            var app = Application as MyApplication;
            if (app != null)
            {
                app.MessageList = messageList;
            }

            // Get unique contacts
            contactList = messageList.Select(m => m.From).Distinct().ToList();
            Log.Debug(TAG, $"Found {contactList.Count} unique contacts");

            RunOnUiThread(() => 
            {
                adapter.Clear();
                adapter.AddAll(contactList);
                adapter.NotifyDataSetChanged();
                Log.Debug(TAG, "Adapter updated");

                if (contactList.Count == 0)
                {
                    Log.Debug(TAG, "No contacts found");
                    FindViewById<TextView>(Resource.Id.emptyStateTextView).Visibility = ViewStates.Visible;
                }
                else
                {
                    FindViewById<TextView>(Resource.Id.emptyStateTextView).Visibility = ViewStates.Gone;
                }
            });
        }
    }

    public class BlackTextArrayAdapter : ArrayAdapter<string>
    {
        public BlackTextArrayAdapter(Context context, int resourceId, List<string> items) : base(context, resourceId, items)
        {
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = base.GetView(position, convertView, parent);
            TextView text = (TextView)view.FindViewById(Android.Resource.Id.Text1);
            text.SetTextColor(Color.Black);
            return view;
        }
    }

    public class SmsMessage
    {
        public string From { get; set; }
        public string Body { get; set; }
    }
}
using Android.App;
using Android.Runtime;
using System.Collections.Generic;

namespace FraudApp
{
    [Application]
    public class MyApplication : Application
    {
        public List<SmsMessage> MessageList { get; set; }

        public MyApplication(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {
            MessageList = new List<SmsMessage>();
        }

        public override void OnCreate()
        {
            base.OnCreate();
            // Initialize your application-wide resources here
        }
    }
}
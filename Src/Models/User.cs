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

namespace com.xamarin.beta_3.Models
{
    class User
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public String Url { get; set; }
    }
}
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

namespace com.xamarin.ipgMobile.Models  
{
    class Report
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Img { get; set; }
        public bool IsComplete { get; set; }

        // EF core ainda não tem datatype para geospatial
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public int CategoryId { get; set; }
        public string UserId { get; set; }

    }
}
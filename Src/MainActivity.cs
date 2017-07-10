using System;
using Android.App;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Util;
using Android.Widget;
using Android.Content;
using Xamarin.Forms.Maps;
using Android.Gms.Maps;
using Xamarin;
using Android.Gms.Maps.Model;
using Android.Support.Design.Widget;
using Xamarin.Forms.Platform.Android;

namespace com.xamarin.ipgMobile
{
    [Activity(Label = "IPG - Projeto")]
    public class MainActivity : AppCompatActivity, ILocationListener, IOnMapReadyCallback
    {
        static readonly string TAG = "X:" + typeof(MainActivity).Name;
        LocationManager locMgr;

        // --------------- DEBUG ----------------
        Button _button;
        //TextView _textViewLatitude;
        //TextView _textViewLongitude;
        // --------------- DEBUG ----------------

        private GoogleMap _map;
        private MapFragment _mapFragment;

        ProgressDialog progress; // spinner stuff para location
        protected override void OnCreate(Bundle bundle)
        {

            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);
            // receber categories
            var spinnerCat = Intent.GetStringExtra("spinnerCat") ?? "Data not available";

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            //Toolbar will now take on default Action Bar characteristics
            SetActionBar(toolbar);
            //You can now use and reference the ActionBar
            ActionBar.Title = "Hello from Toolbar";


            locMgr = GetSystemService(Context.LocationService) as LocationManager;
            var locationCriteria = new Criteria();
            locationCriteria.Accuracy = Accuracy.Coarse;
            locationCriteria.PowerRequirement = Power.Medium;
            string locationProvider = locMgr.GetBestProvider(locationCriteria, true);
            Log.Debug(TAG, "Starting location updates with " + locationProvider.ToString());
            locMgr.RequestLocationUpdates(locationProvider, 4000, 1, this);

            // spinner para location
            progress = new ProgressDialog(this);
            progress.Indeterminate = true;
            progress.SetProgressStyle(ProgressDialogStyle.Spinner);
            progress.SetMessage(GetString(Resource.String.GetPosition) + " " + locationProvider + "... Please wait...");
            progress.SetCancelable(false);
            progress.Show();

            //var fab = FindViewById<com.refractored.fab.FloatingActionButton>(Resource.Id.fab);
            //fab.Click += (sender, args) =>
            //{
            //    var location = locMgr.GetLastKnownLocation(locationProvider);

            //    var photoActivity = new Intent(this, typeof(PhotoActivity));
            //    photoActivity.PutExtra("MyLatitude", location.Latitude.ToString());
            //    photoActivity.PutExtra("MyLongitude", location.Longitude.ToString());

            //    StartActivity(photoActivity);
            //};

            // --------------- DEBUG ----------------
            _button = FindViewById<Button>(Resource.Id.fab);
            //_textViewLatitude = FindViewById<TextView>(Resource.Id.textViewLatitude);
            //_textViewLongitude = FindViewById<TextView>(Resource.Id.textViewLongitude);

            _button.Click += delegate
            {
                var location = locMgr.GetLastKnownLocation(locationProvider);

                var photoActivity = new Intent(this, typeof(PhotoActivity));
                photoActivity.PutExtra("MyLatitude", location.Latitude.ToString());
                photoActivity.PutExtra("MyLongitude", location.Longitude.ToString());
                photoActivity.PutExtra("SpinnerCat", spinnerCat);
                

                StartActivity(photoActivity);

            };

            // --------------- DEBUG ----------------

            InitMapFragment();
            Log.Debug(TAG, "MainActivity is loaded.");
        }
        // Launches the startup task
        protected override void OnResume()
        {
            base.OnResume();
        }

        private void InitMapFragment()
        {
            _mapFragment = FragmentManager.FindFragmentByTag("map") as MapFragment;
            if (_mapFragment == null)
            {
                GoogleMapOptions mapOptions = new GoogleMapOptions()
                    .InvokeMapType(GoogleMap.MapTypeHybrid)
                    .InvokeZoomControlsEnabled(false)
                    .InvokeCompassEnabled(true);

                FragmentTransaction fragTx = FragmentManager.BeginTransaction();
                _mapFragment = MapFragment.NewInstance(mapOptions);
                fragTx.Add(Resource.Id.map, _mapFragment, "map");
                fragTx.Commit();
            }
            _mapFragment.GetMapAsync(this);
            // --------------- DEBUG ----------------
            //  var mapFragment = ((SupportMapFragment)SupportFragmentManager.FindFragmentById(Resource.Id.map));
            //  mapFragment.GetMapAsync(this);
            // --------------- DEBUG ----------------
        }

        // Prevent the back button from canceling the startup process
        public override void OnBackPressed() { }



        //--------------------------------------------------------------
        //----------------  INTERFACES ILocationListener----------------
        //--------------------------------------------------------------
        public void OnLocationChanged(Location location)
        {
            // --------------- DEBUG ----------------
            //  _textViewLatitude.Text = location.Latitude.ToString();
            //  _textViewLongitude.Text = location.Longitude.ToString();
            Log.Debug(TAG, "Location changed MAIN");
            Log.Debug(TAG, "Latitude: " + location.Latitude.ToString());
            Log.Debug(TAG, "Longitude: " + location.Longitude.ToString());
            // --------------- DEBUG ----------------

            progress.Hide(); /// melhor alternativa !!!! 
            UpdateLocation(location); // update marker quando utilizador se move
        }

        private void UpdateLocation(Location location)
        {
            _map.Clear(); // remover os anterior ... encontrar melhor método!!
            LatLng latlng = new LatLng(location.Latitude, location.Longitude);
            CameraUpdate camera = CameraUpdateFactory.NewLatLngZoom(latlng, 18);
            _map.MoveCamera(camera);

            MarkerOptions options = new MarkerOptions()
                .SetPosition(latlng)
                .SetTitle("Estamos Aqui!")
                .SetSnippet("mova-se até ao local pretendido.")
                .Draggable(true); // mover manualmente, talvez colocar false

            _map.AddMarker(options);

            Marker marker = _map.AddMarker(new MarkerOptions()
                                            .SetPosition(latlng)
                                            .SetTitle("Estamos Aqui!")
                                            .SetSnippet("mova-se até ao local pretendido.")
                                            .Draggable(true)); // mover manualmente, talvez colocar false

            marker.ShowInfoWindow(); // abrir info allways
        }

        public void OnProviderDisabled(string provider)
        {
            Log.Debug(TAG, provider + " disaled by user");
        }

        public void OnProviderEnabled(string provider)
        {
            Log.Debug(TAG, provider + " enabled by user");
        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
            Log.Debug(TAG, provider + " availability has changed to " + status.ToString());
        }
        //--------------------------------------------------------------
        //----------------  INTERFACES IOnMapReadyCallback--------------
        //--------------------------------------------------------------
        public void OnMapReady(GoogleMap map)
        {
            _map = map; // iniciar o fragmento com a mapa já configurado
        }



    }

}
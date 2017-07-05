using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Util;
using Plugin.SecureStorage;
using com.xamarin.beta_3.Models;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;
using Android.Widget;
using Android.Locations;
using Android.Runtime;

namespace com.xamarin.beta_3
{
    [Activity(Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : AppCompatActivity/*, ILocationListener*/
    {
        //LocationManager locMgr;
        static readonly string TAG = "X:" + typeof(SplashActivity).Name;

        public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
        {
            base.OnCreate(savedInstanceState, persistentState);
            Log.Debug(TAG, "SplashActivity.OnCreate");
        }

        // Launches the startup task
        protected override void OnResume()
        {
            base.OnResume();

            //// initialize location manager
            //locMgr = GetSystemService(Context.LocationService) as LocationManager;

            //var locationCriteria = new Criteria();
            //    locationCriteria.Accuracy = Accuracy.Coarse;
            //    locationCriteria.PowerRequirement = Power.Medium;

            //string locationProvider = locMgr.GetBestProvider(locationCriteria, true);

            //Log.Debug(TAG, "Starting location updates with " + locationProvider.ToString());
            //locMgr.RequestLocationUpdates(locationProvider, 2000, 1, this);


            Task startupWork = new Task(() => { LoginStartup();/* PositionStartup();*/ });
            startupWork.Start();
        }

        // se pretender obter position em backgroud COMENTAR onPause()
        //protected override void OnPause()
        //{
        //    base.OnPause();
        //    // RemoveUpdates takes a pending intent - here, we pass the current Activity
        //    locMgr.RemoveUpdates(this);
        //    Log.Debug(TAG, "Location updates paused because application is entering the background");
        //}


        void PositionStartup()
        {
            //passei para onResume .. não funcionava aqui .. rever
        }

        async void LoginStartup()
        {
            Log.Debug(TAG, "Performing some startup work that takes a bit of time.");
            SecureStorageImplementation.StoragePassword = "passwordINIT";
            if (CrossSecureStorage.Current.HasKey("Email"))
            {
                User user = new User
                {
                    Email = CrossSecureStorage.Current.GetValue("Email"),
                    Password = CrossSecureStorage.Current.GetValue("Password")
                };

                var content = new StringContent(
                JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");

                var client = new HttpClient();
                var result = await client.PostAsync(CrossSecureStorage.Current.GetValue("Url") + "/api/account/token", content);
                if (result.IsSuccessStatusCode) // Já existe utilizador válido
                {
                    //var token = await result.Content.ReadAsStringAsync();
                    StartActivity(new Intent(Application.Context, typeof(MainActivity)));
                }
                else // password inválido
                {
                    Toast.MakeText(this, GetString(Resource.String.LoginFail), ToastLength.Long).Show();
                    // preencher com values que existem, password deve ter sidoalterada
                    var LoginActivity = new Intent(Application.Context, typeof(LoginActivity));
                    LoginActivity.PutExtra("userEmail", user.Email);
                    LoginActivity.PutExtra("userPassword", user.Password);
                    LoginActivity.PutExtra("userUrl", user.Url);
                    StartActivity(LoginActivity);
                }
            }
            else
                // primeiro login na applicação
                StartActivity(new Intent(Application.Context, typeof(LoginActivity)));
        }

        // Prevent the back button from canceling the startup process
        public override void OnBackPressed() { }


        //// back Location interface
        //public void OnLocationChanged(Location location)
        //{
        //    //CrossSecureStorage.Current.SetValue("Latitude", location.Latitude.ToString());
        //    //CrossSecureStorage.Current.SetValue("Longitude", location.Longitude.ToString());
        //    //CrossSecureStorage.Current.SetValue("Provider", location.Provider.ToString());
        //    //Log.Debug(TAG, "Location changed SPLASH");
        //    //Log.Debug(TAG, "Latitude: " + location.Latitude.ToString());
        //    //Log.Debug(TAG, "Longitude: " + location.Longitude.ToString());

        //}

        //public void OnProviderDisabled(string provider)
        //{
        //    Log.Debug(TAG, provider + " disaled by user");
        //}

        //public void OnProviderEnabled(string provider)
        //{
        //    Log.Debug(TAG, provider + " enabled by user");
        //}

        //public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        //{
        //    Log.Debug(TAG, provider + " availability has changed to " + status.ToString());
        //}
    }
}
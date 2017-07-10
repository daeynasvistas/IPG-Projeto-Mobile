using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Util;
using Plugin.SecureStorage;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using Android.Widget;
using Android.Locations;
using Android.Runtime;
using com.xamarin.ipgMobile.Models;
using System.Collections;

namespace com.xamarin.ipgMobile 
{
    [Activity(Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : AppCompatActivity/*, ILocationListener*/
    {
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
            Task startupWork = new Task(() => { LoginStartup();/* PositionStartup();*/ });
            startupWork.Start();
        }


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

                    // receber as settings da aplicação (categorias)
                    var uri = new Uri(CrossSecureStorage.Current.GetValue("Url") +  "/api/categories");
                    HttpClient myClient = new HttpClient();

                    var MainActivity = new Intent(Application.Context, typeof(MainActivity));

                    var response = await myClient.GetAsync(uri);
                        if (response.IsSuccessStatusCode)
                        {
                            var content_cat = await response.Content.ReadAsStringAsync();
                            MainActivity.PutExtra("spinnerCat", content_cat);
                        //  var Items = JsonConvert.DeserializeObject<List<RootObject>>(content_cat);
                    }

                    StartActivity(MainActivity);
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
    }
}
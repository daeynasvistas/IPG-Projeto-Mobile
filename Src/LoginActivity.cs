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
using System.Net.Http;
using Newtonsoft.Json;
using Plugin.SecureStorage;
using com.xamarin.ipgMobile.Models;

namespace com.xamarin.ipgMobile 
{
    [Activity(Label = "LoginActivity", WindowSoftInputMode = SoftInput.StateHidden)] // hide teclado quando não tenho focus
    public class LoginActivity : Activity
    {
        ProgressBar spinner; // spinner stuff para location
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Login);

            Button button = FindViewById<Button>(Resource.Id.buttonLogin);
            EditText editTextEmail = FindViewById<EditText>(Resource.Id.editTextEmail);
            EditText editTextPassword = FindViewById<EditText>(Resource.Id.editTextPassword);
            EditText editTextUrl = FindViewById<EditText>(Resource.Id.editTextUrl);

            spinner = FindViewById<ProgressBar>(Resource.Id.progressBar1);  

            // old value if exist
            editTextEmail.Text = Intent.GetStringExtra("userEmail") ?? "";
            editTextPassword.Text = Intent.GetStringExtra("userPassword") ?? "";
            editTextUrl.Text = Intent.GetStringExtra("userUrl") ?? "";

            // debug:
            editTextUrl.Text = "http://projetoipg.azurewebsites.net";
            // Create your application here
            spinner.Visibility = ViewStates.Gone;

            button.Click += delegate
            {
                spinner.Visibility = ViewStates.Visible;
                // teste de Login e receber token
                var result = LoginAsync(editTextEmail.Text.ToString(), 
                                        editTextPassword.Text.ToString(), 
                                        editTextUrl.Text.ToString());
            };


        }

        private async System.Threading.Tasks.Task<object> LoginAsync(string email, string password, string url)
        {
            User user = new User { Email = email, Password = password };
            var content = new StringContent(
                JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");

            var client = new HttpClient();
            var result = await client.PostAsync(url + "/api/account/token", content);

            if (result.IsSuccessStatusCode)
            {
                var token = await result.Content.ReadAsStringAsync();
                // plugin sameerIOTApps.Plugin.SecureStorage 
                CrossSecureStorage.Current.SetValue("SessionToken", token);
                CrossSecureStorage.Current.SetValue("Email", email);
                CrossSecureStorage.Current.SetValue("Password", password);
                CrossSecureStorage.Current.SetValue("Url", url);

                Toast.MakeText(this, GetString(Resource.String.LoginOk), ToastLength.Long).Show();
                StartActivity(new Intent(Application.Context, typeof(MainActivity)));
            }
            else
            {
                Toast.MakeText(this, GetString(Resource.String.LoginFail), ToastLength.Long).Show();
                spinner.Visibility = ViewStates.Gone;
            }
            return result.StatusCode.ToString();
        }
    }
}
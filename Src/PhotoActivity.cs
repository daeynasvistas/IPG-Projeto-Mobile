using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Android.Graphics;
using System.IO;
using System.Collections;
using com.xamarin.ipgMobile.Models;
using Newtonsoft.Json;
using Plugin.SecureStorage;
using System.Net.Http;
using System.Text;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;


namespace com.xamarin.ipgMobile 
{

    [Activity(Label = "PhotoActivity")]
    public class PhotoActivity : Activity
    {

        protected static Bitmap bitmap; 
        ImageView imageView;
        Button buttonCamera;
        Button buttonSend;
        string str="";
        double latitude;
        double longitude;
        string token = "";
        ProgressDialog progress; // spinner stuff para location


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Photo);
            imageView = FindViewById<ImageView>(Resource.Id.imageViewPhoto);

            EditText editTextTitle = FindViewById<EditText>(Resource.Id.editTextreportTitle);
            EditText editTextDescription = FindViewById<EditText>(Resource.Id.editTextreportDescription);

            progress = new ProgressDialog(this);
            progress.Indeterminate = true;
            progress.SetProgressStyle(ProgressDialogStyle.Spinner);

            // Receber todas as cenas da posição GPS
            latitude = Intent.GetDoubleExtra("MyLatitude", latitude);
            longitude = Intent.GetDoubleExtra("MyLongitude", longitude);


            buttonCamera = FindViewById<Button>(Resource.Id.buttonCamera);
            // // clique na camera
            buttonCamera.Click += buttonCameraClick;


            if (bitmap != null) { imageView.SetImageBitmap(bitmap); } // guardar state -.. talvez melhor método


            ArrayList items = new ArrayList();  // categoria nome
            ArrayList items_id = new ArrayList(); // id (para guardar ID, spinner não tem ID????) ver melhor


            Spinner spinner = FindViewById<Spinner>(Resource.Id.spinner);
            Newtonsoft.Json.Linq.JArray json = Newtonsoft.Json.Linq.JArray.Parse(CrossSecureStorage.Current.GetValue("SpinnerCategory"));
            foreach (var dataItem in json)
            {
                items.Add(dataItem["name"]);
                items_id.Add(dataItem["categoryId"]);
            }

            ArrayAdapter adapter = new ArrayAdapter(this, Resource.Layout.Spinner_layout /*Android.Resource.Layout.SimpleSpinnerItem*/, items);
            adapter.SetDropDownViewResource(Resource.Layout.Spinner_item);
            spinner.Adapter = adapter;


            // enviar as cenas ...
            // --------------- DEBUG ----------------
            buttonSend = FindViewById<Button>(Resource.Id.fab);
            buttonSend.Click += async delegate
            {
                // ALTERAR .. receber token para renovação cada x tempo
                User user = new User
                {
                    Email = CrossSecureStorage.Current.GetValue("Email"),
                    Password = CrossSecureStorage.Current.GetValue("Password")
                };
                showWaitDialog();
                string tok = await StoreTokenFromLoginAsync(user);
                await PostReport(editTextTitle, editTextDescription, tok);
                progress.Hide();
                StartActivity(new Intent(Application.Context, typeof(MainActivity)));
            };
    
    }

        private void showWaitDialog()
        {
            // spinner para location
           
            progress.SetMessage("... Posting Please wait...");
            progress.SetCancelable(false);
            progress.Show();
        }

        private async Task<string> StoreTokenFromLoginAsync(User user)
        {
            var content = new StringContent(
            JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");

            var client = new HttpClient();
            var result = await client.PostAsync(CrossSecureStorage.Current.GetValue("Url") + "/api/account/token", content);
            if (result.IsSuccessStatusCode) // LOGIN OK
            {
                var tok = await result.Content.ReadAsStringAsync();
                dynamic resultado = JObject.Parse(tok);
                token = resultado.token;
            };

            return token;   
        }

        private async Task PostReport(EditText editTextTitle, EditText editTextDescription, string token)
        {

            Report report = new Report  // novo report
            {
                Name = editTextTitle.Text,
                Description = editTextDescription.Text,
                Img = "data:image/jpeg;base64," + str,
                IsComplete = false,
                Latitude = latitude,
                Longitude = longitude,
                CategoryId = 1,
                UserId = CrossSecureStorage.Current.GetValue("Email")
            };

            var content = new StringContent(JsonConvert.SerializeObject(report), Encoding.UTF8, "application/json");
            //.----------------------------------------------------------------------------
            HttpClient oHttpClient = new HttpClient();

            oHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var oTaskPostAsync = oHttpClient.PostAsync(CrossSecureStorage.Current.GetValue("Url") + "/api/report", content);

            await oTaskPostAsync.ContinueWith((oHttpResponseMessage) =>
             {
                 var data = oHttpResponseMessage.Result.Content.ReadAsStringAsync();
                 dynamic resultado = JObject.Parse(data.Result);
             });
            StartActivity(new Intent(Application.Context, typeof(MainActivity)));

        }

    protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
    {
            base.OnActivityResult(requestCode, resultCode, data);
            bitmap = (Bitmap)data.Extras.Get("data");
            imageView.SetImageBitmap(bitmap); 
            buttonCamera.Text = GetString(Resource.String.alterPhoto); // muda text

            byte[] bitmapData;  // só recebe small image x 190 ... 
            using (var stream = new MemoryStream())
            {
                bitmap.Compress(Bitmap.CompressFormat.Jpeg, 0, stream);
                bitmapData = stream.ToArray();
                str = Convert.ToBase64String(bitmapData);  // para enviar no JSON
            }


            //using (var stream = new MemoryStream()) // Resize para enviar
            //{
            //    var bitmapScalled = Bitmap.CreateScaledBitmap(bitmap, 1024, 768, true);
            //    bitmapScalled.Compress(Bitmap.CompressFormat.Jpeg, 90, stream);

            //    var bytes = stream.ToArray();
            //    str = Convert.ToBase64String(bytes);  // para enviar no JSON
            //}
    }

    private void buttonCameraClick(object sender, EventArgs e)
    {
            Intent intent = new Intent(Android.Provider.MediaStore.ActionImageCapture);
            StartActivityForResult(intent, 0);
    }
  

    private Bitmap ResizeBitmap(Bitmap originalImage, int widthToScae, int heightToScale)
    {
        Bitmap resizedBitmap = Bitmap.CreateBitmap(widthToScae, heightToScale, Bitmap.Config.Argb4444);

        float originalWidth = originalImage.Width;
        float originalHeight = originalImage.Height;

        Canvas canvas = new Canvas(resizedBitmap);

        float scale = imageView.Width / originalWidth;

        float xTranslation = 0.0f;
        float yTranslation = (imageView.Height - originalHeight * scale) / 2.0f;

        Matrix transformation = new Matrix();
        transformation.PostTranslate(xTranslation, yTranslation);
        transformation.PreScale(scale, scale);

        Paint paint = new Paint();
        paint.FilterBitmap = true;

        canvas.DrawBitmap(originalImage, transformation, paint);

        return resizedBitmap;
    }

    }
}
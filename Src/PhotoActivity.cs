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

namespace com.xamarin.ipgMobile 
{
    [Activity(Label = "PhotoActivity")]
    public class PhotoActivity : Activity
    {
        protected static Bitmap bitmap; 

        ImageView imageView;
        Button buttonCamera;

        string str;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Photo);

            // Receber todas as cenas da posição GPS
            string latitude = Intent.GetStringExtra("MyLatitude") ?? "Data not available";
            string longitude = Intent.GetStringExtra("MyLongitude") ?? "Data not available";
            // receber categories
            var spinnerCat = Intent.GetStringExtra("SpinnerCat") ?? "Data not available";

            buttonCamera = FindViewById<Button>(Resource.Id.buttonCamera);
            // // clique na camera
            buttonCamera.Click += buttonCameraClick;
            //var buttonEnviar = FindViewById<Button>(Resource.Id.buttonEnviar);
            imageView = FindViewById<ImageView>(Resource.Id.imageViewPhoto);

            if (bitmap != null) { imageView.SetImageBitmap(bitmap); } // guardar state -.. talvez melhor método


            ArrayList items = new ArrayList();  // categoria nome
            ArrayList items_id = new ArrayList(); // id (para guardar ID, spinner não tem ID????) ver melhor

            Spinner spinner = FindViewById<Spinner>(Resource.Id.spinner);
            Newtonsoft.Json.Linq.JArray json = Newtonsoft.Json.Linq.JArray.Parse(spinnerCat);
                foreach (var dataItem in json)
                {
                    items.Add(dataItem["name"]);
                    items_id.Add(dataItem["categoryId"]);
                }

            ArrayAdapter adapter = new ArrayAdapter(this, Resource.Layout.Spinner_layout /*Android.Resource.Layout.SimpleSpinnerItem*/, items);
            adapter.SetDropDownViewResource(Resource.Layout.Spinner_item);
            spinner.Adapter = adapter;
        }



        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            bitmap = (Bitmap)data.Extras.Get("data");
            imageView.SetImageBitmap(bitmap); 
            buttonCamera.Text = GetString(Resource.String.alterPhoto); // muda text

            using (var stream = new MemoryStream()) // Resize para enviar
            {
                ResizeBitmap(bitmap, 800, 600).Compress(Bitmap.CompressFormat.Png, 0, stream);
                var bytes = stream.ToArray();
                str = Convert.ToBase64String(bytes);  // para enviar no JSON
            }
        }



        private void buttonCameraClick(object sender, EventArgs e)
        {
            Intent intent = new Intent(Android.Provider.MediaStore.ActionImageCapture);
            StartActivityForResult(intent, 0);
        }

        private Bitmap ResizeBitmap(Bitmap originalImage, int widthToScae, int heightToScale)
        {
            Bitmap resizedBitmap = Bitmap.CreateBitmap(widthToScae, heightToScale, Bitmap.Config.Argb8888);

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
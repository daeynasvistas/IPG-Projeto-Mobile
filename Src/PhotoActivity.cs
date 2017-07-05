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
using Android.Graphics;
using System.IO;

namespace com.xamarin.beta_3
{
    [Activity(Label = "PhotoActivity")]
    public class PhotoActivity : Activity
    {
        ImageView imageView;
        string str;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Photo);
            
            // Receber todas as cenas da posição GPS
            string latitude = Intent.GetStringExtra("MyLatitude") ?? "Data not available";
            string longitude = Intent.GetStringExtra("MyLongitude") ?? "Data not available";

            // Create your application here
            var buttonCamera = FindViewById<Button>(Resource.Id.buttonCamera);
            // // clique na camera
            buttonCamera.Click += buttonCameraClick;
            // var buttonEnviar = FindViewById<Button>(Resource.Id.buttonEnviar);

            imageView = FindViewById<ImageView>(Resource.Id.imageViewPhoto);

        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            Bitmap bitmap = (Bitmap)data.Extras.Get("data");

            imageView.SetImageBitmap(bitmap); // Origina

            using (var stream = new MemoryStream()) // Resize para enviar
            {
                ResizeBitmap(bitmap, 800, 600).Compress(Bitmap.CompressFormat.Png, 0, stream);

                var bytes = stream.ToArray();
                str = Convert.ToBase64String(bytes);
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
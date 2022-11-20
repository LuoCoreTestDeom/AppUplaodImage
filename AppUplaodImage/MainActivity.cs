using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using AndroidX.Core.Graphics;
using AppUplaodImage.Assets;
using Java.IO;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using static Android.Content.ClipData;

namespace AppUplaodImage
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private GridView gw;
        private JavaList<IDictionary<string, object>> _datas;
        private GridViewAddImgesAdpter gridViewAddImgesAdpter;
      
        private int PHOTO_REQUEST_CAREMA = 1;// 拍照
        private int PHOTO_REQUEST_GALLERY = 2;// 从相册中选择private static final String PHOTO_FILE_NAME = "temp_photo.jpg";
        private Java.IO.File tempFile;
        private String IMAGE_DIR = Android.OS.Environment.ExternalStorageDirectory + "/gridview/";
        /* 头像名称 */
        private String PHOTO_FILE_NAME = "temp_photo.jpg";
        ImageView _imageTest;
        Button _btn_Upload;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
         
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            gw = FindViewById<GridView>(Resource.Id.gw);
            _imageTest = FindViewById<ImageView>(Resource.Id.ImageTest);
            _btn_Upload = FindViewById<Button>(Resource.Id.Btn_Upload);
            _datas = new JavaList<IDictionary<string, object>>();
            gridViewAddImgesAdpter = new GridViewAddImgesAdpter(_datas, this);
            gw.Adapter = gridViewAddImgesAdpter;
            gw.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) =>
            {
                AddImageDialog();
            };
            _btn_Upload.Click += _btn_Upload_Click;
        }

        private void _btn_Upload_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < _datas.Count; i++)
{
                foreach (var item2 in _datas[i])
                {
                    Java.IO.File outputImage = new Java.IO.File(item2.Value.ToString());
                    PostUploadImage<string>("http://127.0.0.1:80/UploadFile", outputImage.Path);

                    gridViewAddImgesAdpter.NotifyDataSetChanged();

                }
                _datas.Remove(i);
            }
          
        }


     
    

        public static T PostUploadImage<T>( string uploadUrl, string imgPath, string fileparameter = "files")
        {
            if (uploadUrl.StartsWith("https")) { System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls; }

            using (HttpClient httpClient=new HttpClient())
            {
                using (MultipartFormDataContent httpContent = new MultipartFormDataContent())
                {
                    using (ByteArrayContent fileContent = new ByteArrayContent(System.IO.File.ReadAllBytes(imgPath)))
                    {
                        try
                        {
                            int pos = imgPath.LastIndexOf("/");
                            string fileName = imgPath.Substring(pos + 1);
                            httpContent.Add(fileContent, fileparameter, fileName);
                            T result = default(T);
                            using (HttpResponseMessage response = httpClient.PostAsync(uploadUrl, httpContent).Result)
                            {
                                try
                                {
                                    string strRes = response.Content.ReadAsStringAsync().Result;
                                    
                                    return result;
                                }
                                finally
                                {
                                    response.Dispose();
                                }

                            }
                        }
                        finally
                        {

                            fileContent.Dispose();
                            httpContent.Dispose();
                            httpClient.Dispose();
                        }

                    }
                }
            }
        }

        private int ConvertPixelsToDp(float pixelValue)
        {
            var dp = (int)((pixelValue) / Resources.DisplayMetrics.Density);
            return dp;
        }
        /// <summary>
        /// 选择图片对话框
        /// </summary>
        public void Showdialog()
        {
            View localView = LayoutInflater.From(this).Inflate(Resource.Layout.dialog_add_picture, null);
            TextView tv_camera = localView.FindViewById<TextView>(Resource.Id.tv_camera);
            TextView tv_gallery = localView.FindViewById<TextView>(Resource.Id.tv_gallery);
            TextView tv_cancel = localView.FindViewById<TextView>(Resource.Id.tv_cancel);
            Dialog  dialog = new Dialog(this, Resource.Style.custom_dialog);
            dialog.SetContentView(localView);
            dialog.Window.SetGravity(GravityFlags.Bottom);
            // 设置全屏
            WindowManagerLayoutParams lp = dialog.Window.Attributes;
            DisplayMetrics metrics = this.Resources.DisplayMetrics;
            lp.Width = ConvertPixelsToDp(metrics.WidthPixels); // 设置宽度
            dialog.Window.Attributes = lp;
            dialog.Show();
            tv_cancel.Click += (object sender, EventArgs e) =>
            {
                dialog.Dismiss();
            };
            tv_camera.Click += (object sender, EventArgs e) =>
            {
                dialog.Dismiss();
                // 拍照
                Camera();
            };
            tv_gallery.Click += (object sender, EventArgs e) =>
            {
                dialog.Dismiss();
                // 从系统相册选取照片
                PhoneSelectionPhoto();
            };



        }



        protected void AddImageDialog()
        {
            AndroidX.AppCompat.App.AlertDialog.Builder builder = new AndroidX.AppCompat.App.AlertDialog.Builder(this);
            builder.SetTitle("添加图片");
            builder.SetIcon(Resource.Mipmap.ic_launcher_round);
            builder.SetCancelable(false); //不响应back按钮
            builder.SetItems(new System.String[] { "本地相册", "相机添加", "取消选择" }, (object sender, DialogClickEventArgs eargs) =>
            {
                AppCompatDialog dialog = (AppCompatDialog)sender;
                switch (eargs.Which)
                {
                    case 0: //本地相册
                        PhoneSelectionPhoto();
                        break;
                    case 1: //手机相机
                        Camera();
                        break;
                    case 2: //取消添加
                        break;
                    default:
                        break;
                }
                dialog.Dismiss();
            });
            //显示对话框
            builder.Create().Show();
        }


        /// <summary>
        /// 判断sdcard是否被挂载
        /// </summary>
        /// <returns></returns>
        public bool HasSdcard()
        {
            return Android.OS.Environment.ExternalStorageState.Equals(Android.OS.Environment.MediaMounted);
        }
        /// <summary>
        /// 拍照
        /// </summary>
        public void Camera()
        {
            // 判断存储卡是否可以用，可用进行存储
            if (HasSdcard())
            {
                if (ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.WriteExternalStorage) != Android.Content.PM.Permission.Granted)
                {
                    //请求权限
                    ActivityCompat.RequestPermissions(this, new String[] { Android.Manifest.Permission.WriteExternalStorage, Android.Manifest.Permission.Camera }, 1);

                }


                Java.IO.File dir = new Java.IO.File(IMAGE_DIR);
                if (!dir.Exists())
                {
                    dir.Mkdir();
                }

                var currenttimemillis = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                tempFile = new Java.IO.File(dir, currenttimemillis + "_" + PHOTO_FILE_NAME);


                //从文件中创建uri
                Android.Net.Uri imageUri = Android.Net.Uri.FromFile(tempFile);
                if (Build.VERSION.SdkInt >= (BuildVersionCodes)24)
                {
                    imageUri = FileProvider.GetUriForFile(this, "com.companyname.appuplaodimage.fileprovider", tempFile);
                }
                Intent intent = new Intent();
                intent.PutExtra(MediaStore.ExtraOutput, imageUri);
                intent.SetAction(MediaStore.ActionImageCapture);
                intent.AddCategory(Intent.CategoryDefault);
                // 开启一个带有返回值的Activity，请求码为PHOTO_REQUEST_CAREMA
                StartActivityForResult(intent, PHOTO_REQUEST_CAREMA);
            }
            else
            {
                Toast.MakeText(this, "未找到存储卡，无法拍照！", ToastLength.Short).Show();
            }
        }

        /// <summary>
        /// 从相册获取2
        /// </summary>
        public void PhoneSelectionPhoto()
        {

            //Intent intent = new Intent(MediaStore.ACTION_PICK_IMAGES);
            //intent.putExtra(MediaStore.EXTRA_PICK_IMAGES_MAX, maxNumPhotosAndVideos);
            //startActivityForResult(intent, PHOTO_PICKER_MULTI_SELECT_REQUEST_CODE);

          
            Intent iet= new Intent();
            iet.SetType("image/*");
            iet.PutExtra(Intent.ExtraAllowMultiple, true);
            iet.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(iet, "Select Picture"), PHOTO_REQUEST_GALLERY);

            //Intent intent = new Intent(
            //        Intent.ActionPick,
            //        MediaStore.Images.Media.ExternalContentUri);
            //StartActivityForResult(intent, PHOTO_REQUEST_GALLERY);
        }

        public void photoPath(String path)
        {
            //Bitmap bitmap = BitmapFactory.DecodeStream(ContentResolver.OpenInputStream(path));
            JavaDictionary<String, Object> map = new JavaDictionary<String, Object>();
            map.Add("path", path);
            _datas.Add(map);
            Log.Info("我是照片=", "" + map);
            gridViewAddImgesAdpter.NotifyDataSetChanged();
        }
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (resultCode == Result.Ok)
            {
                if (requestCode == PHOTO_REQUEST_GALLERY)
                {
                    // 从相册返回的数据
                    if (data != null)
                    {
                        // 得到图片的全路径
                        Android.Net.Uri uri = data.Data;

                        String[] proj = { MediaStore.Images.Media.InterfaceConsts.Data };
                        //好像是android多媒体数据库的封装接口，具体的看Android文档
                        Android.Database.ICursor cursor = this.ContentResolver.Query(uri, proj, null, null, null);

                      
                       

                        //按我个人理解 这个是获得用户选择的图片的索引值
                        int column_index = cursor.GetColumnIndexOrThrow(MediaStore.Images.Media.InterfaceConsts.Data);
                        //将光标移至开头 ，这个很重要，不小心很容易引起越界
                        cursor.MoveToFirst();
                        //最后根据索引值获取图片路径
                        String path = cursor.GetString(column_index);

                        Java.IO.File outputImage = new Java.IO.File(path);
                        if (!outputImage.Exists())
                        {
                            return;
                        }
                        _imageTest.SetImageBitmap(Android.Graphics.BitmapFactory.DecodeFile(path));
                        photoPath(path);
                        //UploadImage(path);
                    }

                }
                else if (requestCode == PHOTO_REQUEST_CAREMA)
                {
                    if (resultCode != Result.Canceled)
                    {
                        // 从相机返回的数据
                        if (HasSdcard())
                        {
                            if (tempFile != null)
                            {
                                //UploadImage(tempFile.Path);
                                photoPath(tempFile.Path);
                            }
                            else
                            {
                                Toast.MakeText(this, "相机异常请稍后再试！", ToastLength.Short).Show();
                            }

                            Log.Info("images", "拿到照片path=" + tempFile.Path);
                        }
                        else
                        {
                            Toast.MakeText(this, "未找到存储卡，无法存储照片！", ToastLength.Short).Show();
                        }
                    }
                }

            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
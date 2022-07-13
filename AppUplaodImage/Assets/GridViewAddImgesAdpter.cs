using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Java.Lang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppUplaodImage.Assets
{
    internal class GridViewAddImgesAdpter : BaseAdapter
    {

        private JavaList<IDictionary<string, object>> _datas;
        private Context _context;
        private LayoutInflater _inflater;
        /**
         * 可以动态设置最多上传几张，之后就不显示+号了，用户也无法上传了
         * 默认9张
         */
        private int maxImages = 4;


        public GridViewAddImgesAdpter(JavaList<IDictionary<string, object>> datas, Context context)
        {
            this._datas = datas;
            this._context = context;
            _inflater = LayoutInflater.From(context);
        }

        /// <summary>
        /// 获取最大上传张数
        /// </summary>
        /// <returns></returns>
        public int getMaxImages()
        {
            return maxImages;
        }
        /// <summary>
        /// 设置最大上传张数
        /// </summary>
        /// <param name="maxImages"></param>
        public void setMaxImages(int maxImages)
        {
            this.maxImages = maxImages;
        }
        public void notifyDataSetChanged(JavaList<IDictionary<string, object>> datas)
        {
            this._datas = datas;
            this.NotifyDataSetChanged();

        }

        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }

        public override long GetItemId(int position)
        {
            return 0;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {

            GridViewAddImgesAdpterViewHolder viewHolder = null;
            if (convertView == null)
            {
                convertView = _inflater.Inflate(Resource.Layout.item_published_grida, parent, false);
                viewHolder = new GridViewAddImgesAdpterViewHolder(convertView);
                convertView.Tag = viewHolder;
            }
            else
            {
                viewHolder = (GridViewAddImgesAdpterViewHolder)convertView.Tag;
            }
            if (_datas != null && position < _datas.Size())
            {

                var imgkeyValuePair= _datas[position].Where(x => x.Key == "path").FirstOrDefault();
                if (imgkeyValuePair.Value != null) 
                {
                    Java.IO.File outputImage = new Java.IO.File(imgkeyValuePair.Value.ToString());
                    if (outputImage.Exists())
                    {
                        viewHolder.ivimage.SetImageBitmap(Android.Graphics.BitmapFactory.DecodeFile(outputImage.Path));
                        viewHolder.btdel.Visibility = ViewStates.Visible;
                        viewHolder.btdel.Click += (object sender, EventArgs e) =>
                        {
                            if (outputImage.Exists())
                            {
                                outputImage.Delete();
                            }
                            _datas.Remove(position);
                            NotifyDataSetChanged();
                        };
                    }
                }
            }
            else
            {
                Log.Error("我是Else==", "1111");
                Glide.With(_context)
                       .Load(Resource.Mipmap.image_add)
                       .SetPriority(Priority.High)
                       .CenterCrop()
                       .Into(viewHolder.ivimage);
                viewHolder.ivimage.SetScaleType(ImageView.ScaleType.FitXy);
                viewHolder.btdel.Visibility = ViewStates.Gone;
            }
            return convertView;
        }


        //Fill in cound here, currently 0
        public override int Count
        {
            get
            {
                int count = _datas == null ? 1 : _datas.Size() + 1;
                if (count >= maxImages)
                {
                    return _datas.Size();
                }
                else
                {
                    return count;
                }
            }
        }

    }

    internal class GridViewAddImgesAdpterViewHolder : Java.Lang.Object
    {
        public ImageView ivimage;
        public Button btdel;
        public View root;

        public GridViewAddImgesAdpterViewHolder(View root)
        {
            ivimage = root.FindViewById<ImageView>(Resource.Id.iv_image);
            btdel = root.FindViewById<Button>(Resource.Id.bt_del);
            this.root = root;
        }
    }
}
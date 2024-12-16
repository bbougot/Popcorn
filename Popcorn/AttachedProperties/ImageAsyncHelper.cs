using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using Popcorn.Services.Cache;

namespace Popcorn.AttachedProperties
{
    public enum ImageType
    {
        Thumbnail,
        Poster,
        Background,
        None
    }

    /// <summary>
    /// Image async
    /// </summary>
    public class ImageAsyncHelper : DependencyObject
    {
        private static readonly Lazy<HttpClient> HttpClient = new Lazy<HttpClient>(() => new HttpClient(), LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Get ImageType
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static ImageType GetType(DependencyObject obj)
        {
            return (ImageType) obj.GetValue(TypeProperty);
        }

        /// <summary>
        /// Set ImageType
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetType(DependencyObject obj, ImageType value)
        {
            obj.SetValue(TypeProperty, value);
        }

        /// <summary>
        /// ImageType property
        /// </summary>
        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.RegisterAttached("Type",
                typeof(ImageType),
                typeof(ImageAsyncHelper),
                new PropertyMetadata(ImageType.Background));

        /// <summary>
        /// Get source uri
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetImagePath(DependencyObject obj)
        {
            return (string) obj.GetValue(ImagePathProperty);
        }

        /// <summary>
        /// Set source uri
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetImagePath(DependencyObject obj, Uri value)
        {
            obj.SetValue(ImagePathProperty, value);
        }

        /// <summary>
        /// Image path property
        /// </summary>
        public static readonly DependencyProperty ImagePathProperty =
            DependencyProperty.RegisterAttached("ImagePath",
                typeof(string),
                typeof(ImageAsyncHelper),
                new PropertyMetadata
                {
                    PropertyChangedCallback = async (obj, e) =>
                    {
                        var image = (Image) obj;
                        var imageType = GetType(obj);
                        await Task.Run(async () =>
                        {
                            var resourceDictionary = new ResourceDictionary
                            {
                                Source = new Uri("Popcorn;component/Resources/ImageLoading.xaml", UriKind.Relative)
                            };

                            try
                            {
                                var path = e.NewValue as string;
                                if (string.IsNullOrEmpty(path))
                                {
                                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                                    {
                                        if (imageType == ImageType.Thumbnail)
                                        {
                                            var errorThumbnail = resourceDictionary["ImageError"] as DrawingImage;
                                            errorThumbnail?.Freeze();
                                            image.RenderTransformOrigin = new Point(0.5d, 0.5d);
                                            image.Stretch = Stretch.None;
                                            image.Source = errorThumbnail;
                                        }
                                        else
                                        {
                                            image.Source = new BitmapImage();
                                        }
                                    });

                                    return;
                                }

                                var hash = Convert.ToBase64String(Encoding.UTF8.GetBytes(path));
                                var mustDownload = false;
                                var cacheService = SimpleIoc.Default.GetInstance<ICacheService>();
                                var localFile = cacheService.Assets + hash;
                                if (!File.Exists(localFile))
                                {
                                    mustDownload = true;
                                    if (imageType == ImageType.Thumbnail)
                                    {
                                        DispatcherHelper.CheckBeginInvokeOnUI(() =>
                                        {
                                            var loadingImage = resourceDictionary["ImageLoading"] as DrawingImage;
                                            loadingImage?.Freeze();

                                            #region Create Loading Animation

                                            var scaleTransform = new ScaleTransform(0.5, 0.5);
                                            var rotateTransform = new RotateTransform(0);

                                            var group = new TransformGroup();
                                            group.Children.Add(scaleTransform);
                                            group.Children.Add(rotateTransform);

                                            var doubleAnimation =
                                                new DoubleAnimation(0, 359, new TimeSpan(0, 0, 0, 1))
                                                {
                                                    RepeatBehavior = RepeatBehavior.Forever
                                                };

                                            rotateTransform.BeginAnimation(RotateTransform.AngleProperty,
                                                doubleAnimation);

                                            var loadingAnimationTransform = group;

                                            #endregion


                                            image.Source = loadingImage;
                                            image.Stretch = Stretch.Uniform;
                                            image.RenderTransformOrigin = new Point(0.5, 0.5);
                                            image.RenderTransform = loadingAnimationTransform;
                                        });
                                    }
                                }

                                try
                                {
                                    if (mustDownload)
                                    {
                                        await using var ms = await HttpClient.Value.GetStreamAsync(path);
                                        await using var stream = new MemoryStream();
                                        await ms.CopyToAsync(stream);
                                        if (!File.Exists(cacheService.Assets + hash))
                                        {
                                            await using var fs =
                                                new FileStream(localFile, FileMode.Create,
                                                    FileAccess.ReadWrite, FileShare.ReadWrite, 4096, true);
                                            var writeAsync = stream.ToArray();
                                            await fs.WriteAsync(writeAsync, 0, writeAsync.Length);
                                        }
                                    }

                                    await using (var fs = new FileStream(localFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                                    {
                                        var bitmapImage = new BitmapImage();
                                        bitmapImage.BeginInit();
                                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                                        if (imageType == ImageType.Thumbnail)
                                        {
                                            bitmapImage.DecodePixelWidth = 400;
                                            bitmapImage.DecodePixelHeight = 600;
                                        }
                                        else if (imageType == ImageType.Poster)
                                        {
                                            bitmapImage.DecodePixelWidth = 500;
                                            bitmapImage.DecodePixelHeight = 750;
                                        }
                                        else if (imageType != ImageType.None)
                                        {
                                            bitmapImage.DecodePixelWidth = 1920;
                                            bitmapImage.DecodePixelHeight = 1080;
                                        }

                                        bitmapImage.StreamSource = fs;
                                        bitmapImage.EndInit();
                                        bitmapImage.Freeze();
                                        DispatcherHelper.CheckBeginInvokeOnUI(() =>
                                        {
                                            image.RenderTransformOrigin = new Point(0, 0);
                                            if (imageType != ImageType.Poster)
                                            {
                                                image.Stretch = Stretch.UniformToFill;
                                            }

                                            image.RenderTransform = new TransformGroup();
                                            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);
                                            image.Source = bitmapImage;
                                        });
                                    }
                                }
                                catch (Exception)
                                {
                                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                                    {
                                        if (imageType == ImageType.Thumbnail)
                                        {
                                            var errorThumbnail = resourceDictionary["ImageError"] as DrawingImage;
                                            errorThumbnail?.Freeze();

                                            image.RenderTransformOrigin = new Point(0.5d, 0.5d);
                                            image.RenderTransform = new TransformGroup();
                                            image.Stretch = Stretch.None;
                                            image.Source = errorThumbnail;
                                        }
                                        else
                                        {
                                            image.RenderTransformOrigin = new Point(0, 0);
                                            image.RenderTransform = new TransformGroup();
                                            image.Source = new BitmapImage();
                                        }
                                    });
                                }
                            }
                            catch (Exception)
                            {
                                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                                {
                                    if (imageType == ImageType.Thumbnail)
                                    {
                                        var errorThumbnail = resourceDictionary["ImageError"] as DrawingImage;
                                        errorThumbnail?.Freeze();

                                        image.RenderTransformOrigin = new Point(0.5d, 0.5d);
                                        image.RenderTransform = new TransformGroup();
                                        image.Stretch = Stretch.None;
                                        image.Source = errorThumbnail;
                                    }
                                    else
                                    {
                                        image.RenderTransformOrigin = new Point(0, 0);
                                        image.RenderTransform = new TransformGroup();
                                        image.Source = new BitmapImage();
                                    }
                                });
                            }
                        });
                    }
                }
            );
    }
}
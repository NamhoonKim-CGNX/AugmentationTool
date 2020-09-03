﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;
using OpenCvSharp;
using Data_AugTool.Model;
using OpenCvSharp.Extensions;
using System.Windows.Media.Effects;
using System.Drawing.Drawing2D;
using OpenCvSharp.Tracking;
using System.Data.SqlClient;
using System.Windows.Media.TextFormatting;
using System.Net;
using MahApps.Metro.Controls;
using System.Web.UI;
using System.Security.Cryptography.X509Certificates;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using OpenCvSharp.XImgProc.Segmentation;

namespace Data_AugTool
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        private Random _RandomGenerator = new Random();
        private List<AlbumentationInfo> _AlbumentationInfos = new List<AlbumentationInfo>();
        private ObservableCollection<AlbumentationInfo> _GeneratedAlbumentations = new ObservableCollection<AlbumentationInfo>();
        private List<AlbumentationInfo> _PreviousAlbumentations = new List<AlbumentationInfo>();
        private string _OutputPath = null;
        //Mat previewMat = new Mat();
        public MainWindow()
        {
            InitializeComponent();

            _AlbumentationInfos.AddRange(new List<AlbumentationInfo>()
            {
                new AlbumentationInfo("Contrast", 0.5, 2.0),
                new AlbumentationInfo("Brightness", 0, 100.0),
                new AlbumentationInfo("Blur", 0, 2),
                new AlbumentationInfo("Rotation" , 0, 30),
                new AlbumentationInfo("Rotation90"),
                new AlbumentationInfo("Horizontal Flip"),
                new AlbumentationInfo("Vertical Flip"),
                new AlbumentationInfo("Noise", 0.0, 20.0),
                new AlbumentationInfo("Zoom In"),
                new AlbumentationInfo("Sharpen"),
                new AlbumentationInfo("CLAHE"),

            });
            foreach (var item in _AlbumentationInfos)
            {
                item.IsChecked = true;
            }
            AlbumentationListBox.ItemsSource = _AlbumentationInfos;
        }

        private void ListView1_SelectionChanged(object sender, SelectedCellsChangedEventArgs e)
        {

        }

        string[] files;

        //<!-- Image listview & orginal Image Viewer with Input URL shown -->
        private void btnLoadFromFile_Click(object sender, RoutedEventArgs e)
        {
            string path;
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                path = dialog.FileName; // 테스트용, 폴더 선택이 완료되면 선택된 폴더를 label에 출력 } 
                files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);

                textBox.Text = path;
            }
            string[] imageFormat = new string[] { "jpg", "jpeg", "png", "bmp", "tif", "tiff" };
            var imageFiles = files.Where(file => imageFormat.Any(extension => file.ToLower().EndsWith(extension))).ToList();
            List<ImageInfo> items = new List<ImageInfo>();
            for (int i = 0; i < imageFiles.Count(); i++)
            {
                items.Add(new ImageInfo() { ImageNumber = i + 1, ImageName = imageFiles[i] });
            }
            ListView1.ItemsSource = items;
            Dynamic2.Source = new BitmapImage(new Uri(items[0].ImageName));
        }

        private void btnLoadFromOutput_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog();
            openFileDialog.IsFolderPicker = true;
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                _OutputPath = openFileDialog.FileName;
                textBox2.Text = _OutputPath;
            }

        }

        private void ListView1_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = ListView1.SelectedItem as ImageInfo;
            if (selectedItem == null)
                return;

            Dynamic2.Source = new BitmapImage(new Uri(selectedItem.ImageName));

            UpdatePreviewImage();
        }

        private void ListView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                var selectedItem = ListView1.SelectedItem as ImageInfo;
                if (selectedItem == null)
                    return;

                var items = ListView1.ItemsSource as List<ImageInfo>;
                items.Remove(selectedItem);
                ListView1.ItemsSource = items;
            }

        }


        private void AlbumentationListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            McScroller.Value = 0;

            UpdatePreviewImage();
            UpdateSliderMinMax();
        }

        private void UpdateSliderMinMax()
        {
            var listbox = AlbumentationListBox as ListBox;
            if (listbox == null)
                return;

            var slider = McScroller as Slider;
            if (slider == null)
                return;

            var listBoxSelectedItem = listbox.SelectedItem as AlbumentationInfo;
            if (listBoxSelectedItem == null)
                return;

            slider.Minimum = listBoxSelectedItem.ValueMin;
            slider.Maximum = listBoxSelectedItem.ValueMax;

            // slider.TickFrequency
        }

        private Slider _Slider;
        private void UpdatePreviewImage()
        {
            var listbox = AlbumentationListBox as ListBox;
            if (listbox == null)
                return;

            var listBoxSelectedItem = listbox.SelectedItem as AlbumentationInfo;
            if (listBoxSelectedItem == null)
                return;

            var selectedImageInfo = ListView1.SelectedItem as ImageInfo;
            if (selectedImageInfo == null)
                return;

            var slider = McScroller as Slider;
            if (slider == null)
                return;

            // 추후 수정
            _Slider = slider;

            Mat previewMat = Recipe(selectedImageInfo.ImageName, listBoxSelectedItem.TypeName);
            // 기존에 알고리즘 구현되었던 곳
            #region
            ////var originalImage = new BitmapImage(new Uri(selectedImageInfo.ImageName));
            //Mat orgMat = new Mat(selectedImageInfo.ImageName);
            ////Mat previewMat = new Mat();


            //ui_PreviewImage.Source = new BitmapImage(new Uri(selectedImageInfo.ImageName));
            //Mat matrix;
            //switch (listBoxSelectedItem.TypeName)
            //{
            //    case "Contrast":
            //        Cv2.AddWeighted(orgMat, slider.Value, orgMat, 0, 0, previewMat);
            //        break;
            //    case "Brightness":
            //        Cv2.Add(orgMat, slider.Value, previewMat);
            //        break;
            //    case "Blur":
            //        Cv2.GaussianBlur(orgMat, previewMat, new OpenCvSharp.Size(9, 9), slider.Value, 1, BorderTypes.Default);
            //        break;
            //    case "Rotation":
            //        matrix = Cv2.GetRotationMatrix2D(new Point2f(orgMat.Width / 2, orgMat.Height / 2), slider.Value, 1.0);
            //        Cv2.WarpAffine(orgMat, previewMat, matrix, new OpenCvSharp.Size(orgMat.Width, orgMat.Height), InterpolationFlags.Cubic);
            //        break;
            //    case "Rotation90":
            //        matrix = Cv2.GetRotationMatrix2D(new Point2f(orgMat.Width / 2, orgMat.Height / 2), 90, 1.0);
            //        Cv2.WarpAffine(orgMat, previewMat, matrix, new OpenCvSharp.Size(orgMat.Width, orgMat.Height), InterpolationFlags.Cubic);
            //        break;
            //    case "Horizontal Flip":
            //        Cv2.Flip(orgMat, previewMat, FlipMode.Y);
            //        break;
            //    case "Vertical Flip":
            //        Cv2.Flip(orgMat, previewMat, FlipMode.X);
            //        break;
            //    case "Noise":
            //        matrix = new Mat(orgMat.Size(), MatType.CV_8UC3);
            //        Cv2.Randn(matrix, Scalar.All(0), Scalar.All(slider.Value));         //정규분포를 나타내는 이미지를 랜덤하게 생성
            //        Cv2.AddWeighted(orgMat, 1, matrix, 1, 0, previewMat);               //두 이미지를 가중치를 설정하여 합침
            //        break;
            //    case "Zoom In":
            //        //Cv2.PyrDown(orgMat, previewMat);
            //        //#1. Center만 확대
            //        double width_param = (int)(0.8 * orgMat.Width); // 0.8이 배율 orgMat.Width이 원본이미지의 사이즈 // 나중에 0.8만 80%형식으로 바꿔서 파라미터로 빼야됨
            //        double height_param = (int)(0.8 * orgMat.Height); // 0.8이 배율 orgMat.Height 원본이미지의 사이즈 //
            //        int startX = orgMat.Width - (int)width_param;// 이미지를 Crop해올 좌상단 위치 지정하는값 // 원본사이즈 - 배율로 감소한 사이즈
            //        int startY = orgMat.Height - (int)height_param;//
            //        Mat tempMat = new Mat(orgMat, new OpenCvSharp.Rect(startX, startY, (int)width_param, (int)height_param));//중간과정 mat이고 Rect안에 x,y,width,height 값 지정해주는거
            //        //예외처리 범위 밖으로 벗어나는경우 shift시키거나 , 제로페딩을 시키거나
            //        //예외처리                 
            //        Cv2.Resize(tempMat, previewMat, new OpenCvSharp.Size(orgMat.Width, orgMat.Height), (double)((double)orgMat.Width / (double)width_param), (double)((double)orgMat.Height / (double)height_param), InterpolationFlags.Cubic);
            //        // (double) ( (double)orgMat.Width  /  (double)width_param)
            //        // 형변환       원본이미지 형변환      /       타겟이미지 배율    == 타겟이미지가 원본이미지 대비 몇배인가? 의 수식임
            //        // (double) ( (double)orgMat.Height  /  (double)height_param)
            //        break;

            //    case "Sharpen":
            //        float filterBase = -1f;
            //        float filterCenter = filterBase * -9;
            //        float[] data = new float[9] { filterBase, filterBase, filterBase,
            //                                      filterBase, filterCenter, filterBase,
            //                                      filterBase, filterBase, filterBase };
            //        Mat kernel = new Mat(3, 3, MatType.CV_32F, data);
            //        Cv2.Filter2D(orgMat, previewMat, orgMat.Type(), kernel);
            //        break;
            //    case "IAASharpen":
            //        //  Args:
            //        //  alpha((float, float)): range to choose the visibility of the sharpened image.At 0, only the original image is
            //        //    visible, at 1.0 only its sharpened version is visible.Default: (0.2, 0.5).
            //        // lightness((float, float)): range to choose the lightness of the sharpened image.Default: (0.5, 1.0).
            //        //  p(float): probability of applying the transform.Default: 0.5.
            //        break;
            //    // Contrast Limited Adapative Histogram Equalization
            //    case "CLAHE":
            //        CLAHE test = Cv2.CreateCLAHE();
            //        test.SetClipLimit(700.0f);
            //        test.SetTilesGridSize(new OpenCvSharp.Size(4.0, 4.0));
            //        Mat normalized = new Mat();
            //        Mat temp = new Mat();
            //        orgMat.ConvertTo(temp, MatType.CV_16UC1);
            //        Cv2.CvtColor(temp, temp, ColorConversionCodes.BGR2GRAY);
            //        test.Apply(temp, previewMat);
            //        //previewMat.ConvertTo(previewMat, MatType.CV_8U);
            //        Cv2.CvtColor(previewMat, previewMat, ColorConversionCodes.GRAY2BGR);
            //        break;
            //    default:
            //        break;
            //}
            #endregion 기존 알고리즘 구현 되었던 곳
            if (previewMat.Width == 0 || previewMat.Height == 0)
                return;

            ui_PreviewImage.Source = previewMat.ToBitmapSource();

        }

        private Mat Recipe(string path, string option)
        {
            //var originalImage = new BitmapImage(new Uri(selectedImageInfo.ImageName));
            Mat orgMat = new Mat(path);
            //Mat previewMat = new Mat();

            Mat previewMat = new Mat();
            #region //Algorithm
            //ui_PreviewImage.Source = new BitmapImage(new Uri(selectedImageInfo.ImageName));
            Mat matrix;
            switch (option)
            {
                case "Contrast":
                    Cv2.AddWeighted(orgMat, _Slider.Value, orgMat, 0, 0, previewMat);
                    break;
                case "Brightness":
                    Cv2.Add(orgMat, _Slider.Value, previewMat);
                    break;
                case "Blur":
                    Cv2.GaussianBlur(orgMat, previewMat, new OpenCvSharp.Size(9, 9), _Slider.Value, 1, BorderTypes.Default);
                    break;
                case "Rotation":
                    matrix = Cv2.GetRotationMatrix2D(new Point2f(orgMat.Width / 2, orgMat.Height / 2), _Slider.Value, 1.0);
                    Cv2.WarpAffine(orgMat, previewMat, matrix, new OpenCvSharp.Size(orgMat.Width, orgMat.Height), InterpolationFlags.Cubic);
                    break;
                case "Rotation90":
                    matrix = Cv2.GetRotationMatrix2D(new Point2f(orgMat.Width / 2, orgMat.Height / 2), 90, 1.0);
                    Cv2.WarpAffine(orgMat, previewMat, matrix, new OpenCvSharp.Size(orgMat.Width, orgMat.Height), InterpolationFlags.Cubic);
                    break;
                case "Horizontal Flip":
                    Cv2.Flip(orgMat, previewMat, FlipMode.Y);
                    break;
                case "Vertical Flip":
                    Cv2.Flip(orgMat, previewMat, FlipMode.X);
                    break;
                case "Noise":
                    matrix = new Mat(orgMat.Size(), MatType.CV_8UC3);
                    Cv2.Randn(matrix, Scalar.All(0), Scalar.All(_Slider.Value));         //정규분포를 나타내는 이미지를 랜덤하게 생성
                    Cv2.AddWeighted(orgMat, 1, matrix, 1, 0, previewMat);               //두 이미지를 가중치를 설정하여 합침
                    break;
                case "Zoom In":
                    //Cv2.PyrDown(orgMat, previewMat);
                    //#1. Center만 확대
                    double width_param = (int)(0.8 * orgMat.Width); // 0.8이 배율 orgMat.Width이 원본이미지의 사이즈 // 나중에 0.8만 80%형식으로 바꿔서 파라미터로 빼야됨
                    double height_param = (int)(0.8 * orgMat.Height); // 0.8이 배율 orgMat.Height 원본이미지의 사이즈 //
                    int startX = orgMat.Width - (int)width_param;// 이미지를 Crop해올 좌상단 위치 지정하는값 // 원본사이즈 - 배율로 감소한 사이즈
                    int startY = orgMat.Height - (int)height_param;//
                    Mat tempMat = new Mat(orgMat, new OpenCvSharp.Rect(startX, startY, (int)width_param, (int)height_param));//중간과정 mat이고 Rect안에 x,y,width,height 값 지정해주는거
                    //예외처리 범위 밖으로 벗어나는경우 shift시키거나 , 제로페딩을 시키거나
                    //예외처리                 
                    Cv2.Resize(tempMat, previewMat, new OpenCvSharp.Size(orgMat.Width, orgMat.Height), (double)((double)orgMat.Width / (double)width_param), (double)((double)orgMat.Height / (double)height_param), InterpolationFlags.Cubic);
                    // (double) ( (double)orgMat.Width  /  (double)width_param)
                    // 형변환       원본이미지 형변환      /       타겟이미지 배율    == 타겟이미지가 원본이미지 대비 몇배인가? 의 수식임
                    // (double) ( (double)orgMat.Height  /  (double)height_param)
                    break;
                case "Sharpen":
                    float filterBase = -1f;
                    float filterCenter = filterBase * -9;
                    float[] data = new float[9] { filterBase, filterBase, filterBase,
                                                  filterBase, filterCenter, filterBase,
                                                  filterBase, filterBase, filterBase };
                    Mat kernel = new Mat(3, 3, MatType.CV_32F, data);
                    Cv2.Filter2D(orgMat, previewMat, orgMat.Type(), kernel);
                    break;
                // Contrast Limited Adapative Histogram Equalization
                case "CLAHE":
                    CLAHE test = Cv2.CreateCLAHE();
                    test.SetClipLimit(700.0f);
                    test.SetTilesGridSize(new OpenCvSharp.Size(4.0, 4.0));
                    Mat normalized = new Mat();
                    Mat temp = new Mat();
                    orgMat.ConvertTo(temp, MatType.CV_16UC1);
                    Cv2.CvtColor(temp, temp, ColorConversionCodes.BGR2GRAY);
                    test.Apply(temp, previewMat);
                    //previewMat.ConvertTo(previewMat, MatType.CV_8U);
                    Cv2.CvtColor(previewMat, previewMat, ColorConversionCodes.GRAY2BGR);
                    break;

                default:
                    break;
            }

            return previewMat;
        }
        #endregion

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePreviewImage();
        }

        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void McScroller_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var selectedItem = AlbumentationListBox.SelectedItem as AlbumentationInfo;
            int index = _AlbumentationInfos.IndexOf(selectedItem);

            var slider = McScroller as Slider;
            if (slider == null)
                return;

            // 예외처리 필요
            _AlbumentationInfos[index].Value = slider.Value;

            UpdatePreviewImage();
            UpdateSliderValue(slider.Value);
        }

        private void UpdateSliderValue(double value)
        {
            ValueTextBox.Text = value.ToString("F");
        }

        private void dataGrid1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ui_GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            _GeneratedAlbumentations = new ObservableCollection<AlbumentationInfo>();
            foreach (var info in _AlbumentationInfos)
            {
                if (info.IsChecked)
                {
                    if (info.TypeName == "Contrast")
                        info.Value = (info.ValueMax - info.ValueMin) * _RandomGenerator.NextDouble() + info.ValueMin;
                    else
                        info.Value = _RandomGenerator.Next((int)info.ValueMin, (int)info.ValueMax + 1);

                    _GeneratedAlbumentations.Add(info);
                }
            }
            ui_dataGridRecipe.ItemsSource = _GeneratedAlbumentations;

            //McScroller.Value = _GeneratedAlbumentations[0].Value;
        }

        private void ui_GenerateNormal_Click(object sender, RoutedEventArgs e)
        {

            var slider = McScroller as Slider;
            if (slider == null)
                return;

            _GeneratedAlbumentations = new ObservableCollection<AlbumentationInfo>();
            foreach (var info in _AlbumentationInfos)
            {
                if (info.IsChecked)
                {
                    ////if (info.TypeName == "Contrast")
                    ////    info.Value = (info.ValueMax - info.ValueMin) * _RandomGenerator.NextDouble() + info.ValueMin;
                    ////else
                    //info.Value = slider.Value;

                    _GeneratedAlbumentations.Add(info);
                }
            }
            ui_dataGridRecipe.ItemsSource = _GeneratedAlbumentations;
            _PreviousAlbumentations = _GeneratedAlbumentations.ToList().ConvertAll(o => new AlbumentationInfo() { TypeName = o.TypeName, Value = o.Value, ValueMax = o.ValueMax, ValueMin = o.ValueMin });
        }

        private void ui_GeneratePrevious_Click(object sender, RoutedEventArgs e)
        {
            ui_dataGridRecipe.ItemsSource = _PreviousAlbumentations;
        }

        private void ui_dataGridRecipe_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedRecipeItem = ui_dataGridRecipe.SelectedItem as AlbumentationInfo;
            if (selectedRecipeItem == null)
                return;

            McScroller.Value = selectedRecipeItem.Value;
        }

        private void ui_AlbumentationStart_Click(object sender, RoutedEventArgs e)
        {
            var items = ListView1.ItemsSource as List<ImageInfo>;

            foreach (AlbumentationInfo albumentation in _GeneratedAlbumentations)
            {
                // 파일 끝 인덱스 추가

                foreach (ImageInfo imageInfo in items)
                {
                    string fileName = System.IO.Path.GetFileNameWithoutExtension(imageInfo.ImageName);
                    string fileExtention = System.IO.Path.GetExtension(imageInfo.ImageName);
                    string subDirectoryName = System.IO.Path.Combine(_OutputPath, albumentation.TypeName);
                    if (!Directory.Exists(subDirectoryName))
                    {
                        System.IO.Directory.CreateDirectory(subDirectoryName);
                    }
                    string renameFile = fileName + "_" + albumentation.TypeName + fileExtention;
                    Mat previewMat = Recipe(imageInfo.ImageName, albumentation.TypeName);
                    previewMat.SaveImage(System.IO.Path.Combine(subDirectoryName, renameFile));

                }
            }
        }


        public class ImageInfo
        {
            public int ImageNumber { get; set; }
            public string ImageName { get; set; }
        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            //string text = ValueTextBox.Text;
            //string lastText = text.Substring(0, text.Length - 1);
            //if (lastText == ".")
            //{
            //    return;
            //}

            //double value = Convert.ToDouble(ValueTextBox.Text);

            //var selectedItem = AlbumentationListBox.SelectedItem as AlbumentationInfo;
            //int index = _AlbumentationInfos.IndexOf(selectedItem);

            //// 예외처리 필요
            //double valueMin = _AlbumentationInfos[index].ValueMin;
            //double valueMax = _AlbumentationInfos[index].ValueMax;

            //if (value > valueMax)
            //{
            //    ValueTextBox.Text = valueMax.ToString("F");
            //}
            //else if (value < valueMin)
            //{
            //    ValueTextBox.Text = valueMin.ToString("F");
            //}

            //McScroller.Value = value;
        }

        private void ValueTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (e.Text != ".")
                {
                    if (!char.IsDigit(c))
                    {
                        e.Handled = true;
                        break;
                    }
                }
            }
        }
        private void CheckBox_Checked_1(object sender, RoutedEventArgs e)
        {
            var tempList = _AlbumentationInfos.ToList();
            foreach (var info in tempList)
            {
                info.IsChecked = true;
            }

            _AlbumentationInfos = new List<AlbumentationInfo>(tempList);
        }
 
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var tempList = _AlbumentationInfos.ToList();
            foreach (var info in tempList)
            {
                info.IsChecked = false;
            }



            _AlbumentationInfos = new List<AlbumentationInfo>(tempList);
        }

  
    }
}


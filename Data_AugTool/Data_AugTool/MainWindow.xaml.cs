using Microsoft.Win32;
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

namespace Data_AugTool
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {


        private List<AlbumentationInfo> AlbumentationInfos = new List<AlbumentationInfo>();
        public MainWindow()
        {
            InitializeComponent();
        
            AlbumentationInfos.AddRange(new List<AlbumentationInfo>()
            {
                new AlbumentationInfo("Contrast", 0.0, 1.0),
                new AlbumentationInfo("Brightness", -100.0, 100.0),
                new AlbumentationInfo("Blur", 0, 1),
                new AlbumentationInfo("Rotation"),
                new AlbumentationInfo("Rotation90"),
                new AlbumentationInfo("Horizontal Flip"),
                new AlbumentationInfo("Vertical Flip"),
                new AlbumentationInfo("Translation"),
                new AlbumentationInfo("Noise", 0.0, 2.0),
                new AlbumentationInfo("CropResize"),
                

            }); 

            AlbumentationListBox.ItemsSource = AlbumentationInfos;
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
            List<ImageInfo> items = new List<ImageInfo>();
            for (int i = 0; i < files.Length; i++)
            {
                items.Add(new ImageInfo() { ImageNumber = i + 1, ImageName = files[i] });
            }
            ListView1.ItemsSource = items;
            Dynamic2.Source = new BitmapImage(new Uri(items[0].ImageName));
        }


        private void btnLoadFromOutput_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                BitmapImage Test = new BitmapImage();
                Test.BeginInit();
                Test.UriSource = new Uri(openFileDialog.FileName);
                Test.EndInit();
                Dynamic2.Source = Test;
            }
         

        }

        private void ListView1_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = ListView1.SelectedItem as ImageInfo;

            Dynamic2.Source = new BitmapImage(new Uri(selectedItem.ImageName));

            UpdatePreviewImage();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void AlbumentationListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePreviewImage();
        }

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

            //var originalImage = new BitmapImage(new Uri(selectedImageInfo.ImageName));
            Mat orgMat = new Mat(selectedImageInfo.ImageName);
            Mat previewMat = new Mat();

            //ui_PreviewImage.Source = new BitmapImage(new Uri(selectedImageInfo.ImageName));



            switch (listBoxSelectedItem.TypeName)
            {

                case "Contrast":
                    Cv2.AddWeighted(orgMat, listBoxSelectedItem.ValueMin, orgMat, 0, 0, previewMat);
                    break;
                case "Brightness":
                    Cv2.Add(orgMat, listBoxSelectedItem.ValueMin, previewMat);
                    break;
                case "Blur":
                    Cv2.GaussianBlur(orgMat, previewMat , new OpenCvSharp.Size(9, 9), listBoxSelectedItem.ValueMin, 1, BorderTypes.Default);
                    break;
                case "Rotation":
                    Mat matrix = Cv2.GetRotationMatrix2D(new Point2f(orgMat.Width / 2, orgMat.Height / 2), listBoxSelectedItem.ValueMin, 1.0);
                    Cv2.WarpAffine(orgMat, previewMat, matrix, new OpenCvSharp.Size(orgMat.Width, orgMat.Height));
                    break;
                case "Rotation90":
                    Cv2.Add(orgMat, listBoxSelectedItem.ValueMin, previewMat);
                    break;
                case "Horizontal Flip":
                    //Cv2.Flip(orgMat, previewMat, listBoxSelectedItem.ValueMin, previewMat, FlipMode.Y);
                    break;
                case "Vertical Flip":
                    Cv2.Add(orgMat, listBoxSelectedItem.ValueMin, previewMat);
                    break;
                case "Translation":
                    Cv2.Add(orgMat, listBoxSelectedItem.ValueMin, previewMat);
                    break;
                case "Noise":
                    Cv2.Add(orgMat, listBoxSelectedItem.ValueMin, previewMat);
                    break;
                case "CropResize":
                    Cv2.Add(orgMat, listBoxSelectedItem.ValueMin, previewMat);
                    break;

    
                default:
                    break;

            }
            ui_PreviewImage.Source = previewMat.ToBitmapSource();

        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePreviewImage();
        }
    }
    public class ImageInfo
    {
        public int ImageNumber { get; set; }
        public string ImageName { get; set; }
    }

}

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

namespace Data_AugTool
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {

        Augmentation augmentation = new Augmentation();
        private List<AlbumentationInfo> AlbumentationInfos = new List<AlbumentationInfo>();
        public MainWindow()
        {
            InitializeComponent();
            var contrast = new AlbumentationInfo()
            {
                IsChecked = false,
                IsUseValueMax = true,
                IsUseValueMin = true,
                TypeName = "Contrast",
                ValueMin = 0.0,
                ValueMax = 1.0
            };
            var brightness = new AlbumentationInfo()
            {
                IsChecked = false,
                IsUseValueMax = true,
                IsUseValueMin = true,
                TypeName = "Brightness",
                ValueMin = -100.0,
                ValueMax = 100.0
            };
            AlbumentationInfos.Add(contrast);
            AlbumentationInfos.Add(brightness);
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
                //Uri fileUri = new Uri(openFileDialog.FileName);
                Dynamic2.Source = Test;
            }
            double a = 0; ;
            Mat mat = new Mat();
            Mat outputMat;

            outputMat = augmentation.GetRotationImage(mat, a);


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

            var selectedImageInfo = ListView1.SelectedItem as ImageInfo;
            if (selectedImageInfo == null)
                return;

            //var originalImage = new BitmapImage(new Uri(selectedImageInfo.ImageName));
            Mat orgMat = new Mat(selectedImageInfo.ImageName);
            Mat previewMat = new Mat();

            //ui_PreviewImage.Source = new BitmapImage(new Uri(selectedImageInfo.ImageName));
            switch (listBoxSelectedItem.TypeName)
            {
                case "Brightness":
                    Cv2.Add(orgMat, listBoxSelectedItem.ValueMin, previewMat);
                    break;
                case "Contrast":
                    Cv2.AddWeighted(orgMat, listBoxSelectedItem.ValueMin, orgMat, 0, 0, previewMat);
                    break;
                default:
                    break;
            }
            ui_PreviewImage.Source = previewMat.ToBitmapSource();
        }
    }
    public class ImageInfo
    {
        public int ImageNumber { get; set; }
        public string ImageName { get; set; }
    }

}

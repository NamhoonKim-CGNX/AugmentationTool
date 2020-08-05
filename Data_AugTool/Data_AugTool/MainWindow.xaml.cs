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
using System.Windows.Media.TextFormatting;
using System.Net;
using MahApps.Metro.Controls;
using System.Web.UI;
using System.Security.Cryptography.X509Certificates;

namespace Data_AugTool
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
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
                new AlbumentationInfo("Noise", 0.0, 2.0),
                new AlbumentationInfo("Zoom In"),
                new AlbumentationInfo("Sharpen"),
                new AlbumentationInfo("Gradation"),
                new AlbumentationInfo("RandomBrightnessContrast"),
                new AlbumentationInfo("IAASharpen"),
                new AlbumentationInfo("CLAHE"),

            });
            foreach (var item in AlbumentationInfos)
            {
                item.IsChecked= true;
            }
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
            Mat matrix;
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
                    matrix = Cv2.GetRotationMatrix2D(new Point2f(orgMat.Width / 2, orgMat.Height / 2), listBoxSelectedItem.ValueMin, 1.0);
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
                    matrix = new Mat(orgMat.Size() ,MatType.CV_8UC3);
                    Cv2.Randn(matrix, Scalar.All(0), Scalar.All(50));         //정규분포를 나타내는 이미지를 랜덤하게 생성
                    Cv2.AddWeighted(orgMat, 1, matrix, 1, 0, previewMat);     //두 이미지를 가중치를 설정하여 합침
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
                    
                    Cv2.Resize(tempMat, previewMat, new OpenCvSharp.Size(orgMat.Width, orgMat.Height),(double)((double)orgMat.Width/(double)width_param), (double)((double)orgMat.Height/(double)height_param), InterpolationFlags.Cubic);
                    // (double) ( (double)orgMat.Width  /  (double)width_param)
                    // 형변환       원본이미지 형변환      /       타겟이미지 배율    == 타겟이미지가 원본이미지 대비 몇배인가? 의 수식임
                    // (double) ( (double)orgMat.Height  /  (double)height_param)

                    //#2. 랜덤좌표
                    //#3. 지정좌표
                    break;

                case "Sharpen":
                    float[] data = new float[9] { -1, -1, -1, -1, 9, -1, -1, -1, -1 };
                    Mat kernel = new Mat(3, 3, MatType.CV_8S, data);
                    Cv2.AddWeighted(orgMat, 1.5, kernel, -0.9, 0, previewMat);              
                    break;

                case "Gradation":
                    
                    break;

 
                case "Random Brightness Contrast":


                    break;

                case "CLAHE":


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
    }




public class ImageInfo
    {
        public int ImageNumber { get; set; }
        public string ImageName { get; set; }
    }


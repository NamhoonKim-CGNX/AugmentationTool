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


namespace Data_AugTool
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();

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

        }

        private void ListView1_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = ListView1.SelectedItem as ImageInfo;

            Dynamic2.Source = new BitmapImage(new Uri(selectedItem.ImageName));


        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
    public class ImageInfo
    {
        public int ImageNumber { get; set; }
        public string ImageName { get; set; }
    }

}

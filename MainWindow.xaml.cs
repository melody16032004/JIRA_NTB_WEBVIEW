using Microsoft.Web.WebView2.Core;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JIRA_NTB_WEBVIEW
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeWebView();

            this.KeyDown += MainWindow_KeyDown;
        }

        // Xử lý phím F5
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                RefreshPage();
            }
        }

        private async void InitializeWebView()
        {
            try
            {
                await webView.EnsureCoreWebView2Async(null);

                // Đăng ký các sự kiện cần thiết
                webView.NavigationStarting += WebView_NavigationStarting;
                webView.NavigationCompleted += WebView_NavigationCompleted;

                // Tắt Context Menu mặc định (chống click chuột phải -> View Source)
                webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                // (Tùy chọn) Tắt các phím tắt trình duyệt (F5, Ctrl+P, v.v.)
                webView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;


                // TODO: Thay đổi URL này thành địa chỉ web app quản lý nhân sự của bạn
                string targetUrl = "https://localhost:7132/";

                webView.Source = new Uri(targetUrl);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể khởi tạo WebView2: {ex.Message}\n\nVui lòng đảm bảo bạn đã cài đặt WebView2 Runtime.", "Lỗi nghiêm trọng", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region WebView Event Handlers

        private void WebView_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            // Hiển thị chỉ báo tải trang
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                // Trang tải thành công -> ẩn loading
                loadingIndicator.Visibility = Visibility.Collapsed;
                errorPage.Visibility = Visibility.Collapsed;
                webView.Visibility = Visibility.Visible;
            }
            else
            {
                //// Nếu trang không tải được, giữ hiển thị loading hoặc show thông báo trong UI
                //loadingIndicator.Visibility = Visibility.Visible;

                //// (Tùy chọn) Thêm TextBlock thông báo lỗi nhẹ nhàng
                //if (loadingIndicator.Children.Count > 0 && loadingIndicator.Children[0] is StackPanel sp && sp.Children.Count > 1 && sp.Children[1] is TextBlock tb)
                //{
                //    tb.Text = "Không thể kết nối tới server. Đang thử lại...";
                //}

                //// Tự động thử reload sau vài giây (tùy chọn)
                //Task.Delay(5000).ContinueWith(_ =>
                //{
                //    Dispatcher.Invoke(() =>
                //    {
                //        if (webView?.CoreWebView2 != null)
                //            webView.Reload();
                //    });
                //});
                // Ẩn webview & loading, hiển thị trang lỗi
                loadingIndicator.Visibility = Visibility.Collapsed;
                webView.Visibility = Visibility.Collapsed;
                errorPage.Visibility = Visibility.Visible;
            }
        }

        #endregion

        #region Custom Title Bar Handlers

        // Cho phép kéo thả cửa sổ
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Maximized;
                btnMaximize.Content = "\uE923"; // Icon Restore
            }
            else
            {
                this.WindowState = WindowState.Normal;
                btnMaximize.Content = "\uE922"; // Icon Maximize
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // Hàm refresh trang (dùng chung cho nút và F5)
        private void RefreshPage()
        {
            if (webView?.CoreWebView2 != null)
            {
                loadingIndicator.Visibility = Visibility.Visible; // hiển thị loading
                webView.Reload();
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshPage();
        }

        private void ReopenButton_Click(object sender, RoutedEventArgs e)
        {
            // Lấy đường dẫn exe hiện tại
            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            try
            {
                // Mở lại ứng dụng
                System.Diagnostics.Process.Start(exePath);

                // Đóng ứng dụng hiện tại
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể mở lại ứng dụng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RetryConnection_Click(object sender, RoutedEventArgs e)
        {
            errorPage.Visibility = Visibility.Collapsed;
            loadingIndicator.Visibility = Visibility.Visible;

            try
            {
                if (webView?.CoreWebView2 != null)
                {
                    webView.Reload();
                }
                else
                {
                    InitializeWebView();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thử kết nối lại: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}
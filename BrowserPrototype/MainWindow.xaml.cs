using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BrowserPrototype
{

    public partial class MainWindow : Window
    {
        // Окно браузера

        // Регистрирует новое универсальное свойство (в данном случае - свойство Html (что бы это ни значило))
        private static readonly DependencyProperty HtmlProperty = DependencyProperty.RegisterAttached( 
            "Html",                                         // Имя свойства
            typeof(string),                                 // Тип свойства
            typeof(MainWindow),                             // На какой объект распространяется свойство (владелец)
            new FrameworkPropertyMetadata(OnHtmlChanged));  // что будет отрабатываться при изменении свойства

        public MainWindow()
        {
            InitializeComponent();
        }

        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            //===================================================================================================
            // Выполняется в главном потоке, Id потока - 1

            //Debug.WriteLine($"Thread  Nr {Thread.CurrentThread.ManagedThreadId}");
            //HttpClient httpClient = new HttpClient();
            ////string html = httpClient.GetStringAsync("https://google.com").Result; // маленький файл (всего 4кб)

            //string htmlBig = httpClient.GetStringAsync("http://speedtest.ftp.otenet.gr/files/test10Mb.db").Result; // большой файл (целых 10мб)
            //myButton.Content = "Done";

            //MessageBox.Show(htmlBig.Length.ToString());

            //===================================================================================================
            // Выполнение в отдельном потоке, Id = 3, тк на основной приходятся 2 потока
            Task.Run(() =>
            {
                Debug.WriteLine($"Thread  Nr { Thread.CurrentThread.ManagedThreadId}");
                HttpClient httpClientTask = new HttpClient();
                //string html = httpClient.GetStringAsync("https://google.com").Result; // маленький файл (всего 4кб)

                string htmlBigTask = httpClientTask.GetStringAsync("http://speedtest.ftp.otenet.gr/files/test10Mb.db").Result; // большой файл (целых 10мб)

                //myButton.Content = "Done";
                myButton.Dispatcher.Invoke(() => // поскольку за UI отвечает главный поток, то вызывает выполнение операций в основном потоке
                {
                    Debug.WriteLine($"Thread  Nr {Thread.CurrentThread.ManagedThreadId}");
                    myButton.Content = "Done";
                });

                MessageBox.Show(htmlBigTask.Length.ToString());
            });
        }

        private async void myButton_Click2(object sender, RoutedEventArgs e) // не такой накрученный вариант с асинхронным task-ом и изменением UI в главном потоке
        {
            string html = string.Empty;
            Debug.WriteLine($"Thread  Nr {Thread.CurrentThread.ManagedThreadId}");
            await Task.Run(() => 
            {
                Debug.WriteLine($"Thread  Nr {Thread.CurrentThread.ManagedThreadId}");
                HttpClient httpClientTask = new HttpClient();
                html = httpClientTask.GetStringAsync("https://google.com").Result; // "http://speedtest.ftp.otenet.gr/files/test10Mb.db"
            });
            
            myButton2.Content = "Done Downloading";
            browserWindow.SetValue(HtmlProperty, html); // по сути просто закидываем значение в свойство зависимого объекта(условно любой элемент, объект) через метод (только как оно, являясь общим, принадлежит браузеру понять сложно)
            
        }

        // Обработчик изменеyия свойства Html 
        private static void OnHtmlChanged (DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            WebBrowser webBrowser = dependencyObject as WebBrowser;
            if (webBrowser != null) webBrowser.NavigateToString(dependencyPropertyChangedEventArgs.NewValue as string); // закидываем новое значение свойства в браузер
        }
    }
}

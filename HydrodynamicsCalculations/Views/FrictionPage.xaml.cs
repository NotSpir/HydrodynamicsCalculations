using HydrodynamicsCalculations.Windows;
using iText.Html2pdf;
using ScottPlot;
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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MessageBox = System.Windows.MessageBox;

namespace HydrodynamicsCalculations.Views
{
    /// <summary>
    /// Логика взаимодействия для FrictionPage.xaml
    /// </summary>
    public partial class FrictionPage : Page
    {
        List<double> xLiquidCords = new List<double>();
        List<double> yLiquidCords = new List<double>();
        List<string> dataStrings = new List<string>();
        string directory = System.IO.Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
        public FrictionPage()
        {
            InitializeComponent();

            WpfPlot1.Plot.XLabel("Position");
            WpfPlot1.Plot.YLabel("Skin Friction Coefficient");
        }

        private void GetFileBtn_Click(object sender, RoutedEventArgs e)
        {
            AppManager.GetFileToGraph(WpfPlot1, ref xLiquidCords, ref yLiquidCords);
            if (xLiquidCords.Count >= 1)
            {
                SillyBtn.IsEnabled = true;
            }
            else
            {
                SillyBtn.IsEnabled = false;
            }
        }

        private void SillyBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dataStrings.Clear();
                AppManager.isThreeReady = false;
                AppManager.thirdTaskDataStrings.Clear();
                //Расчет Cf
                double Cf = 0;
                int cordCount = 0;
                for (int i = 0; i < xLiquidCords.Count; i++)
                {
                    if (xLiquidCords[i] > AppManager.LStart)
                    {
                        cordCount++;
                        Cf += yLiquidCords[i];
                    }
                }
                Cf = Cf / cordCount;
                CfValueTextBox.Text = Cf.ToString();

                dataStrings.Add(Cf.ToString());
                AppManager.isThreeReady = true;
                AppManager.thirdTaskDataStrings = dataStrings;
                //Получение графика в форме изображения
                string imageFileName = directory + "\\cfgraph.png";
                WpfPlot1.Plot.SaveFig(imageFileName);

                PdfBtn.IsEnabled = true;
            }
            catch
            {
                MessageBox.Show("Что-то пошло не так. Проверьте корректность введенных данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            AppManager.mainWindow.CheckCompletionStatus();
        }

        private void GetPDFFileBtn_Click(object sender, RoutedEventArgs e)
        {
            //Создание HTML страницы
            string HTMLReportVersion = @"<html lang=""en"">
<body>
    <style>
        body {
            font-size: 16pt;
        }
    </style>
    <h3>Задание 3</h3>
    <p>Построить график зависимости коэффициента трения на стенке канала, Cf, от координаты x. Проанализировать распределение коэффициента трения и убедиться в том, что для участка развитого течения выполняется соотношение  = 2 Cf.
    </p>
    <h4>Графические данные: </h4>
    <p><b>Зависимость коэффициента трения на стенке канала, Cf, от координаты:</b></p>
    <img src=""cfgraph.png"" />
    " + $@"<p>C<font style=""font-size: 12pt;"">f</font> для участка развитого течения: <b>{dataStrings[0]}?</b></p>
</body>
</html>";

            string fileName = directory + @"\PDFStructuralFile.html";

            try
            {

                // Check if file already exists. If yes, delete it.
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
                File.WriteAllText(fileName, HTMLReportVersion);
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.ToString());
            }

            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();
                string fullResult = fbd.SelectedPath + "\\ОтчетЗадание3.pdf";

                // Process open file dialog box results
                if ((result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath)))
                {
                    var converterProperties = new ConverterProperties();
                    HtmlConverter.ConvertToPdf(
                        new FileInfo(fileName),
                        new FileInfo(fullResult)
                        );
                    MessageBox.Show("Файл успешно создан");
                }
                else MessageBox.Show("Что-то пошло не так!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TaskViewBtn_Click(object sender, RoutedEventArgs e)
        {
            TaskViewWindow taskViewWindow = new TaskViewWindow();
            taskViewWindow.TaskConditionsTextBox.Text = "Построить график зависимости коэффициента трения на стенке канала, Cf, от координаты x. Проанализировать распределение коэффициента трения и убедиться в том, что для участка развитого течения выполняется соотношение  = 2 Cf.";
            taskViewWindow.TaskResultsTextBox.Text = "Определить  Cf для участка развитого течения";
            taskViewWindow.Show();
        }
    }
}

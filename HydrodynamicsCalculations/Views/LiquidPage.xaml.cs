using iText.Html2pdf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

namespace HydrodynamicsCalculations.Views
{
    /// <summary>
    /// Логика взаимодействия для LiquidPage.xaml
    /// </summary>
    public partial class LiquidPage : Page
    {
        string directory = System.IO.Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
        List<double> xLiquidCords = new List<double>();
        List<double> yLiquidCords = new List<double>();
        List<double> xHSpeedCords = new List<double>();
        List<double> yHSpeedCords = new List<double>();
        List<double> x19HSpeedCords = new List<double>();
        List<double> y19HSpeedCords = new List<double>();
        List<DataObject> listFirstTable = new List<DataObject>();
        List<DataObject> listSecondTable = new List<DataObject>();
        List<string> dataStrings = new List<string>();

        private class DataObject
        {
            public string A { get; set; }
            public string B { get; set; }
            public string C { get; set; }
            public string D { get; set; }
            public string E { get; set; }
        }

        public LiquidPage()
        {
            InitializeComponent();

            WpfPlot1.Plot.XLabel("Position");
            WpfPlot1.Plot.YLabel("Static Pressure");
            WpfPlot2.Plot.XLabel("Position");
            WpfPlot2.Plot.YLabel("X Velocity");
            WpfPlot3.Plot.XLabel("Position");
            WpfPlot3.Plot.YLabel("X Velocity");

            listFirstTable.Add(new DataObject() { A = "", B = "Начальный участок", C = "Весь канал", D = "Развитое течение", E = "λ = 24/Re" });
            listFirstTable.Add(new DataObject() { A = "ΔL", B = "", C = "", D = "", E = "" });
            listFirstTable.Add(new DataObject() { A = "ΔP", B = "", C = "", D = "", E = "" });
            listFirstTable.Add(new DataObject() { A = "λ", B = "", C = "", D = "", E = "" });
            this.dataGrid1.ItemsSource = listFirstTable;

            listSecondTable.Add(new DataObject() { A = "Re", B = "Участок", C = "λ = (Δp * H * 2) / (ΔL * p * Vср)", D = "λ по углу наклона", E = "λ = 24/Re" });
            listSecondTable.Add(new DataObject() { A = "", B = "", C = "", D = "", E = "" });
            listSecondTable.Add(new DataObject() { A = "", B = "", C = "", D = "", E = "" });
            listSecondTable.Add(new DataObject() { A = "", B = "", C = "", D = "", E = "" });
            this.dataGrid2.ItemsSource = listSecondTable;
        }


        Regex regex = new Regex(@"^[0-9]");
        private void GetFileBtn_Click(object sender, RoutedEventArgs e)
        {
            AppManager.GetFileToGraph(WpfPlot1, ref xLiquidCords, ref yLiquidCords);
            if (LiquidNumberText.Text != "" && xLiquidCords.Count >= 1 && xHSpeedCords.Count >= 1 && x19HSpeedCords.Count >= 1)
            {
                SillyBtn.IsEnabled = true;
            }
            else
            {
                SillyBtn.IsEnabled = false;
            }
        }

        private void GetSecondFileBtn_Click(object sender, RoutedEventArgs e)
        {
            AppManager.GetFileToGraph(WpfPlot2, ref xHSpeedCords, ref yHSpeedCords);
            if (LiquidNumberText.Text != "" && xLiquidCords.Count >= 1 && xHSpeedCords.Count >= 1 && x19HSpeedCords.Count >= 1)
            {
                SillyBtn.IsEnabled = true;
            }
            else
            {
                SillyBtn.IsEnabled = false;
            }
        }

        private void GetThirdFileBtn_Click(object sender, RoutedEventArgs e)
        {
            AppManager.GetFileToGraph(WpfPlot3, ref x19HSpeedCords, ref y19HSpeedCords);
            if (LiquidNumberText.Text != "" && xLiquidCords.Count >= 1 && xHSpeedCords.Count >= 1 && x19HSpeedCords.Count >= 1)
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
                AppManager.isTwoReady = false;
                AppManager.secondTaskDataStrings.Clear();
                WpfPlot1.Plot.Clear();
                WpfPlot1.Plot.AddScatter(xLiquidCords.ToArray(), yLiquidCords.ToArray(), System.Drawing.Color.Black, 1, 3);
                double[] xCordLstart = {AppManager.LStart, AppManager.LStart};
                double[] yCordLstart = { yLiquidCords.Max(), 0};
                WpfPlot1.Plot.AddScatter(xCordLstart, yCordLstart, System.Drawing.Color.Red, 1, 3, ScottPlot.MarkerShape.filledCircle, ScottPlot.LineStyle.Dash, "Разделение начального участка");
                WpfPlot1.Plot.Legend();
                WpfPlot1.Refresh();
                double durability = Convert.ToDouble(LiquidNumberText.Text);
                double H = AppManager.H;
                double Lstart = AppManager.LStart;
                PdfBtn.IsEnabled = false;
                //1-й пункт | определить среднее значение скорости в в сечениях х =Н (использовать для начального участка) и х = 19Н (использовать для развитого течения и канала в целом L = 20*H)
                dataStrings.Add(LiquidNumberText.Text);
            //Вычисление средней x=H
            double avgSpeedH = 0;
            for (int i = 0; i < xHSpeedCords.Count; i++)
            {
                avgSpeedH += xHSpeedCords[i];
            }
            avgSpeedH = avgSpeedH / xHSpeedCords.Count;
                //Вычисление средней x=19H
            double avgSpeed19H = 0;
            for (int i = 0; i < x19HSpeedCords.Count; i++)
            {
                avgSpeed19H += x19HSpeedCords[i];
            }
            avgSpeed19H = avgSpeed19H / x19HSpeedCords.Count;
                //Вывод данных
            Average19HSpeedTextBox.Text = avgSpeed19H.ToString();
            AverageHSpeedTextBox.Text = avgSpeedH.ToString();
            dataStrings.Add(AverageHSpeedTextBox.Text);
            dataStrings.Add(Average19HSpeedTextBox.Text);
            AppManager.secondTaskDataStrings = dataStrings;
                //2-й пункт | Последовательно заполнить таблицу для каждого из трех чисел Re:
                //Заполнение строки DeltaL
            double L = 20;
            double DevelopedL = L - Lstart;
            listFirstTable[1].B = Lstart.ToString();
            listFirstTable[1].C = L.ToString();
            listFirstTable[1].D = DevelopedL.ToString();
            listFirstTable[1].E = (24.0/AppManager.ReNum1).ToString();
                //Заполнение строки Delta p
            double sectionLengthForGraph = xLiquidCords.Max();
            sectionLengthForGraph = Lstart;
            double firstCord = 0;
            double secondCord = 0;
            double multiplier = 0;
            double xLiquidCordFirst = 0;
            for (int i = 0; i < xLiquidCords.Count; i++)
            {
                if (xLiquidCords[i] < sectionLengthForGraph)
                {
                    firstCord = yLiquidCords[i];
                    xLiquidCordFirst = xLiquidCords[i];
                }
                else if (sectionLengthForGraph <= xLiquidCords[i])
                {
                    secondCord = yLiquidCords[i];
                    multiplier = (sectionLengthForGraph - xLiquidCordFirst) / (xLiquidCords[i] - xLiquidCordFirst);
                    break;
                }
            }
            double toAddToLessVal = (secondCord - firstCord) * multiplier;
            double DeltaPStartSection = firstCord + toAddToLessVal;
            double DeltaPZero = yLiquidCords.First() - DeltaPStartSection;
            double DeltaPWhole = yLiquidCords.First() - yLiquidCords.Last();
            double DeltaPDeveloped = DeltaPStartSection - yLiquidCords.Last();
            listFirstTable[2].B = DeltaPZero.ToString();
            listFirstTable[2].C = DeltaPWhole.ToString();
            listFirstTable[2].D = DeltaPDeveloped.ToString();
            listFirstTable[2].E = (24.0 / AppManager.ReNum2).ToString();
                //Заполнение строки Lambda ???
                double lambdaStart = (DeltaPZero * H * 2) / (Lstart * durability * avgSpeedH);
                double lambdaFull = (DeltaPWhole * H * 2) / (L * durability * ((avgSpeed19H + avgSpeedH)/2));
                double lambdaDeveloped = (DeltaPDeveloped * H * 2) / (DevelopedL * durability * avgSpeedH);
                listFirstTable[3].B = lambdaStart.ToString();
                listFirstTable[3].C = lambdaFull.ToString();
                listFirstTable[3].D = lambdaDeveloped.ToString();
                listFirstTable[3].E = (24.0 / AppManager.ReNum3).ToString();
                //asd
                AppManager.secondTaskDataStrings.Add(listFirstTable[1].A);
            AppManager.secondTaskDataStrings.Add(listFirstTable[1].B);
            AppManager.secondTaskDataStrings.Add(listFirstTable[1].C);
            AppManager.secondTaskDataStrings.Add(listFirstTable[1].D);
            AppManager.secondTaskDataStrings.Add(listFirstTable[1].E);
            AppManager.secondTaskDataStrings.Add(listFirstTable[2].A);
            AppManager.secondTaskDataStrings.Add(listFirstTable[2].B);
            AppManager.secondTaskDataStrings.Add(listFirstTable[2].C);
            AppManager.secondTaskDataStrings.Add(listFirstTable[2].D);
            AppManager.secondTaskDataStrings.Add(listFirstTable[2].E);
            AppManager.secondTaskDataStrings.Add(listFirstTable[3].A);
            AppManager.secondTaskDataStrings.Add(listFirstTable[3].B);
            AppManager.secondTaskDataStrings.Add(listFirstTable[3].C);
            AppManager.secondTaskDataStrings.Add(listFirstTable[3].D);
            AppManager.secondTaskDataStrings.Add(listFirstTable[3].E);

            dataGrid1.Items.Refresh();


                //3-й пункт | Определить значение (Лямбда) для участка развитого течения из угла наклона линейного участка зависимости.
                //По графику перепада давления для каждого из участков (начальный, развитое и канал целиком) определить угол наклона.
                double angleDeveloped = Math.Abs((DeltaPStartSection - yLiquidCords.Last()) / (Lstart - xLiquidCords.Last()));
                AngleDevTextBox.Text = angleDeveloped.ToString();
                //Найди формулу для лямбды и задание сделано
                double lambdaFromAngle = angleDeveloped;
                dataStrings.Add(lambdaFromAngle.ToString());
                AppManager.secondTaskDataStrings.Add(lambdaFromAngle.ToString());
                //Пункт 4 | Провести сопоставление с теоретической оценкой для развитого участка (Лямбда) = 24/Re; представить результаты в виде таблицы и графически.
                listSecondTable[1].A = AppManager.ReNum1.ToString();
                listSecondTable[2].A = AppManager.ReNum2.ToString();
                listSecondTable[3].A = AppManager.ReNum3.ToString();
                listSecondTable[1].B = DevelopedL.ToString();
                listSecondTable[2].B = DevelopedL.ToString();
                listSecondTable[3].B = DevelopedL.ToString();
                listSecondTable[1].C = lambdaDeveloped.ToString();
                listSecondTable[2].C = lambdaDeveloped.ToString();
                listSecondTable[3].C = lambdaDeveloped.ToString();
                listSecondTable[1].D = lambdaFromAngle.ToString();
                listSecondTable[2].D = lambdaFromAngle.ToString();
                listSecondTable[3].D = lambdaFromAngle.ToString();
                listSecondTable[1].E = (24.0 / AppManager.ReNum1).ToString();
                listSecondTable[2].E = (24.0 / AppManager.ReNum2).ToString();
                listSecondTable[3].E = (24.0 / AppManager.ReNum3).ToString();
                dataGrid2.Items.Refresh();
                AppManager.secondTaskDataStrings.Add(listSecondTable[1].A);
                AppManager.secondTaskDataStrings.Add(listSecondTable[1].B);
                AppManager.secondTaskDataStrings.Add(listSecondTable[1].C);
                AppManager.secondTaskDataStrings.Add(listSecondTable[1].D);
                AppManager.secondTaskDataStrings.Add(listSecondTable[1].E);
                AppManager.secondTaskDataStrings.Add(listSecondTable[2].A);
                AppManager.secondTaskDataStrings.Add(listSecondTable[2].B);
                AppManager.secondTaskDataStrings.Add(listSecondTable[2].C);
                AppManager.secondTaskDataStrings.Add(listSecondTable[2].D);
                AppManager.secondTaskDataStrings.Add(listSecondTable[2].E);
                AppManager.secondTaskDataStrings.Add(listSecondTable[3].A);
                AppManager.secondTaskDataStrings.Add(listSecondTable[3].B);
                AppManager.secondTaskDataStrings.Add(listSecondTable[3].C);
                AppManager.secondTaskDataStrings.Add(listSecondTable[3].D);
                AppManager.secondTaskDataStrings.Add(listSecondTable[3].E);
                //Получение графика в форме изображения
                string imageFileName = directory + "\\graphimage1.png";
                WpfPlot1.Plot.SaveFig(imageFileName);
                imageFileName = directory + "\\graphimage2.png";
                WpfPlot2.Plot.SaveFig(imageFileName);
                imageFileName = directory + "\\graphimage3.png";
                WpfPlot3.Plot.SaveFig(imageFileName);
                AppManager.isTwoReady = true;
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
        table {
            border-color: black;
            border-width: 1px;
            border-radius: 5px;
            align-items: center;
            align-self: center;
            align-content: center;
            border-color: black;
            border-style: solid;  
        }
        td {
            padding: 10px;
            border-color: black;
            border-style: solid; 
            border-width: 1px; 
        }
    </style>
    <h3>Задание 2</h3>
    <p>Для каждого из трех чисел Re:
        Проанализировать поле давления; найти коэффициент сопротивления λ для начального участка, участка с установившимся параболическим профилем скорости и канала в целом при помощи формулы: </p>
        <p>
        Δp = λ * (ΔL/H) * ((ρV<font style=""font-size: 12pt;"">ср</font>^2)/2)</p>
    <p>где Δp – перепад давления на участке канала длиной ΔL, ρ – плотность жидкости, Vср – средняя по сечению скорость. Определить значение λ для участка развитого течения из угла наклона линейного участка зависимости. Провести сопоставление с теоретической оценкой для развитого участка λ = 24/Re; представить результаты в виде таблицы и графически.</p>
        " + $@"
    </p>
    <p><b>Входные данные:</b> p = {dataStrings[0]}</p>
    <p>H = {AppManager.H}</p>
    <p>L<font style=""font-size: 12pt;"">нач</font> = {AppManager.LStart}</p>
    <p><b>Числа Re:</b> Re<font style=""font-size: 12pt;"">1</font> = {AppManager.ReNum1}; Re<font style=""font-size: 12pt;"">2</font> = {AppManager.ReNum2}; Re<font style=""font-size: 12pt;"">3</font> = {AppManager.ReNum3}</p>
    <h4>Графические данные: </h4>
    <p><b>Значение давления р в центральном сечении канала:</b></p>
    <img src=""graphimage1.png"" />
    <p><b>Скорость в сечении х =Н:</b></p>
    <img src=""graphimage2.png"" />
    <p><b>Скорость в сечении х = 19Н:</b></p>
    <img src=""graphimage3.png"" />

    <p>V<font style=""font-size: 12pt;"">ср</font> в сечении x=H: {dataStrings[1]}</p>
    <p>V<font style=""font-size: 12pt;"">ср</font> в сечении x=19H: {dataStrings[2]}</p>
    <table>
        <tr>
            <td></td>
            <td>Начальный участок</td>
            <td>Развитое течение</td>
            <td>Весь канал</td>
            <td>λ = 24/Re </td>
        </tr>
        <tr>
            <td>{listFirstTable[1].A}</td>
            <td>{listFirstTable[1].B}</td>
            <td>{listFirstTable[1].D}</td>
            <td>{listFirstTable[1].C}</td>
            <td>{listFirstTable[1].E}</td>
        </tr>
        <tr>
            <td>{listFirstTable[2].A}</td>
            <td>{listFirstTable[2].B}</td>
            <td>{listFirstTable[2].D}</td>
            <td>{listFirstTable[2].C}</td>
            <td>{listFirstTable[2].E}</td>
        </tr>
        <tr>
            <td>{listFirstTable[3].A}</td>
            <td>{listFirstTable[3].B}</td>
            <td>{listFirstTable[3].D}</td>
            <td>{listFirstTable[3].C}</td>
            <td>{listFirstTable[3].E}</td>
        </tr>
    </table>

    <p>Угол наклона для учатска развитого течения: {AppManager.secondTaskDataStrings[18]}</p>
    <br/>
    <table>
        <tr>
            <td>Re</td>
            <td>Участок</td>
            <td>λ = (Δp * H * 2) / (ΔL * p * Vср)</td>
            <td>λ по углу наклона</td>
            <td>λ = 24/Re </td>
        </tr>
        <tr>
            <td>{listSecondTable[1].A}</td>
            <td>{listSecondTable[1].B}</td>
            <td>{listSecondTable[1].C}</td>
            <td>{listSecondTable[1].D}</td>
            <td>{listSecondTable[1].E}</td>
        </tr>
        <tr>
            <td>{listSecondTable[2].A}</td>
            <td>{listSecondTable[2].B}</td>
            <td>{listSecondTable[2].C}</td>
            <td>{listSecondTable[2].D}</td>
            <td>{listSecondTable[2].E}</td>
        </tr>
        <tr>
            <td>{listSecondTable[3].A}</td>
            <td>{listSecondTable[3].B}</td>
            <td>{listSecondTable[3].C}</td>
            <td>{listSecondTable[3].D}</td>
            <td>{listSecondTable[3].E}</td>
        </tr>
    </table>
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
                string fullResult = fbd.SelectedPath + "\\ОтчетЗадание2.pdf";

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

        private void scrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
        }

        private void CheckCorrectTextInput(object sender, TextCompositionEventArgs e)
        {
            AppManager.CheckTextIsNumber(sender, e);
        }
        private void TextBoxNumsPasting(object sender, DataObjectPastingEventArgs e)
        {
            AppManager.CheckPastedTextIsNumber(sender, e);
        }
        private void TextBoxData_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (LiquidNumberText.Text != "" && xLiquidCords.Count >= 1 && xHSpeedCords.Count >= 1 && x19HSpeedCords.Count >= 1)
            {
                SillyBtn.IsEnabled = true;
            }
            else
            {
                SillyBtn.IsEnabled = false;
            }
        }
        private void BlockSpaceForTextBox(object sender, KeyEventArgs e)
        {
            e.Handled = e.Key == Key.Space;
        }
    }
}

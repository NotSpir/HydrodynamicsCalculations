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
using static System.Net.Mime.MediaTypeNames;
using System.IO;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using ScottPlot;
using System.Windows.Markup;
using iText.Html2pdf;
using iText;
using static iText.Svg.SvgConstants;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using iText.Html2pdf.Resolver.Font;
using TextBox = System.Windows.Controls.TextBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using HydrodynamicsCalculations.Windows;

namespace HydrodynamicsCalculations.Views
{
    /// <summary>
    /// Логика взаимодействия для SpeedPage.xaml
    /// </summary>
    public partial class SpeedPage : Page
    {
        List<double> xLiquidCords = new List<double>();
        List<double> yLiquidCords = new List<double>();
        List<DataObject> list = new List<DataObject>();
        List<string> dataStrings = new List<string>();
        bool IsThereAPointToTheorysLife = false;
        string directory = System.IO.Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
        private class DataObject
        {
            public string A { get; set; }
            public string B { get; set; }
            public string C { get; set; }
            public string D { get; set; }
        }

        public SpeedPage()
        {
            InitializeComponent();

            WpfPlot1.Plot.XLabel("Position");
            WpfPlot1.Plot.YLabel("Velocity Magnitude");

            list.Add(new DataObject() { A = "Число Re", B = "95% Vmax", C = "98% Vmax", D = "Lнач= (0.04*Re)* H"});
            list.Add(new DataObject() { A = Re1NumberText.Text, B = "", C = "", D = ""});
            list.Add(new DataObject() { A = Re2NumberText.Text, B = "", C = "", D = ""});
            list.Add(new DataObject() { A = Re3NumberText.Text, B = "", C = "", D = ""});
            this.dataGrid1.ItemsSource = list;
        }


        Regex regex = new Regex(@"^[0-9]");
        private void GetFileBtn_Click(object sender, RoutedEventArgs e)
        {
            AppManager.GetFileToGraph(WpfPlot1, ref xLiquidCords, ref yLiquidCords);
            if (Re1NumberText.Text != "" && Re2NumberText.Text != "" && Re3NumberText.Text != ""
                && VNumberText.Text != "" && HNumberText.Text != "" && xLiquidCords.Count >= 1)
            {
                SillyBtn.IsEnabled = true;
            }
            else
            {
                SillyBtn.IsEnabled = false;
            }
        }

        //Кнопка выполнения вычислений
        private void SillyBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Восстановление всех параметров
                WpfPlot1.Plot.Clear();
                WpfPlot1.Plot.AddScatter(xLiquidCords.ToArray(), yLiquidCords.ToArray(), System.Drawing.Color.Black, 1, 3);
                dataStrings.Clear();
                AppManager.IsThereATheoryValue = false;
                AppManager.ReNum1 = new double();
                AppManager.ReNum2 = new double();
                AppManager.ReNum3 = new double();
                AppManager.H = Convert.ToDouble(HNumberText.Text);
                AppManager.mainWindow.SecondTaskTab.IsEnabled = false;
                AppManager.mainWindow.ThirdTaskTab.IsEnabled = false;
                AppManager.firstTaskDataStrings.Clear();
                AppManager.listFirstTAskTable.Clear();
                AppManager.isOneReady = false;
                if (VNumberText.Text.StartsWith(","))
                    VNumberText.Text = "0" + VNumberText.Text;
                if (HNumberText.Text.StartsWith(","))
                    HNumberText.Text = "0" + HNumberText.Text;
                if (Re1NumberText.Text.StartsWith(","))
                    Re1NumberText.Text = "0" + Re1NumberText.Text;
                if (Re2NumberText.Text.StartsWith(","))
                    Re2NumberText.Text = "0" + Re2NumberText.Text;
                if (Re3NumberText.Text.StartsWith(","))
                    Re3NumberText.Text = "0" + Re3NumberText.Text;
                dataStrings.Add(VNumberText.Text);
                dataStrings.Add(HNumberText.Text);
                dataStrings.Add(Re1NumberText.Text);
                dataStrings.Add(Re2NumberText.Text);
                dataStrings.Add(Re3NumberText.Text);
                //Первый пункт | Определить максимальное значение скорости Vmax и вывести на экран.
                double maxSpeed = yLiquidCords.Max();
                MaxSpeedValueTextBox.Text = maxSpeed.ToString();
                dataStrings.Add(maxSpeed.ToString());
                //Второй пункт | Сравнить со значением 1.5Vвх, если меньше, то проинформировать пользователя
                double V15max = Convert.ToDouble(VNumberText.Text) * 1.5;
                double VTheoryX = Convert.ToDouble(VNumberText.Text);
                if (V15max < maxSpeed)
                {
                    IsThereAPointToTheorysLife = true;
                    MaxSpeedLessLabel.Content = "Значение больше 1.5Vвx";
                }
                else if (V15max == maxSpeed)
                {
                    IsThereAPointToTheorysLife = false;
                    MaxSpeedLessLabel.Content = "Значение равно 1.5Vвx";
                }
                else if (V15max > maxSpeed)
                {
                    MaxSpeedLessLabel.Content = "Значение меньше 1.5Vвx";
                    IsThereAPointToTheorysLife = false;
                }
                dataStrings.Add(MaxSpeedLessLabel.Content.ToString());
                //Третий пункт | Найти координаты, которым соответствует 95% и 98% Vmax. Определить ближайшую по модулю координату.
                var V95percent = maxSpeed * 0.95;
                var V98percent = maxSpeed * 0.98;
                int firstCord95Index = 0;
                int secondCord95Index = 0;
                double multiplier95 = 0;
                //Получение 2-х ближайших координат к V95
                double yLiquidCord95First = 0;
                for (int i = 0; i < xLiquidCords.Count; i++)
                {
                    if (yLiquidCords[i] < V95percent)
                    {
                        firstCord95Index = i;
                        yLiquidCord95First = yLiquidCords[i];
                    }
                    else if (V95percent <= yLiquidCords[i])
                    {
                        secondCord95Index = i;
                        multiplier95 = (V95percent - yLiquidCord95First) / (yLiquidCords[i] - yLiquidCord95First);
                        break;
                    }
                }
                double firstCord95 = xLiquidCords[firstCord95Index];
                double secondCord95 = xLiquidCords[secondCord95Index];
                double vX95 = (secondCord95 - firstCord95) * multiplier95;
                double[] cordsForV95 = {firstCord95 + vX95, V95percent };
                //Получение 2-х ближайших координат к V98
                int firstCord98Index = 0;
                int secondCord98Index = 0;
                double multiplier98 = 0;
                double yLiquidCord98First = 0;
                for (int i = 0; i < xLiquidCords.Count; i++)
                {
                    if (yLiquidCords[i] < V98percent)
                    {
                        firstCord98Index = i;
                        yLiquidCord98First = yLiquidCords[i];
                    }
                    else if (V98percent <= yLiquidCords[i])
                    {
                        secondCord98Index = i;
                        multiplier98 = (V98percent - yLiquidCord98First) / (yLiquidCords[i] - yLiquidCord98First);
                        break;
                    }
                }
                double firstCord98 = xLiquidCords[firstCord98Index];
                double secondCord98 = xLiquidCords[secondCord98Index];
                double vX98 = (secondCord98 - firstCord98) * multiplier98;

                Y98SpeedModuleValueTextBox.Text = V98percent.ToString();
                Y95SpeedValueTextBox.Text = V95percent.ToString();
                X98SpeedModuleValueTextBox.Text = (firstCord98 + vX98).ToString();
                X95SpeedValueTextBox.Text = (firstCord95 + vX95).ToString();

                //Расчет ближайших по модулю значений
                double avgX98 = (xLiquidCords[firstCord98Index] + xLiquidCords[secondCord98Index])/2;
                double avgY98 = (yLiquidCords[firstCord98Index] + yLiquidCords[secondCord98Index]) / 2;
                double avgX95 = (xLiquidCords[firstCord95Index] + xLiquidCords[secondCord95Index]) / 2;
                double avgY95 = (yLiquidCords[firstCord95Index] + yLiquidCords[secondCord95Index]) / 2;
                
                X98ClosestSpeedModuleTextBox.Text = avgX98.ToString();
                Y98ClosestSpeedModuleTextBox.Text = avgY98.ToString();
                X95ClosestSpeedModuleTextBox.Text = avgX95.ToString();
                Y95ClosestSpeedModuleTextBox.Text = avgY95.ToString();

                dataStrings.Add((firstCord98 + vX98).ToString());
                dataStrings.Add(V98percent.ToString());
                dataStrings.Add((firstCord95 + vX95).ToString());
                dataStrings.Add(V95percent.ToString());

                dataStrings.Add(avgX98.ToString());
                dataStrings.Add(avgY98.ToString());
                dataStrings.Add(avgX95.ToString());
                dataStrings.Add(avgY95.ToString());

                double[] yCords98Line = { avgY98, avgY98 };
                double[] xCords98Line = { 0, xLiquidCords.Last() };
                double[] yCords95Line = { avgY95, avgY95 };
                double[] xCords95Line = { 0, xLiquidCords.Last() };

                WpfPlot1.Plot.AddScatter(xCords98Line, yCords98Line, System.Drawing.Color.Green, 1, 5, MarkerShape.filledCircle, LineStyle.Dot, "Ближайшее по модулю значение к 98% Vmax");
                WpfPlot1.Plot.AddScatter(xCords95Line, yCords95Line, System.Drawing.Color.Orange, 1, 5, MarkerShape.filledCircle, LineStyle.Dot, "Ближайшее по модулю значение к 95% Vmax");
                WpfPlot1.Plot.AddPoint(firstCord98 + vX98, V98percent, System.Drawing.Color.LightCoral, 5, MarkerShape.filledCircle, "Координата, соответствующая 98% Vmax");
                WpfPlot1.Plot.AddPoint(firstCord95 + vX95, V95percent, System.Drawing.Color.Blue, 5, MarkerShape.filledCircle, "Координата, соответствующая 95% Vmax");
                

                //Пункт 4 | Представить результаты в виде таблицы:
                list[1].A = Re1NumberText.Text;
                list[2].A = Re2NumberText.Text;
                list[3].A = Re3NumberText.Text;

                list[1].B = avgY95.ToString();
                list[1].C = avgY98.ToString();
                list[2].B = avgY95.ToString();
                list[2].C = avgY98.ToString();
                list[3].B = avgY95.ToString();
                list[3].C = avgY98.ToString();

                list[1].B.Replace('.', ',');
                list[2].C.Replace('.', ',');
                list[3].D.Replace('.', ',');
                list[1].B.Replace('.', ',');
                list[2].C.Replace('.', ',');
                list[3].D.Replace('.', ',');
                list[1].B.Replace('.', ',');
                list[2].C.Replace('.', ',');
                list[3].D.Replace('.', ',');

                
                var L1max = (0.04 * Convert.ToDouble(list[1].A)) * Convert.ToDouble(HNumberText.Text);
                var L2max = (0.04 * Convert.ToDouble(list[2].A)) * Convert.ToDouble(HNumberText.Text);
                var L3max = (0.04 * Convert.ToDouble(list[3].A)) * Convert.ToDouble(HNumberText.Text);

                list[1].D = L1max.ToString();
                list[2].D = L2max.ToString();
                list[3].D = L3max.ToString();
                dataGrid1.Items.Refresh();

                AppManager.listFirstTAskTable.Add(list[1].A);
                AppManager.listFirstTAskTable.Add(list[1].B);
                AppManager.listFirstTAskTable.Add(list[1].C);
                AppManager.listFirstTAskTable.Add(list[1].D);
                AppManager.listFirstTAskTable.Add(list[2].A);
                AppManager.listFirstTAskTable.Add(list[2].B);
                AppManager.listFirstTAskTable.Add(list[2].C);
                AppManager.listFirstTAskTable.Add(list[2].D);
                AppManager.listFirstTAskTable.Add(list[3].A);
                AppManager.listFirstTAskTable.Add(list[3].B);
                AppManager.listFirstTAskTable.Add(list[3].C);
                AppManager.listFirstTAskTable.Add(list[3].D);
                //Получение 2-х ближайших координат к теоретическому значению
                double lForEachRe = 0;
                double firstCordTheoryValue = 0;
                double secondCordTheoryValue = 0;
                double multiplierTheory = 0;
                double yLiquidCordValFirst = 0;
                int firstCordTheoryIndex = 0;
                int secondCordTheoryIndex = 0;
                for (int i = 0; i < xLiquidCords.Count; i++)
                {
                    if (yLiquidCords[i] < V15max)
                    {
                        firstCordTheoryIndex = i;
                        firstCordTheoryValue = xLiquidCords[i];
                        yLiquidCordValFirst = yLiquidCords[i];
                    }
                    else if (V15max <= yLiquidCords[i])
                    {
                        secondCordTheoryIndex = i;
                        secondCordTheoryValue = xLiquidCords[i];
                        multiplierTheory = (V15max - yLiquidCordValFirst) / (yLiquidCords[i] - yLiquidCordValFirst);
                        break;
                    }
                }

                double vXTheory = (secondCordTheoryValue - firstCordTheoryValue) * multiplierTheory;
                double yCordTheory = V15max;
                double xCordTheory = (firstCordTheoryValue + vXTheory);

                double[] yCordsTheoryLine = { V15max, V15max };
                double[] xCordsTheoryLine = { 0, xLiquidCords.Last() };
                WpfPlot1.Plot.AddScatter(xCordsTheoryLine, yCordsTheoryLine, System.Drawing.Color.Red, 1, 5, MarkerShape.filledCircle, LineStyle.Dash, "Теоретическое значение");
                if (IsThereAPointToTheorysLife)
                {
                    WpfPlot1.Plot.AddPoint(xCordTheory, yCordTheory, System.Drawing.Color.Red);
                }

                WpfPlot1.Plot.Legend();
                WpfPlot1.Refresh();
                    //Пункт 6 | Выбрать значения ближе к теоретическим – это будет длина начального участка Lнач для каждого рассмотренного числа Re.

                lForEachRe = avgX98;
                AppManager.LStart = lForEachRe;
                LForEachReTextBox.Text = lForEachRe.ToString();
                dataStrings.Add(lForEachRe.ToString());

                PdfBtn.IsEnabled = true;
                AppManager.firstTaskDataStrings = dataStrings;
                //Получение графика в форме изображения
                string imageFileName = directory + "\\gohome.png";
                WpfPlot1.Plot.SaveFig(imageFileName);
                AppManager.isOneReady = true;
                AppManager.ReNum1 = Convert.ToDouble(list[1].A);
                AppManager.ReNum2 = Convert.ToDouble(list[2].A);
                AppManager.ReNum3 = Convert.ToDouble(list[3].A);
                AppManager.H = Convert.ToDouble(HNumberText.Text);
                AppManager.mainWindow.SecondTaskTab.IsEnabled = true;
                AppManager.mainWindow.ThirdTaskTab.IsEnabled = true;
            } catch
            {
                MessageBox.Show("Что-то пошло не так. Проверьте корректность введенных данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            AppManager.mainWindow.CheckCompletionStatus();
        }

        private void GetPDFFileBtn_Click(object sender, RoutedEventArgs e)
        {
           
            //Создание HTML страницы
            string HTMLReportVersion = @"<html lang=""en"">
<head>
<meta charset=""UTF-8"">
</head>
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
    <h3>Задание 1</h3>
    <p>Для каждого из трех чисел Re:
        Оценить длину начального участка канала (в качестве границы начального участка следует выбрать сечение, 
        в котором значение максимальной скорости составляет 95% и 98% 
        от максимальной скорости развитого течения Vmax = 1.5Vвх); провести сопоставление полученных 
        результатов по длине начального участка с аналитическим решением Lнач/H = 0.04*Re; 
        представить результаты в виде таблицы и графически.
    </p>" +
                $"<p><b>Входные данные:</b> V = {dataStrings[0]} H = {dataStrings[1]}</p>\r\n    " +
                $"<p><b>Числа Re:</b> Re<font style=\"font-size: 12pt;\">1</font> = {dataStrings[2]}; Re<font style=\"font-size: 12pt;\">2</font> = {dataStrings[3]}; Re<font style=\"font-size: 12pt;\">3</font> = {dataStrings[4]}</p>\r\n    " +
                "<h4>Графические данные: </h4>\r\n    " +
                "<p><b>Значение скорости Vx в центральном сечении канала:</b></p>\r\n    " +
                "<img src=\"gohome.png\" />\r\n    " +
                $"<p><b>Максимальное значение скорости V<font style=\"font-size: 12pt;\">max</font>:</b> {dataStrings[5]}</p>\r\n    " +
                $"<p>{dataStrings[6]}</p>\r\n    " +
                "<p>Координаты, соответствующие 98% V<font style=\"font-size: 12pt;\">max</font>: </p>\r\n    " +
                $"<p>X: {dataStrings[7]}; Y: {dataStrings[8]}</p>\r\n    " +
                "<p>Координаты, соответствующие 95% V<font style=\"font-size: 12pt;\">max</font>: </p>\r\n    " +
                $"<p>X: {dataStrings[9]}; Y: {dataStrings[10]}</p>\r\n    " +
                "<p>Ближайшие по модулю координаты к 98% Vmax: </p>\r\n    " +
                $"<p>X: {dataStrings[11]}; Y: {dataStrings[12]}</p>\r\n    " +
                "<p>Ближайшие по модулю координаты к 95% Vmax: </p>\r\n    " +
                $"<p>X: {dataStrings[13]}; Y: {dataStrings[14]}</p>\r\n    " +
                "<table>\r\n        " +
                "<tr>\r\n            " +
                "<td>Число Re</td>\r\n            " +
                "<td>95%V<font style=\"font-size: 12pt;\">max</font></td>\r\n            " +
                "<td>98%V<font style=\"font-size: 12pt;\">max</font></td>\r\n            " +
                "<td>L<font style=\"font-size: 12pt;\">нач</font> = (0.04 * Re) * H </td>\r\n        </tr>\r\n        " +
                "<tr>\r\n            " +
                $"<td>{list[1].A}</td>\r\n            " +
                $"<td>{list[1].B}</td>\r\n            " +
                $"<td>{list[1].C}</td>\r\n            " +
                $"<td>{list[1].D}</td>\r\n        </tr>\r\n        " +
                "<tr>\r\n            " +
                $"<td>{list[2].A}</td>\r\n            " +
                $"<td>{list[2].B}</td>\r\n            " +
                $"<td>{list[2].C}</td>\r\n            " +
                $"<td>{list[2].D}</td>\r\n        </tr>\r\n        " +
                "<tr>\r\n            " +
                $"<td>{list[3].A}</td>\r\n            " +
                $"<td>{list[3].B}</td>\r\n            " +
                $"<td>{list[3].C}</td>\r\n            " +
                $"<td>{list[3].D}</td>\r\n        </tr>\r\n    " +
                "</table>\r\n\r\n    " +
                $"<p>Длина начального участка L<font style=\"font-size: 12pt;\">нач</font> для каждого рассмотренного числа Re: {dataStrings[15]}</p>\r\n" +
                "</body>\r\n" +
                "</html>";

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
                string fullResult = fbd.SelectedPath + "\\ОтчетЗадание1.pdf";
                
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
        //Проверка ввода и корректности ввода данных
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
            if (Re1NumberText.Text != "" && Re2NumberText.Text != "" && Re3NumberText.Text != ""
                && VNumberText.Text != "" && HNumberText.Text != "" && xLiquidCords.Count >= 1)
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

        private void TaskViewBtn_Click(object sender, RoutedEventArgs e)
        {
            TaskViewWindow taskViewWindow = new TaskViewWindow();
            taskViewWindow.TaskConditionsTextBox.Text = "Для каждого из трех чисел Re:\r\nОценить длину начального участка канала (в качестве границы начального участка следует выбрать сечение, в котором значение максимальной скорости составляет 95% и 98% от максимальной скорости развитого течения Vmax = 1.5Vвх); провести сопоставление полученных результатов по длине начального участка с аналитическим решением Lнач/H = 0.04*Re; представить результаты в виде таблицы и графически.\r\n";
            taskViewWindow.TaskResultsTextBox.Text = "1. Определить максимальное значение скорости Vmax и вывести на экран.\r\n2. Сравнить со значением 1.5Vвх, если меньше, то проинформировать пользователя\r\n3. Найти координаты, которым соответствует 95% и 98% Vmax. Определить ближайшую по модулю координату.\r\n4. Представить результаты в виде таблицы:\r\n5. На графиках скорости в центральном сечении трубы указать 95% и 98% Vmax и теоретическое значение.\r\n6. Выбрать значения ближе к теоретическим – это будет длина начального участка Lнач для каждого рассмотренного числа Re.\r\n";
            taskViewWindow.Show();
        }
    }
}

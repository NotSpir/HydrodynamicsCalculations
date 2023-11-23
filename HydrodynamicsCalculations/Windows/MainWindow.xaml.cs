using HydrodynamicsCalculations.Views;
using iText.Html2pdf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using MessageBox = System.Windows.MessageBox;

namespace HydrodynamicsCalculations
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string directory = System.IO.Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
        public MainWindow()
        {
            InitializeComponent();

            AppManager.mainWindow = this;
            FirstTaskFrame.Navigate(new SpeedPage());
            SecondTaskFrame.Navigate(new LiquidPage());
            ThirdTaskFrame.Navigate(new FrictionPage());
        }
        Regex regex = new Regex(@"^[0-9]");

        
        private void CreatePDFPreview()
        {
            string beegHTML =
                @"<html lang=""en"">
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
    </p> " + $@"
    <p><b>Входные данные:</b> V = {AppManager.firstTaskDataStrings[0]} H = {AppManager.firstTaskDataStrings[1]}</p>
    <p><b>Числа Re:</b> Re<font style=""font-size: 12pt;"">1</font> = {AppManager.firstTaskDataStrings[2]}; Re<font style=""font-size: 12pt;"">2</font> = {AppManager.firstTaskDataStrings[3]}; Re<font style=""font-size: 12pt;"">3</font> = {AppManager.firstTaskDataStrings[4]}</p>
    <h4>Графические данные: </h4>
    <p><b>Значение скорости Vx в центральном сечении канала:</b></p>
    <img src=""gohome.png"" />
    <p><b>Максимальное значение скорости V<font style=""font-size: 12pt;"">max</font>:</b> {AppManager.firstTaskDataStrings[5]}</p>
    <p>{AppManager.firstTaskDataStrings[6]}</p>
    <p>Координаты, соответствующие 98% V<font style=""font-size: 12pt;"">max</font>: </p>
    <p>X: {AppManager.firstTaskDataStrings[7]}; Y: {AppManager.firstTaskDataStrings[8]}</p>
    <p>Координаты, соответствующие 95% V<font style=""font-size: 12pt;"">max</font>: </p>
    <p>X: {AppManager.firstTaskDataStrings[9]}; Y: {AppManager.firstTaskDataStrings[10]}</p>
    <p>Ближайшие по модулю координаты к 98% Vmax: </p>
    <p>X: {AppManager.firstTaskDataStrings[11]}; Y: {AppManager.firstTaskDataStrings[12]}</p>
    <p>Ближайшие по модулю координаты к 95% Vmax: </p>
    <p>X: {AppManager.firstTaskDataStrings[13]}; Y: {AppManager.firstTaskDataStrings[14]}</p>
    <table>
        <tr>
            <td>Число Re</td>
            <td>95%V<font style=""font-size: 12pt;"">max</font></td>
            <td>98%V<font style=""font-size: 12pt;"">max</font></td>
            <td>L<font style=""font-size: 12pt;"">нач</font> = (0.04 * Re) * H </td>
        </tr>
        <tr>
            <td>{AppManager.listFirstTAskTable[0]}</td>
            <td>{AppManager.listFirstTAskTable[1]}</td>
            <td>{AppManager.listFirstTAskTable[2]}</td>
            <td>{AppManager.listFirstTAskTable[3]}</td>
        </tr>
        <tr>
            <td>{AppManager.listFirstTAskTable[4]}</td>
            <td>{AppManager.listFirstTAskTable[5]}</td>
            <td>{AppManager.listFirstTAskTable[6]}</td>
            <td>{AppManager.listFirstTAskTable[7]}</td>
        </tr>
        <tr>
            <td>{AppManager.listFirstTAskTable[8]}</td>
            <td>{AppManager.listFirstTAskTable[9]}</td>
            <td>{AppManager.listFirstTAskTable[10]}</td>
            <td>{AppManager.listFirstTAskTable[11]}</td>
        </tr>
    </table>
   <p>Длина начального участка L<font style=""font-size: 12pt;"">нач</font> для каждого рассмотренного числа Re: {AppManager.firstTaskDataStrings[15]}</p>"
   +

   $@"<h3>Задание 2</h3>
    <p>Для каждого из трех чисел Re:
        Проанализировать поле давления; найти коэффициент сопротивления λ для начального участка, участка с установившимся параболическим профилем скорости и канала в целом при помощи формулы: </p>
        <p>
        Δp = λ * (ΔL/H) * ((ρV<font style=""font-size: 12pt;"">ср</font>^2)/2)</p>
    <p>где Δp – перепад давления на участке канала длиной ΔL, ρ – плотность жидкости, Vср – средняя по сечению скорость. Определить значение λ для участка развитого течения из угла наклона линейного участка зависимости. Провести сопоставление с теоретической оценкой для развитого участка λ = 24/Re; представить результаты в виде таблицы и графически.</p>
        </p>
    <p><b>Входные данные:</b> p = {AppManager.secondTaskDataStrings[0]}</p>
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

    <p>V<font style=""font-size: 12pt;"">ср</font> в сечении x=H: {AppManager.secondTaskDataStrings[1]}</p>
    <p>V<font style=""font-size: 12pt;"">ср</font> в сечении x=19H: {AppManager.secondTaskDataStrings[2]}</p>
    <table>
        <tr>
            <td></td>
            <td>Начальный участок</td>
            <td>Развитое течение</td>
            <td>Весь канал</td>
            <td>λ = 24/Re </td>
        </tr>
        <tr>
            <td>{AppManager.secondTaskDataStrings[3]}</td>
            <td>{AppManager.secondTaskDataStrings[4]}</td>
            <td>{AppManager.secondTaskDataStrings[5]}</td>
            <td>{AppManager.secondTaskDataStrings[6]}</td>
            <td>{AppManager.secondTaskDataStrings[7]}</td>
        </tr>
        <tr>
            <td>{AppManager.secondTaskDataStrings[8]}</td>
            <td>{AppManager.secondTaskDataStrings[9]}</td>
            <td>{AppManager.secondTaskDataStrings[10]}</td>
            <td>{AppManager.secondTaskDataStrings[11]}</td>
            <td>{AppManager.secondTaskDataStrings[12]}</td>
        </tr>
        <tr>
            <td>{AppManager.secondTaskDataStrings[13]}</td>
            <td>{AppManager.secondTaskDataStrings[14]}</td>
            <td>{AppManager.secondTaskDataStrings[15]}</td>
            <td>{AppManager.secondTaskDataStrings[16]}</td>
            <td>{AppManager.secondTaskDataStrings[17]}</td>
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
            <td>{AppManager.secondTaskDataStrings[19]}</td>
            <td>{AppManager.secondTaskDataStrings[20]}</td>
            <td>{AppManager.secondTaskDataStrings[21]}</td>
            <td>{AppManager.secondTaskDataStrings[22]}</td>
            <td>{AppManager.secondTaskDataStrings[23]}</td>
        </tr>
        <tr>
            <td>{AppManager.secondTaskDataStrings[24]}</td>
            <td>{AppManager.secondTaskDataStrings[25]}</td>
            <td>{AppManager.secondTaskDataStrings[26]}</td>
            <td>{AppManager.secondTaskDataStrings[27]}</td>
            <td>{AppManager.secondTaskDataStrings[28]}</td>
        </tr>
        <tr>
            <td>{AppManager.secondTaskDataStrings[29]}</td>
            <td>{AppManager.secondTaskDataStrings[30]}</td>
            <td>{AppManager.secondTaskDataStrings[31]}</td>
            <td>{AppManager.secondTaskDataStrings[32]}</td>
            <td>{AppManager.secondTaskDataStrings[33]}</td>
        </tr>
    </table>"

    + $@"<h3>Задание 3</h3>
    <p>Построить график зависимости коэффициента трения на стенке канала, Cf, от координаты x. Проанализировать распределение коэффициента трения и убедиться в том, что для участка развитого течения выполняется соотношение  = 2 Cf.
    </p>
    <h4>Графические данные: </h4>
    <p><b>Зависимость коэффициента трения на стенке канала, Cf, от координаты:</b></p>
    <img src=""cfgraph.png"" />
    <p>C<font style=""font-size: 12pt;"">f</font> для участка развитого течения: <b>{AppManager.thirdTaskDataStrings[0]}</b></p>
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
                File.WriteAllText(fileName, beegHTML);

                string navFileName = directory + "PDFStructuralFile.html";
                previewPDF.Navigate(navFileName);
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.ToString());
            }

        }

        private void CreatePDFButton_Click(object sender, RoutedEventArgs e)
        {
            string fileName = directory + @"\PDFStructuralFile.html";

            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();
                string fullResult = fbd.SelectedPath + "\\Полный отчет по лабораторной работе.pdf";

                // Process open file dialog box results
                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    HtmlConverter.ConvertToPdf(
                        new FileInfo(fileName),
                        new FileInfo(fullResult)
                        );
                    MessageBox.Show("Файл успешно создан");
                }
                else MessageBox.Show("Что-то пошло не так!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void CheckCompletionStatus()
        {
            if (AppManager.isOneReady && AppManager.isTwoReady && AppManager.isThreeReady)
            {
                PDFTab.IsEnabled = true;
                CreatePDFPreview();
            }
            else
                PDFTab.IsEnabled = false;
            }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void Tab_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TabItem selectedTab = (sender as TabItem);
            FirstTaskTab.FontSize = 12;
            SecondTaskTab.FontSize = 12;
            ThirdTaskTab.FontSize = 12;
            PDFTab.FontSize = 12;
            selectedTab.FontSize = 16;
        }
    }
    }

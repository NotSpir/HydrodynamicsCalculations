using Microsoft.Win32;
using Org.BouncyCastle.Math.EC.Rfc7748;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using Image = System.Drawing.Image;

namespace HydrodynamicsCalculations
{
    internal class AppManager
    {

        public static MainWindow mainWindow = new MainWindow();
        //Хранение данных для создания отчета по всем заданиям
        public static double ReNum1 = new double();
        public static double ReNum2 = new double();
        public static double ReNum3 = new double();
        public static double H = new double();
        public static double LStart = new double();
        public static bool IsThereATheoryValue = false;

        public static List <string> firstTaskDataStrings = new List<string>();
        public static List<string> listFirstTAskTable = new List<string>();

        public static List<string> secondTaskDataStrings = new List<string>();

        public static List<string> thirdTaskDataStrings = new List<string>();
        public static bool isOneReady = false;
        public static bool isTwoReady = false;
        public static bool isThreeReady = false;

        static Regex regex = new Regex(@"^[0-9]");
        public static void GetFileToGraph(ScottPlot.WpfPlot wpfPlot, ref List<double> xCords, ref List<double> yCords)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog { Filter = "XYFile | *.xy" };
                if (ofd.ShowDialog() == true)
                {
                    if (yCords.Count > 0)
                        yCords.Clear();
                    if (xCords.Count > 0)
                        xCords.Clear();

                    string filePath = ofd.FileName;
                    string contents = File.ReadAllText(filePath);
                    var result = contents.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < result.Length; i++)
                    {
                        if (regex.IsMatch(result[i][0].ToString()))
                        {
                            string[] partResults = result[i].Split(new[] { '	', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            //string[] partResults = result[i].Split('	');

                            if (partResults[0].Contains('.'))
                            {
                                partResults[0] = partResults[0].Replace('.', ',');
                            }
                            if (partResults[1].Contains('.'))
                            {
                                partResults[1] = partResults[1].Replace('.', ',');
                            }
                            xCords.Add(Convert.ToDouble(partResults[0]));
                            yCords.Add(Convert.ToDouble(partResults[1]));
                        }
                    }
                    double[] dataX = xCords.ToArray();
                    double[] dataY = yCords.ToArray();
                    wpfPlot.Plot.Clear();
                    wpfPlot.Plot.AddScatter(dataX, dataY, System.Drawing.Color.Black, 1, 3);
                    wpfPlot.Refresh();
                }

            }
            catch
            {
                MessageBox.Show("Не получилось создать график. Проверьте корректность файла.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static readonly Regex regexNums = new Regex("^[^0-9,\\s]+$");

        private static bool IsTextAllowed(string text)
        {
            return !regexNums.IsMatch(text);
        }
        public static void CheckTextIsNumber(object sender, TextCompositionEventArgs e)
        {
            string fullText = (sender as TextBox).Text;
            int a = fullText.Split(',').Length;
            if ((a > 1 && e.Text == ",") || (fullText == "" && e.Text == ","))
                e.Handled = true;
            else
                e.Handled = regexNums.IsMatch(e.Text);
        }
        public static void CheckPastedTextIsNumber(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                string text = (string)e.DataObject.GetData(typeof(String));
                int a = text.Split(',').Length;

                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

    }

}

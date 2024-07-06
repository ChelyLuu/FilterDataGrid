using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace FilterDataGrid
{
    public class HighLightText
    {
        //private static string DefaultText { get; set; }
        //public static void SetInputText(TextBlock textElement, string value)
        //{
        //    textElement.SetValue(InputTextProperty, value);
        //}

        //public static string GetInputText(TextBlock textElement)
        //{
        //    return (string)textElement.GetValue(InputTextProperty);
        //}

        //// Using a DependencyProperty as the backing store for ColorStart.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty InputTextProperty =
        //    DependencyProperty.RegisterAttached("InputText", typeof(string), typeof(HighLightText), new FrameworkPropertyMetadata("", OnInputTextChanged));

        //private static void OnInputTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    //Console.WriteLine(e.NewValue.ToString());
        //    DefaultText = e.NewValue.ToString();
        //}



        public static void SetHiText(TextBlock textElement, string value)
        {
            textElement.SetValue(HiTextProperty, value);
        }

        public static string GetHiText(TextBlock textElement)
        {
            return (string)textElement.GetValue(HiTextProperty);
        }

        // Using a DependencyProperty as the backing store for ColorStart.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HiTextProperty =
            DependencyProperty.RegisterAttached("HiText", typeof(string), typeof(HighLightText), new FrameworkPropertyMetadata("", OnHiTextChanged));


        private static void OnHiTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            TextBlock textBlock = d as TextBlock;

            if (string.IsNullOrEmpty(textBlock.Text)) return;

            string DefaultText = textBlock.Text;
            string hiText = e.NewValue as string;

            if (!string.IsNullOrEmpty(hiText))
            {
                textBlock.Inlines.Clear();
                SolidColorBrush primaryColor = (SolidColorBrush)Application.Current.Resources["FPrimaryColor"];
                SolidColorBrush primaryFore = (SolidColorBrush)Application.Current.Resources["FPrimaryForeColor"];

                string[] spli = Regex.Split(DefaultText, hiText, RegexOptions.IgnoreCase);
                for (int i = 0; i < spli.Length; i++)
                {
                    textBlock.Inlines.Add(new Run(spli[i]));

                    if (i < spli.Length - 1)
                    {
                        int searchstart = textBlock.Text.Length;
                        var iCaseTextIndex = DefaultText.IndexOf(hiText, searchstart, StringComparison.OrdinalIgnoreCase);
                        if (iCaseTextIndex < 0)
                        {
                            continue;
                        }
                        string caseText = DefaultText.Substring(iCaseTextIndex, hiText.Length);
                        textBlock.Inlines.Add(new Run(caseText) { Background = primaryColor, Foreground = primaryFore });
                    }
                }
            }
            else
            {
                textBlock.Text = DefaultText;
            }
        }

    }
}

using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace FilterDataGrid
{
    public static class StringFormatHelper
    {
        #region Value

        public static DependencyProperty ValueProperty = DependencyProperty.RegisterAttached(
            "Value", typeof(object), typeof(StringFormatHelper), new System.Windows.PropertyMetadata(null, OnValueChanged));

        private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            RefreshFormattedValue(obj);
        }

        public static object GetValue(DependencyObject obj)
        {
            return obj.GetValue(ValueProperty);
        }

        public static void SetValue(DependencyObject obj, object newValue)
        {
            obj.SetValue(ValueProperty, newValue);
        }

        #endregion

        #region Format

        public static DependencyProperty FormatProperty = DependencyProperty.RegisterAttached(
            "Format", typeof(string), typeof(StringFormatHelper), new System.Windows.PropertyMetadata(null, OnFormatChanged));

        private static void OnFormatChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            RefreshFormattedValue(obj);
        }

        public static string GetFormat(DependencyObject obj)
        {
            return (string)obj.GetValue(FormatProperty);
        }

        public static void SetFormat(DependencyObject obj, string newFormat)
        {
            obj.SetValue(FormatProperty, newFormat);
        }

        #endregion

        #region FormattedValue

        public static DependencyProperty FormattedValueProperty = DependencyProperty.RegisterAttached(
            "FormattedValue", typeof(string), typeof(StringFormatHelper), new System.Windows.PropertyMetadata(null));

        public static string GetFormattedValue(DependencyObject obj)
        {
            return (string)obj.GetValue(FormattedValueProperty);
        }


        public static void SetHiText(TextBlock textElement, string value)
        {
            textElement.SetValue(HiTextProperty, value);
        }

        public static string GetHiText(TextBlock textElement)
        {
            return (string)textElement.GetValue(HiTextProperty);
        }



        // Using a DependencyProperty as the backing store for HiText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HiTextProperty =
             DependencyProperty.RegisterAttached("HiText", typeof(string), typeof(StringFormatHelper), new FrameworkPropertyMetadata("", HighLightText.OnHiTextChanged));




        public static void SetFormattedValue(DependencyObject obj, string newFormattedValue)
        {
            TextBlock textBlock = obj as TextBlock;
            if (string.IsNullOrEmpty(GetHiText(textBlock)))
            {
                textBlock.Text = newFormattedValue;
            }
            else
            {
                string DefaultText = newFormattedValue;
                string hiText = GetHiText(textBlock);


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

        }

        #endregion

        private static void RefreshFormattedValue(DependencyObject obj)
        {
            var value = GetValue(obj);
            var format = GetFormat(obj);
            if (format != null)
            {
                if (!format.Contains("{0"))
                {
                    format = String.Format("{{0:{0}}}", format);
                }

                SetFormattedValue(obj, String.Format(format, value));
            }
            else
            {
                SetFormattedValue(obj, value == null ? String.Empty : value.ToString());
            }
        }
    }
}

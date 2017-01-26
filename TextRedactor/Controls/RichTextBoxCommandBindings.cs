using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Controls
{
	static class RichTextBoxCommandBindings
	{
        public static void IntializeCommandBindings(RichTextBox richTextBox)
        {
            if (richTextBox == null) return;

            richTextBox.CommandBindings.Add(new System.Windows.Input.CommandBinding(
                                        EditingCommands.ToggleBold,
                                        (sender, e) => SetBold(richTextBox, !GetBold(richTextBox)),
                                        (sender, e) =>
                                        {
                                            CommandUtil.SetCurrentValue(e, GetBold(richTextBox));

                                            e.CanExecute = true;
                                            e.Handled = true;
                                        }));

            richTextBox.CommandBindings.Add(new CommandBinding(
                                        EditingCommands.ToggleItalic,
                                        (sender, e) => SetItalic(richTextBox, !GetItalic(richTextBox)),
                                        (sender, e) =>
                                            {
                                                CommandUtil.SetCurrentValue(e, GetItalic(richTextBox));

                                                e.CanExecute = true;
                                                e.Handled = true;
                                            }));
            richTextBox.CommandBindings.Add(new CommandBinding(
                                       EditingCommands.ToggleUnderline,
                                       (sender, e) => SetUnderline(richTextBox, !GetUnderline(richTextBox)),
                                       (sender, e) =>
                                       {
                                           CommandUtil.SetCurrentValue(e, GetUnderline(richTextBox));

                                           e.CanExecute = true;
                                           e.Handled = true;
                                       }));
            richTextBox.CommandBindings.Add(new CommandBinding(
                                      EditingCommands.AlignLeft,
                                      (sender, e) => SetAlignLeft(richTextBox, !GetAlignLeft(richTextBox)),
                                      (sender, e) =>
                                      {
                                          CommandUtil.SetCurrentValue(e, GetAlignLeft(richTextBox));

                                          e.CanExecute = true;
                                          e.Handled = true;
                                      }));
            richTextBox.CommandBindings.Add(new CommandBinding(
                                     EditingCommands.AlignCenter,
                                     (sender, e) => SetAlignCenter(richTextBox, !GetAlignCenter(richTextBox)),
                                     (sender, e) =>
                                     {
                                         CommandUtil.SetCurrentValue(e, GetAlignCenter(richTextBox));

                                         e.CanExecute = true;
                                         e.Handled = true;
                                     }));
            richTextBox.CommandBindings.Add(new CommandBinding(
                         EditingCommands.AlignRight,
                         (sender, e) => SetAlignRight(richTextBox, !GetAlignRight(richTextBox)),
                         (sender, e) =>
                         {
                             CommandUtil.SetCurrentValue(e, GetAlignRight(richTextBox));

                             e.CanExecute = true;
                             e.Handled = true;
                         }));
            richTextBox.CommandBindings.Add(new CommandBinding(
                         EditingCommands.AlignJustify,
                         (sender, e) => SetAlignJustify(richTextBox, !GetAlignJustify(richTextBox)),
                         (sender, e) =>
                         {
                             CommandUtil.SetCurrentValue(e, GetAlignJustify(richTextBox));

                             e.CanExecute = true;
                             e.Handled = true;
                         }));
            richTextBox.CommandBindings.Add(new CommandBinding(
                                                MyCommands.SetFontSize,
                                                (sender, e) =>
                                                {
                                                    if (e.Parameter is double)
                                                    {
                                                        SetFontSize(richTextBox, (double)e.Parameter);
                                                    }
                                                },
                                                (sender, e) =>
                                                {
                                                    CommandUtil.SetCurrentValue(e, GetFontSize(richTextBox));
                                                    e.CanExecute = true;
                                                    e.Handled = true;
                                                }));
            richTextBox.CommandBindings.Add(new CommandBinding(
                                              MyCommands.SetFontFamily,
                                              (sender, e) =>
                                             
                                              {
                                                  if (e.Parameter is FontFamily)
                                                  {
                                                      SetFontFamily(richTextBox, (FontFamily)e.Parameter);
                                                  }
                                              },
												(sender, e) =>
                                                {
                                                    CommandUtil.SetCurrentValue(e, GetFontFamily(richTextBox));
                                                    e.CanExecute = true;
                                                    e.Handled = true;
                                                }));
            //richTextBox.CommandBindings.Add(new CommandBinding(
            //                                    MyCommands.SetFontColor,
            //                                    (sender, e) =>
            //                                    {
            //                                        if (e.Parameter is Color)
            //                                        {
            //                                            SetFontColor(richTextBox, (Color)e.Parameter);
            //                                        }
            //                                    },
            //                                    (sender, e) =>
            //                                    {
            //                                        CommandUtil.SetCurrentValue(e, GetFontColor(richTextBox));
            //                                        e.CanExecute = true;
            //                                        e.Handled = true;
            //                                    }));
        }
		private static double GetFontSize(RichTextBox richTextBox)
		{
			var value = richTextBox.Selection.GetPropertyValue(TextBlock.FontSizeProperty);
			if (value != DependencyProperty.UnsetValue)
			{
				return (double)value;
			}
			return 12;
		}
        private static FontFamily GetFontFamily(RichTextBox richTextBox)
        {
            var value = richTextBox.Selection.GetPropertyValue(TextBlock.FontFamilyProperty);
            if (value != DependencyProperty.UnsetValue)
            {
                return (FontFamily)value;
            }
            return Fonts.SystemFontFamilies.First();
        }
        private static Color GetFontColor (RichTextBox richTextBox)
		{
			var value = richTextBox.Selection.GetPropertyValue(TextElement.ForegroundProperty);
			if (value != DependencyProperty.UnsetValue)
			{
				if (value is SolidColorBrush)
					return (value as SolidColorBrush).Color;
			}
			return Colors.Black;
		}

		private static void SetFontColor (RichTextBox richTextBox, Color c)
		{
			richTextBox.Selection.ApplyPropertyValue(TextElement.ForegroundProperty,
			                                         new SolidColorBrush(c));
		}


		private static void SetItalic(RichTextBox richTextBox, bool isItalic)
		{
			richTextBox.Selection.ApplyPropertyValue(TextBlock.FontStyleProperty,
				isItalic ? FontStyles.Italic : FontStyles.Normal);
		}

		private static bool GetItalic(RichTextBox richTextBox)
		{
			var value = richTextBox.Selection.GetPropertyValue(TextBlock.FontStyleProperty);
			if (value != DependencyProperty.UnsetValue)
			{
				return (FontStyle)value == FontStyles.Italic;
			}
			return false;
		}
        private static void SetUnderline(RichTextBox richTextBox, bool isUnderline)
        {
            richTextBox.Selection.ApplyPropertyValue(TextBlock.TextDecorationsProperty,
                isUnderline ? TextDecorations.Underline : null);
        }

        private static bool GetUnderline(RichTextBox richTextBox)
        {
            var value = richTextBox.Selection.GetPropertyValue(TextBlock.TextDecorationsProperty);
            if (value != DependencyProperty.UnsetValue)
            {
                return value == TextDecorations.Underline;
            }
            return false;
        }
        private static void SetAlignLeft(RichTextBox richTextBox, bool isAlignLeft)
        {
            richTextBox.Selection.ApplyPropertyValue(TextBlock.TextAlignmentProperty,
                isAlignLeft ? TextAlignment.Left : TextAlignment.Left);
        }

        private static bool GetAlignLeft(RichTextBox richTextBox)
        {
            var value = richTextBox.Selection.GetPropertyValue(TextBlock.TextAlignmentProperty);
            if (value != DependencyProperty.UnsetValue)
            {
                return (TextAlignment)value == TextAlignment.Left;
            }
            return false;
        }
        private static void SetAlignCenter(RichTextBox richTextBox, bool isAlignCenter)
        {
            richTextBox.Selection.ApplyPropertyValue(TextBlock.TextAlignmentProperty,
                isAlignCenter ? TextAlignment.Center : TextAlignment.Center);
        }

        private static bool GetAlignCenter(RichTextBox richTextBox)
        {
            var value = richTextBox.Selection.GetPropertyValue(TextBlock.TextAlignmentProperty);
            if (value != DependencyProperty.UnsetValue)
            {
                return (TextAlignment)value == TextAlignment.Center;
            }
            return false;
        }
        private static void SetAlignRight(RichTextBox richTextBox, bool isAlignRight)
        {
            richTextBox.Selection.ApplyPropertyValue(TextBlock.TextAlignmentProperty,
                isAlignRight ? TextAlignment.Right : TextAlignment.Right);
        }

        private static bool GetAlignRight(RichTextBox richTextBox)
        {
            var value = richTextBox.Selection.GetPropertyValue(TextBlock.TextAlignmentProperty);
            if (value != DependencyProperty.UnsetValue)
            {
                return (TextAlignment)value == TextAlignment.Right;
            }
            return false;
        }
        private static void SetAlignJustify(RichTextBox richTextBox, bool isAlignJustify)
        {
            richTextBox.Selection.ApplyPropertyValue(TextBlock.TextAlignmentProperty,
                isAlignJustify ? TextAlignment.Justify : TextAlignment.Justify);
        }

        private static bool GetAlignJustify(RichTextBox richTextBox)
        {
            var value = richTextBox.Selection.GetPropertyValue(TextBlock.TextAlignmentProperty);
            if (value != DependencyProperty.UnsetValue)
            {
                return (TextAlignment)value == TextAlignment.Justify;
            }
            return false;
        }
        private static void SetBold(RichTextBox richTextBox, bool isBold)
		{
			richTextBox.Selection.ApplyPropertyValue(TextBlock.FontWeightProperty,
				 isBold ? FontWeights.Bold : FontWeights.Normal);
		}

		private static bool GetBold(RichTextBox richTextBox)
		{
			var value = richTextBox.Selection.GetPropertyValue(TextBlock.FontWeightProperty);
			if (value != DependencyProperty.UnsetValue)
			{
				return (FontWeight)value == FontWeights.Bold;
			}
			return false;
		}

		private static void SetFontSize (RichTextBox richTextBox, double size)
		{
			richTextBox.Selection.ApplyPropertyValue(TextBlock.FontSizeProperty,size);
		}
        private static void SetFontFamily(RichTextBox richTextBox, FontFamily font)
        {
            richTextBox.Selection.ApplyPropertyValue(TextBlock.FontFamilyProperty, font);
        }
    }
}

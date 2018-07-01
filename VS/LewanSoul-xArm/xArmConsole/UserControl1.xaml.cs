using System;
using System.Collections.Generic;
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

namespace xArmConsole
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            (sender as Slider).Value = Math.Floor(e.NewValue);
            e.Handled = true;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = ((TextBox)sender);

            textBox.Dispatcher.BeginInvoke(new Action(() =>
            {
                textBox.SelectAll();
            }));
            e.Handled = true;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox textBox = ((TextBox)sender);

                BindingExpression obj = textBox.GetBindingExpression(TextBox.TextProperty);
                obj.UpdateSource();
                textBox.SelectAll();
            }
        }

        private void ButtonUp_Click(object sender, RoutedEventArgs e)
        {
            int step = Keyboard.IsKeyDown(Key.LeftCtrl) ? 10 : 1;
            
            sliderAngle.Value += step;
        }

        private void ButtonDown_Click(object sender, RoutedEventArgs e)
        {
            int step = Keyboard.IsKeyDown(Key.LeftCtrl) ? 10 : 1;

            sliderAngle.Value -= step;
        }

        private void Slider_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Slider slider = GetSliderFromSender(sender);
            if (slider == null) return;

            int step = Keyboard.IsKeyDown(Key.LeftCtrl) ? 10 : 1;

            if (e.Delta > 0)
            {
                slider.Value += step;
            }
            else
            {
                slider.Value -= step;
            }

            ((TextBox)sender).SelectAll();
        }

        private void Slider_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Slider slider = GetSliderFromSender(sender);
            if (slider == null) return;

            slider.Value = slider.Minimum < 0 ? 0 : Math.Floor(slider.Maximum / 2);
        }

        protected Slider GetSliderFromSender(object sender)
        {
            Slider slider;

            if (sender.GetType().Equals(typeof(Slider)))
            {
                slider = (sender as Slider);
            }
            else
            {
                BindingExpression obj = (sender as TextBox).GetBindingExpression(TextBox.TextProperty);
                slider = (obj.ResolvedSource as Slider);
            }

            return slider;
        }

        private void MouseWheel_Angle(object sender, MouseWheelEventArgs e)
        {
            int step = Keyboard.IsKeyDown(Key.LeftCtrl) ? 10 : 1;

            if (e.Delta > 0)
            {
                sliderAngle.Value += step;
            }
            else
            {
                sliderAngle.Value -= step;
            }

            Keyboard.ClearFocus();
        }

        private void MouseWheel_Offset(object sender, MouseWheelEventArgs e)
        {
            int step = Keyboard.IsKeyDown(Key.LeftCtrl) ? 10 : 1;

            if (e.Delta > 0)
            {
                sliderOffset.Value += step;
            }
            else
            {
                sliderOffset.Value -= step;
            }

            Keyboard.ClearFocus();
        }
    }
}

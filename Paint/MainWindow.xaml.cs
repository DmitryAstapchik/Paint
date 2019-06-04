using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Paint
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string action = "line";
        bool drawing;
        Point firstPoint;
        bool firstMouseMove;
        Point previousPoint;
        string lastThickness = "1";
        string lastEraserThickness = "30";
        Stack<Rectangle> eraserStack = new Stack<Rectangle>();
        Rectangle eraser = new Rectangle();
        Rectangle eraserCursor = new Rectangle();

        public MainWindow()
        {
            InitializeComponent();
            ComboBox_PrimaryColor.ItemsSource = typeof(Brushes).GetProperties();
            ComboBox_SecondaryColor.ItemsSource = typeof(Brushes).GetProperties();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Canvas.Width = Grid.ActualWidth - DockPanel.ActualWidth;
        }

        private void MainWindow_LayoutUpdated(object sender, EventArgs e)
        {
            Canvas.Width = Grid.ActualWidth - DockPanel.ActualWidth;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch (action)
            {
                case "polyline":
                    if (!drawing)
                    {
                        StartPolyline(e);
                    }
                    else
                    {
                        ((Polyline)Canvas.Children[Canvas.Children.Count - 1]).Points.Add(e.GetPosition(Canvas));
                    }
                    break;
                case "polygon":
                    if (!drawing)
                    {
                        StartPolygon(e);
                    }
                    else
                    {
                        ((Polygon)Canvas.Children[Canvas.Children.Count - 1]).Points.Add(e.GetPosition(Canvas));
                    }
                    break;
                case "eraser":
                    StartErase(e);
                    break;
                default:
                    if (drawing == false)
                    {
                        drawing = true;
                        firstPoint = e.GetPosition(Canvas);
                        firstMouseMove = true;
                    }
                    else
                    {
                        drawing = false;
                    }
                    break;
            }
        }

        private void StartErase(MouseButtonEventArgs e)
        {
            drawing = true;
            var eraser = new Rectangle
            {
                Width = Convert.ToDouble(TextBox_Thickness.Text),
                Height = Convert.ToDouble(TextBox_Thickness.Text),
                StrokeThickness = 0,
                Fill = Canvas.Background,
                Margin = new Thickness(e.GetPosition(Canvas).X - Convert.ToDouble(TextBox_Thickness.Text) / 2, e.GetPosition(Canvas).Y - Convert.ToDouble(TextBox_Thickness.Text) / 2, 0, 0)
            };
            Canvas.Children.Add(eraser);
            eraserStack.Push(eraser);
            Canvas.Children.Remove(eraserCursor);
            eraserCursor.Margin = eraser.Margin;
            Canvas.Children.Add(eraserCursor);
        }

        private void StartPolygon(MouseButtonEventArgs e)
        {
            var polygon = new Polygon
            {
                Stroke = (Brush)new BrushConverter().ConvertFromString(((System.Reflection.PropertyInfo)ComboBox_PrimaryColor.SelectedItem).Name),
                StrokeThickness = Convert.ToDouble(TextBox_Thickness.Text)
            };
            polygon.Points.Add(e.GetPosition(Canvas));
            Canvas.Children.Add(polygon);
            drawing = true;
        }

        private void StartPolyline(MouseButtonEventArgs e)
        {
            var polyline = new Polyline
            {
                Stroke = (Brush)new BrushConverter().ConvertFromString(((System.Reflection.PropertyInfo)ComboBox_PrimaryColor.SelectedItem).Name),
                StrokeThickness = Convert.ToDouble(TextBox_Thickness.Text)
            };
            polyline.Points.Add(e.GetPosition(Canvas));
            Canvas.Children.Add(polyline);
            drawing = true;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.GetPosition(Canvas) != previousPoint)
            {
                if (drawing)
                {
                    switch (action)
                    {
                        case "line":
                            DrawLine(e);
                            break;
                        case "rectangle":
                            DrawRectangle(e);
                            break;
                        case "ellipse":
                            DrawEllipse(e);
                            break;
                        case "polyline":
                            DrawPolyline(e);
                            break;
                        case "polygon":
                            DrawPolygon(e);
                            break;
                        case "eraser":
                            Erase(e);
                            break;
                        default:
                            break;
                    }
                }
                else if (action == "eraser")
                {
                    Canvas.Children.Remove(eraserCursor);
                    eraserCursor.Margin = new Thickness(e.GetPosition(Canvas).X - Convert.ToDouble(TextBox_Thickness.Text) / 2, e.GetPosition(Canvas).Y - Convert.ToDouble(TextBox_Thickness.Text) / 2, 0, 0);
                    Canvas.Children.Add(eraserCursor);
                }
                firstMouseMove = false;
                previousPoint = e.GetPosition(Canvas);
            }
        }

        private void Erase(MouseEventArgs e)
        {
            var eraser = new Rectangle
            {
                Width = Convert.ToDouble(TextBox_Thickness.Text),
                Height = Convert.ToDouble(TextBox_Thickness.Text),
                Fill = Canvas.Background,
                Margin = new Thickness(e.GetPosition(Canvas).X - Convert.ToDouble(TextBox_Thickness.Text) / 2, e.GetPosition(Canvas).Y - Convert.ToDouble(TextBox_Thickness.Text) / 2, 0, 0)
            };
            Canvas.Children.Add(eraser);
        }

        private void DrawPolygon(MouseEventArgs e)
        {
            var last = (Polygon)Canvas.Children[Canvas.Children.Count - 1];
            if (last.Points.Count == 1)
            {
                last.Points.Add(e.GetPosition(Canvas));
            }
            else
            {
                last.Points[last.Points.Count - 1] = e.GetPosition(Canvas);
            }
            if (CheckBox_Fill.IsChecked == true)
            {
                last.Fill = (Brush)new BrushConverter().ConvertFromString(((System.Reflection.PropertyInfo)ComboBox_SecondaryColor.SelectedItem).Name);
            }
        }

        private void DrawPolyline(MouseEventArgs e)
        {
            var last = (Polyline)Canvas.Children[Canvas.Children.Count - 1];
            if (last.Points.Count == 1)
            {
                last.Points.Add(e.GetPosition(Canvas));
            }
            else
            {
                last.Points[last.Points.Count - 1] = e.GetPosition(Canvas);
            }
        }

        private void DrawEllipse(MouseEventArgs e)
        {
            Ellipse ellipse = new Ellipse();
            MakeFigure(ellipse, e);
            if (!firstMouseMove)
            {
                Canvas.Children.RemoveAt(Canvas.Children.Count - 1);
            }
            Canvas.Children.Add(ellipse);
        }

        private void DrawRectangle(MouseEventArgs e)
        {
            Rectangle rectangle = new Rectangle();
            MakeFigure(rectangle, e);
            if (!firstMouseMove)
            {
                Canvas.Children.RemoveAt(Canvas.Children.Count - 1);
            }
            Canvas.Children.Add(rectangle);
        }

        private void DrawLine(MouseEventArgs e)
        {
            Line line = new Line
            {
                X1 = firstPoint.X,
                Y1 = firstPoint.Y,
                X2 = e.GetPosition(Canvas).X,
                Y2 = e.GetPosition(Canvas).Y,
                Stroke = (Brush)new BrushConverter().ConvertFromString(((System.Reflection.PropertyInfo)ComboBox_PrimaryColor.SelectedItem).Name),
                StrokeThickness = Convert.ToDouble(TextBox_Thickness.Text)
            };
            if (!firstMouseMove)
            {
                Canvas.Children.RemoveAt(Canvas.Children.Count - 1);
            }
            Canvas.Children.Add(line);
        }

        void MakeFigure(Shape figure, MouseEventArgs e)
        {
            if (e.GetPosition(Canvas).X - firstPoint.X >= 0 && e.GetPosition(Canvas).Y - firstPoint.Y >= 0)
            {
                figure.Margin = new Thickness(firstPoint.X, firstPoint.Y, 0, 0);
            }
            if (e.GetPosition(Canvas).X - firstPoint.X < 0 && e.GetPosition(Canvas).Y - firstPoint.Y >= 0)
            {
                figure.Margin = new Thickness(e.GetPosition(Canvas).X, firstPoint.Y, 0, 0);
            }
            if (e.GetPosition(Canvas).X - firstPoint.X < 0 && e.GetPosition(Canvas).Y - firstPoint.Y < 0)
            {
                figure.Margin = new Thickness(e.GetPosition(Canvas).X, e.GetPosition(Canvas).Y, 0, 0);
            }
            if (e.GetPosition(Canvas).X - firstPoint.X >= 0 && e.GetPosition(Canvas).Y - firstPoint.Y < 0)
            {
                figure.Margin = new Thickness(firstPoint.X, e.GetPosition(Canvas).Y, 0, 0);
            }
            figure.Width = Math.Abs(e.GetPosition(Canvas).X - firstPoint.X);
            figure.Height = Math.Abs(e.GetPosition(Canvas).Y - firstPoint.Y);
            figure.Stroke = (Brush)new BrushConverter().ConvertFromString(((System.Reflection.PropertyInfo)ComboBox_PrimaryColor.SelectedItem).Name);
            figure.StrokeThickness = Convert.ToDouble(TextBox_Thickness.Text);
            if (CheckBox_Fill.IsChecked == true)
            {
                figure.Fill = (Brush)new BrushConverter().ConvertFromString(((System.Reflection.PropertyInfo)ComboBox_SecondaryColor.SelectedItem).Name);
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (action == "eraser")
            {
                drawing = false;
            }
        }

        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Canvas.Children.Count != 0)
            {
                if (drawing)
                {
                    var last = Canvas.Children[Canvas.Children.Count - 1];
                    if (last is Polyline)
                    {
                        ((Polyline)last).Points.RemoveAt(((Polyline)last).Points.Count - 1);
                    }
                    if (last is Polygon)
                    {
                        ((Polygon)last).Points.RemoveAt(((Polygon)last).Points.Count - 1);
                    }
                    drawing = false;
                }
                else
                {
                    if (action == "eraser")
                    {
                        if (eraserStack.Count != 0)
                        {
                            Canvas.Children.Remove(eraserStack.Pop());
                        }
                    }
                    else
                    {
                        Canvas.Children.RemoveAt(Canvas.Children.Count - 1);
                    }
                }
            }
        }

        private void Button_Line_Click(object sender, RoutedEventArgs e)
        {
            action = "line";
            TextBox_Thickness.Text = lastThickness;
            Canvas.Cursor = Cursors.Cross;
        }

        private void Button_Rectangle_Click(object sender, RoutedEventArgs e)
        {
            action = "rectangle";
            TextBox_Thickness.Text = lastThickness;
            Canvas.Cursor = Cursors.Cross;
        }

        private void Button_Ellipse_Click(object sender, RoutedEventArgs e)
        {
            action = "ellipse";
            TextBox_Thickness.Text = lastThickness;
            Canvas.Cursor = Cursors.Cross;
        }

        private void Button_Polyline_Click(object sender, RoutedEventArgs e)
        {
            action = "polyline";
            TextBox_Thickness.Text = lastThickness;
            Canvas.Cursor = Cursors.Cross;
        }

        private void Button_Polygon_Click(object sender, RoutedEventArgs e)
        {
            action = "polygon";
            TextBox_Thickness.Text = lastThickness;
            Canvas.Cursor = Cursors.Cross;
        }

        private void Button_Eraser_Click(object sender, RoutedEventArgs e)
        {
            action = "eraser";
            firstMouseMove = true;
            TextBox_Thickness.Text = "30";
            Canvas.Cursor = Cursors.None;
            eraserCursor.Width = Convert.ToDouble(TextBox_Thickness.Text);
            eraserCursor.Height = Convert.ToDouble(TextBox_Thickness.Text);
            eraserCursor.Fill = Canvas.Background;
            eraserCursor.StrokeThickness = 1;
            Color canvasColor = (Color)new ColorConverter().ConvertFrom(Canvas.Background.ToString());
            eraserCursor.Stroke = new SolidColorBrush(Color.FromArgb(canvasColor.A, (byte)~canvasColor.R, (byte)~canvasColor.G, (byte)~canvasColor.B));
            Canvas.Children.Add(eraserCursor);
        }

        private void DockPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            if (action == "eraser")
            {
                Canvas.Children.RemoveAt(Canvas.Children.Count - 1);
            }
        }

        private void TextBox_Thickness_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (action != "eraser")
            {
                lastThickness = TextBox_Thickness.Text;
            }
            else
            {
                lastEraserThickness = TextBox_Thickness.Text;
            }
        }

        private void Button_Background_Click(object sender, RoutedEventArgs e)
        {
            Canvas.Background = (Brush)new BrushConverter().ConvertFromString(((System.Reflection.PropertyInfo)ComboBox_SecondaryColor.SelectedItem).Name);
        }

        private void TextBox_Thickness_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(char.Parse(e.Text));
        }

        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog() { DefaultExt = "png" };
            if (saveFileDialog.ShowDialog() == true)
            {

                using (var fs = new FileStream(saveFileDialog.FileName, FileMode.Create))
                {
                    RenderTargetBitmap bmp = new RenderTargetBitmap((int)Canvas.ActualWidth, (int)Canvas.ActualHeight, 1 / 96, 1 / 96, PixelFormats.Default);
                    bmp.Render(Canvas);
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bmp));
                    encoder.Save(fs);
                }
            }
        }

        private void Button_Clear_Click(object sender, RoutedEventArgs e)
        {
            Canvas.Children.Clear();
        }
    }

}

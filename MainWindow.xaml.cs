using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SimpleDistr
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        //private int _numberOfModes;
        private readonly List<int> _levels = new List<int>();
/*
        private int _numberOfBindings;
*/

        private const int WidthOfUnit = 30;
        private const int WidthBetweenUnits = 60;
        private const int HeightBetweenUnits = 70;

        private readonly List<Unit> _allUnits = new List<Unit>();
        private readonly List<Binding> _allBindings = new List<Binding>();


        public MainWindow()
        {
            InitializeComponent();
        }

        private Unit PutEllipse(Point position, int nameIndex, int taskCompl, int level)
        {
            var ellipseGrid = new Grid
            {
                Height = WidthOfUnit,
                Width = WidthOfUnit,
                Name = "grid" + nameIndex,
                //Background = Brushes.AliceBlue,
                Margin = new Thickness(position.X, position.Y, 0, 0),
            };

            // RegisterName(ellipseGrid.Name, ellipseGrid);
            Panel.SetZIndex(ellipseGrid, 1);

            var ellipse = new Ellipse
            {
                Height = WidthOfUnit,
                Width = WidthOfUnit,
                StrokeThickness = 1,
                Stroke = Brushes.Black,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Fill = Brushes.White
            };

            var textBlock = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 12,
                Text = nameIndex + ":" + taskCompl,
            };

            ellipseGrid.Children.Add(ellipse);
            ellipseGrid.Children.Add(textBlock);
            MyCanvas.Children.Add(ellipseGrid);

            /*
            _collectionOfUnitGrids.Add(ellipseGrid);
            */

            return (new Unit()
            {
                Index = nameIndex,
                Position = position,
                Binds = new List<int>(),
                Complexity = taskCompl,
                Level = level,
                Name = ""
            });
        }

        private void PutLine(Unit u1, Unit u2, int nameIndex, int delay)
        {
            var width = u2.Position.X > u1.Position.X
                ? u2.Position.X - u1.Position.X + 10
                : u1.Position.X - u2.Position.X + 10;
            var height = u2.Position.Y - u1.Position.Y;

            var lineGrid = new Grid
            {
                Height = height,
                Width = width,
                Name = "GridLine" + nameIndex,
                Margin = new Thickness(
                    u1.Position.X < u2.Position.X 
                        ? u1.Position.X - 5 + WidthOfUnit * 0.5
                        : u2.Position.X - 5 + WidthOfUnit * 0.5,
                    u1.Position.Y + WidthOfUnit * 0.5, 0, 0),
                //Background = Brushes.Aqua,
                
            };

            // RegisterName(ellipseGrid.Name, ellipseGrid);
            Panel.SetZIndex(lineGrid, 0);

            var line = new Line()
            {
                X1 = u1.Position.X < u2.Position.X ? 0 : width-5,
                Y1 = 0,
                X2 = u1.Position.X > u2.Position.X ? 0 : width-5,
                Y2 = height,
                StrokeThickness = 2,
                Stroke = Brushes.Black,
            };
            
            Panel.SetZIndex(line, 0);

            var textBlock = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Background = Brushes.White,
                FontSize = 12,
                Text = delay.ToString(),
                Padding = new Thickness(2,2,2,2)
            };

            Panel.SetZIndex(textBlock, 1);
            lineGrid.Children.Add(line);
            lineGrid.Children.Add(textBlock);
            MyCanvas.Children.Add(lineGrid);
            
        }

        private Binding CreateBinding(Unit unit1, Unit unit2, int delay, int index)
        {
            PutLine(unit1, unit2, index, delay);

            var binding = new Binding()
            {
                Index = index,
                Delay = delay,
                Name = "",
                Units = new List<int>() { unit1.Index, unit2.Index },
            };

            return binding;
        }

        private void buttonAddNode_Click(object sender, RoutedEventArgs e)
        {
            var index = Convert.ToUInt16(textBoxTaskIndex.Text);
            var level = Convert.ToUInt16(textBoxTaskLevel.Text);
            var taskCompl = Convert.ToInt16(textBoxTaskComplexity.Text);

            if (_allUnits.Find(u => u.Index == index) != null)
                return;
            if (index != 0 && _allUnits.Find(u => u.Index == index - 1) == null)
                return;
            if (level != 0 && _allUnits.Find(u => u.Level == level - 1) == null)
                return;

            if (_levels.Count == level)
                _levels.Add(0);

            var marginTop = level * HeightBetweenUnits + 20;
            var marginLeft = _levels[level] * WidthBetweenUnits + 20;

            _allUnits.Add(PutEllipse(new Point(marginLeft, marginTop), index, taskCompl, level));

            _levels[level]++;
            //_numberOfModes++;
            textBoxTaskIndex.Text = (++index).ToString();
        }

        private void buttonBindNodes_Click(object sender, RoutedEventArgs e)
        {
            var index = _allBindings.Count;
            var delay = Convert.ToUInt16(textBoxBindDelay.Text);
            var u1Id = Convert.ToUInt16(textBoxBindFrom.Text);
            var u2Id = Convert.ToUInt16(textBoxBindTo.Text);

            var unit1 = _allUnits.Find(u => u.Index == u1Id);
            var unit2 = _allUnits.Find(u => u.Index == u2Id);

            if (unit1 == null || unit2 == null || unit1.Level >= unit2.Level)
                return;
            var binding = _allBindings.Find(b => b.Units.Contains(u1Id) && b.Units.Contains(u2Id));
            if (binding != null)
            {
                textBoxBindDelay.Text = binding.Delay.ToString();
                return;
            }

            _allBindings.Add(CreateBinding(unit1, unit2, delay, index));
            unit1.Binds.Add(_allBindings.Last().Index);
        }

        private void buttonTableCreate_Click(object sender, RoutedEventArgs e)
        {
            var proc = Convert.ToUInt16(textBoxProc.Text);
            if (proc < 1)
                return;

            var table = new Table(_allUnits, _allBindings, proc);
            table.Show();
        }

        private void textBoxProc_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
            {
                e.Handled = true;
            }
        }

        private void textBoxBindDelay_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
            {
                e.Handled = true;
            }
        }

        private void textBoxBindTo_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
            {
                e.Handled = true;
            }
        }

        private void textBoxBindFrom_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
            {
                e.Handled = true;
            }
        }

        private void textBoxTaskLevel_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
            {
                e.Handled = true;
            }
        }

        private void textBoxTaskComplexity_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
            {
                e.Handled = true;
            }
        }

        private void textBoxTaskIndex_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
            {
                e.Handled = true;
            }
        }
    }
}

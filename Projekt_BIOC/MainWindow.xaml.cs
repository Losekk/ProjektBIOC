using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Windows.Controls;


namespace Projekt_BIOC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private const int PopulationCount = 200;
        private readonly Location _startLocation = new Location(50, 50);
        private TravellingSalesmanAlgorithm _algorithm;
        private Location[] _bestSolutionSoFar;
        private List<Location> location = new List<Location>();
        private bool _onStart = false;
        private readonly object _algorithmLock = new object();
        private readonly object _lock = new object();
        private bool _messageWaitingToBeProcessed;
        private bool _closing;
        private bool _paused;
        private volatile bool _mutateDuplicates = true;
        private volatile bool _mustDoCrossovers = true;
        public Thread thread;
        private int _equalsSollutionsCounter = 0;

        public MainWindow()
        {
            InitializeComponent();

            buttonStart.Click += new RoutedEventHandler(buttonStart_Click);
            buttonResetSearch.Click += new RoutedEventHandler(buttonResetSearch_Click);
            buttonNewDestinations.Click += new RoutedEventHandler(buttonNewDestinations_Click);
            checkBoxDoCrossover.Checked += new RoutedEventHandler(checkBoxDoCrossover_Checked);
            checkBoxDoCrossover.Unchecked += new RoutedEventHandler(checkBoxDoCrossover_Unchecked);


        }

        void checkBoxDoCrossover_Checked(object sender, RoutedEventArgs e)
        {
            _mustDoCrossovers = true;
        }

        void checkBoxDoCrossover_Unchecked(object sender, RoutedEventArgs e)
        {
            _mustDoCrossovers = false;
        }


        private void canvas_Initialized(object sender, EventArgs e)
        {
            var circle = new Ellipse
            {
                Stroke = Brushes.Black,
                Fill = Brushes.Red,
                Width = 11,
                Height = 11
            };
            Canvas.SetLeft(circle, _startLocation.X - 5);
            Canvas.SetTop(circle, _startLocation.Y - 5);
            (sender as Canvas).Children.Add(circle);
        }

        private void canvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!_onStart)
            {
                var canvas = sender as Canvas;
                Point point = e.GetPosition(canvas);
                location.Add(new Location((int)point.X, (int)point.Y));

                var canvasChildren = canvas.Children;
                var circle = new Ellipse();
                circle.Stroke = Brushes.Black;
                circle.Fill = Brushes.Yellow;
                circle.Width = 11;
                circle.Height = 11;
                Canvas.SetLeft(circle, point.X - 5);
                Canvas.SetTop(circle, point.Y - 5);
                canvasChildren.Add(circle);
            }

        }

        void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            if (!_onStart & location.Count > 1)
            {
                _algorithm = new TravellingSalesmanAlgorithm(_startLocation, location.ToArray<Location>(), PopulationCount);

                _bestSolutionSoFar = _algorithm.GetBestSolutionSoFar().ToArray();
                _DrawLines();

                _onStart = true;
                _equalsSollutionsCounter = 0;

                if (thread == null)
                {
                    thread = new Thread(_Thread);
                    thread.Start();
                }
                else
                {
#pragma warning disable CS0618 
                    thread.Resume();
#pragma warning restore CS0618
                }
            }


        }

        void buttonNewDestinations_Click(object sender, RoutedEventArgs e)
        {
            if (_onStart)
            {
                location.Clear();
                var canvasChildren = canvas.Children;
                canvasChildren.Clear();
                _onStart = false;
                labelDistance.Content = 0;
                canvas_Initialized(canvas, e);
#pragma warning disable CS0618
                thread.Suspend();
#pragma warning restore CS0618
            }
        }

        void buttonResetSearch_Click(object sender, RoutedEventArgs e)
        {
            if (_onStart)
            {


                _equalsSollutionsCounter = 0;
                lock (_algorithmLock)
                    _algorithm = new TravellingSalesmanAlgorithm(_startLocation, location.ToArray<Location>(),
                        PopulationCount);

                _bestSolutionSoFar = _algorithm.GetBestSolutionSoFar().ToArray();
                _DrawLines();
            }
        }



        private void _Thread()
        {
            while (!_closing)
            {
                if (_paused)
                {
                    lock (_lock)
                    {
                        if (_closing)
                            return;

                        while (_paused)
                        {
                            Monitor.Wait(_lock);

                            if (_closing)
                                return;
                        }
                    }
                }

                lock (_algorithmLock)
                {

                    _algorithm.MustDoCrossovers = _mustDoCrossovers;
                    _algorithm.Reproduce();

                    if (_mutateDuplicates)
                        _algorithm.MutateDuplicates();

                    var newSolution = _algorithm.GetBestSolutionSoFar().ToArray();
                    if (!newSolution.SequenceEqual(_bestSolutionSoFar))
                    {

                        lock (_lock)
                        {
                            _bestSolutionSoFar = newSolution;

                            if (!_messageWaitingToBeProcessed)
                            {
                                _messageWaitingToBeProcessed = true;


                                Dispatcher.BeginInvoke(new Action(_DrawLines), DispatcherPriority.ApplicationIdle);
                            }
                        }


                        Thread.Sleep(150);
                    }
                    else
                    {


                        if (_equalsSollutionsCounter++ == 5000)
                        {
                            MessageBox.Show("Prawdopodobnie znaleziono najlepsze rozwiazanie!");
                        }

                    }
                }
            }
        }

        private void _DrawLines()
        {
            Location[] bestSolutionSoFar;
            lock (_lock)
            {
                _messageWaitingToBeProcessed = false;
                bestSolutionSoFar = _bestSolutionSoFar;
            }

            labelDistance.Content = (long)Location.GetTotalDistance(_startLocation, bestSolutionSoFar);

            var canvasChildren = canvas.Children;
            canvasChildren.Clear();

            var actualLocation = _startLocation;
            int index = 0;
            foreach (var destination in _AddEndLocation(bestSolutionSoFar))
            {
                int red = 255 * index / bestSolutionSoFar.Length;
                int blue = 255 - red;

                var line = new Line();
                var color = Color.FromRgb((byte)red, 0, (byte)blue);

                line.Stroke = new SolidColorBrush(color);
                line.X1 = actualLocation.X;
                line.Y1 = actualLocation.Y;
                line.X2 = destination.X;
                line.Y2 = destination.Y;
                canvasChildren.Add(line);

                var circle = new Ellipse();
                circle.Stroke = Brushes.Black;

                if (destination == _startLocation)
                    circle.Fill = Brushes.Red;
                else
                    circle.Fill = Brushes.Yellow;

                circle.Width = 11;
                circle.Height = 11;
                Canvas.SetLeft(circle, destination.X - 5);
                Canvas.SetTop(circle, destination.Y - 5);
                canvasChildren.Add(circle);

                actualLocation = destination;
                index++;
            }
        }

        private IEnumerable<Location> _AddEndLocation(Location[] middleLocations)
        {
            foreach (var location in middleLocations)
                yield return location;

            yield return _startLocation;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _closing = true;

            lock (_lock)
                Monitor.Pulse(_lock);
        }

    }
}


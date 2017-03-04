using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using Microsoft.Win32;
using System.ComponentModel;
using MotionDetection;



namespace MotionFinder
{
    using System.Collections.Generic;

    using OxyPlot;

    using OxyPlot.Axes;
    using OxyPlot.Series;

    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private int percent = 0;
        public int Percent
        {
            get { return this.percent; }
            set
            {
                this.percent = value;
                NotifyPropertyChanged("Percent");
            }
        }

        public MainViewModel()
        {
            this.Title = "Example 2";
            this.Points = new List<DataPoint>
                              {
                                  new DataPoint(0, 4),
                                  new DataPoint(10, 13),
                                  new DataPoint(20, 15),
                                  new DataPoint(30, 16),
                                  new DataPoint(40, 12),
                                  new DataPoint(50, 12)
                              };
            this.PlotModel = new PlotModel();
            

            LineSeries lineserie = new LineSeries
            {
                ItemsSource = Points,
                DataFieldX = "x",
                DataFieldY = "Y",
                StrokeThickness = 2,
                MarkerSize = 1,
                LineStyle = LineStyle.Solid,
                Color = OxyColors.Black,
                MarkerType = MarkerType.Circle,
            };

            this.PlotModel.Series.Add(lineserie);
            NotifyPropertyChanged("PlotModel");

        }

       public PlotModel PlotModel { get; private set; }

        public string Title { get; private set; }

        public IList<DataPoint> Points { get; private set; }


        private ICommand _clickCommand;
        public ICommand ClickCommand
        {
            get
            {
                return _clickCommand ?? (_clickCommand = new AsyncRelayCommand( x => {
                    return Task.Run(() => MyAction());
                   
                   
                }, new Func<object, bool>(x =>_canExecute)));
            }
        }
        private bool _canExecute = true;
        public async Task MyAction()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                var test = openFileDialog.FileName;
                this.Title = test;
                NotifyPropertyChanged("Title");
                var array = VideoAnalyser.GetByteArrayDifferencesAlternative(test, new Action<int>(x => Percent = x));

               this.Points = array.Select((x, i) => new DataPoint(DateTimeAxis.ToDouble(new DateTime(1,1,1,1,i/60,i % 60)), x)).ToList();

                this.PlotModel.InvalidatePlot(true);
                this.PlotModel = new PlotModel();
                LineSeries lineserie = new LineSeries
                {
                    ItemsSource = Points,
                    DataFieldX = "x",
                    DataFieldY = "Y",
                    StrokeThickness = 2,
                    MarkerSize = 1,
                    LineStyle = LineStyle.Solid,
                    Color = OxyColors.Black,
                    MarkerType = MarkerType.Circle,
                };
                this.PlotModel.Series.Clear();

                this.PlotModel.Axes.Add(new DateTimeAxis {
                    Position = AxisPosition.Bottom,
                    StringFormat = "mm:ss",
                    IntervalLength = 75,
                    MinorIntervalType = DateTimeIntervalType.Seconds,
                    IntervalType = DateTimeIntervalType.Minutes,
                    MajorGridlineStyle = LineStyle.Solid,
                    MinorGridlineStyle = LineStyle.None,
                                   });
                
                this.PlotModel.Series.Add(lineserie);
               
            }
            NotifyPropertyChanged("PlotModel");

        }

    }


  

}

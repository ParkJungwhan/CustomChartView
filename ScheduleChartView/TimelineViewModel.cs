using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Threading;

namespace ScheduleChartView
{
    public class TimelineViewModel : INotifyPropertyChanged
    {
        private readonly Random _rand = new();
        private readonly DispatcherTimer _timer;

        public ObservableCollection<SegmentVM> AllSegments { get; } = new();

        public double PixelsPerMs { get; set; } = 1.0;
        public double RowHeight { get; set; } = 60;
        public double SegmentHeight => Math.Max(12, RowHeight - 20);

        private double _canvasWidth = 900;

        public double CanvasWidth
        {
            get => _canvasWidth;
            private set { _canvasWidth = value; OnPropertyChanged(); }
        }

        private double _canvasHeight = 320;

        public double CanvasHeight
        {
            get => _canvasHeight;
            private set { _canvasHeight = value; OnPropertyChanged(); }
        }

        private int _barCounter = 0; // 바 번호 증가용

        public TimelineViewModel()
        {
            // DispatcherTimer로 랜덤 데이터 주입
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _timer.Tick += (s, e) => AddRandomSegment();
            _timer.Start();
        }

        private void AddRandomSegment()
        {
            int row = _rand.Next(0, 5); // 0~4번 라인
            double start = _rand.Next(0, 700);
            double duration = _rand.Next(80, 250);

            var colors = new[] { Colors.IndianRed, Colors.Gold, Colors.MediumTurquoise, Colors.RoyalBlue, Colors.MediumOrchid };
            var seg = new SegmentVM
            {
                Row = row,
                StartMs = start,
                DurationMs = duration,
                Brush = new SolidColorBrush(colors[row]),
                BarIndex = ++_barCounter
            };

            AllSegments.Add(seg);

            // 캔버스 크기 재계산
            double maxEnd = AllSegments.Max(s => s.StartMs + s.DurationMs);
            CanvasWidth = Math.Max(CanvasWidth, maxEnd * PixelsPerMs + 60);
            CanvasHeight = Math.Max(CanvasHeight, (row + 1) * RowHeight);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string n = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    public class SegmentVM
    {
        public int BarIndex { get; set; }
        public int Row { get; set; }
        public double StartMs { get; set; }
        public double DurationMs { get; set; }
        public Brush Brush { get; set; }
        public string Tooltip => $"{BarIndex}번 Bar, {DurationMs:0} ms 동안 진행";
    }
}
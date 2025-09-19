using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace ScheduleChartView
{
    public class TimelineViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<LaneVM> Lanes { get; } = new();
        public ObservableCollection<SegmentVM> AllSegments { get; } = new();

        // 렌더 파라미터
        private double _pixelsPerMs = 1.0;     // 1ms = 1px (원하면 조절)

        public double PixelsPerMs
        { get => _pixelsPerMs; set { _pixelsPerMs = value; OnPropertyChanged(); RecalcSize(); } }

        private double _rowHeight = 60;

        public double RowHeight
        { get => _rowHeight; set { _rowHeight = value; OnPropertyChanged(); OnPropertyChanged(nameof(SegmentHeight)); RecalcSize(); } }

        public double SegmentHeight => Math.Max(12, RowHeight - 20);

        private double _canvasWidth = 900;

        public double CanvasWidth
        { get => _canvasWidth; private set { _canvasWidth = value; OnPropertyChanged(); } }

        private double _canvasHeight = 320;

        public double CanvasHeight
        { get => _canvasHeight; private set { _canvasHeight = value; OnPropertyChanged(); } }

        public TimelineViewModel()
        {
            // 샘플 데이터: 그림과 유사하게 구성
            // Row 0 (빨강)
            Add(0, 0, 180, Colors.IndianRed);
            Add(0, 260, 180, Colors.IndianRed);
            Add(0, 540, 200, Colors.IndianRed);

            // Row 1 (노랑)
            Add(1, 40, 360, Colors.Gold);
            Add(1, 520, 180, Colors.Gold);

            // Row 2 (민트)
            Add(2, 20, 110, Colors.MediumTurquoise);
            Add(2, 150, 110, Colors.MediumTurquoise);
            Add(2, 290, 110, Colors.MediumTurquoise);
            Add(2, 500, 260, Colors.MediumTurquoise);

            // Row 3 (파랑)
            Add(3, 70, 380, Colors.RoyalBlue);

            // Row 4 (보라)
            Add(4, 460, 150, Colors.MediumOrchid);
            Add(4, 680, 150, Colors.MediumOrchid);

            RecalcSize();
        }

        private void Add(int row, double startMs, double durationMs, Color color)
        {
            var seg = new SegmentVM
            {
                Row = row,
                StartMs = startMs,
                DurationMs = durationMs,
                Brush = new SolidColorBrush(color)
            };
            AllSegments.Add(seg);

            var lane = Lanes.FirstOrDefault(l => l.Row == row);
            if (lane == null) { lane = new LaneVM(row); Lanes.Add(lane); }
            lane.Segments.Add(seg);
        }

        private void RecalcSize()
        {
            var maxEnd = AllSegments.Any()
                ? AllSegments.Max(s => s.StartMs + s.DurationMs)
                : 0;

            CanvasWidth = Math.Max(600, Math.Ceiling(maxEnd * PixelsPerMs) + 60);
            CanvasHeight = Math.Max(200, (Lanes.Count * RowHeight));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    public class LaneVM
    {
        public int Row { get; }
        public ObservableCollection<SegmentVM> Segments { get; } = new();

        public LaneVM(int row) => Row = row;
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
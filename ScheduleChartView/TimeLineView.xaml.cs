using System.Windows.Controls;

namespace ScheduleChartView
{
    /// <summary>
    /// TimeLineView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TimeLineView : UserControl
    {
        public TimeLineView()
        {
            InitializeComponent();

            this.DataContext = new TimelineViewModel();
        }
    }
}
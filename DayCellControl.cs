using System;
using System.Drawing;
using System.Windows.Forms;

namespace personelizintakip
{
    public partial class DayCellControl : UserControl
    {
        public DateTime Date { get; set; }

        // Backing fields ve özellikler
        private bool isSunday;
        public bool IsSunday
        {
            get { return isSunday; }
            set
            {
                if (isSunday != value)
                {
                    isSunday = value;
                    this.Invalidate(); // Kontrolü yeniden çiz
                }
            }
        }

        private bool isLeftMarked;
        public bool IsLeftMarked
        {
            get { return isLeftMarked; }
            set
            {
                if (isLeftMarked != value)
                {
                    isLeftMarked = value;
                    this.Invalidate(); // Kontrolü yeniden çiz
                }
            }
        }

        private bool isRightMarked;
        public bool IsRightMarked
        {
            get { return isRightMarked; }
            set
            {
                if (isRightMarked != value)
                {
                    isRightMarked = value;
                    this.Invalidate(); // Kontrolü yeniden çiz
                }
            }
        }

        // Tatil günleri kontrolü
        private bool isHoliday;
        public bool IsHoliday
        {
            get { return isHoliday; }
            set
            {
                if (isHoliday != value)
                {
                    isHoliday = value;
                    this.Invalidate();
                }
            }
        }

        private bool isFullDayHoliday;
        public bool IsFullDayHoliday
        {
            get { return isFullDayHoliday; }
            set
            {
                if (isFullDayHoliday != value)
                {
                    isFullDayHoliday = value;
                    this.Invalidate();
                }
            }
        }

        private bool isHalfDayHoliday;
        public bool IsHalfDayHoliday
        {
            get { return isHalfDayHoliday; }
            set
            {
                if (isHalfDayHoliday != value)
                {
                    isHalfDayHoliday = value;
                    this.Invalidate();
                }
            }
        }

        // İşaretlemelerin değiştiğini bildiren olay
        public event EventHandler MarkingChanged;

        public DayCellControl()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.MouseClick += DayCellControl_MouseClick;
            this.Resize += DayCellControl_Resize;
        }

        private void DayCellControl_Resize(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Küpün boyutunu ayarlayalım
            int size = Math.Min(this.Width, this.Height) - 10;
            int x = (this.Width - size) / 2;
            int y = (this.Height - size) / 2;
            Rectangle cubeRect = new Rectangle(x, y - 5, size, size);

            // Varsayılan arka plan rengi gri
            Color cubeBackgroundColor = Color.Gray;

            // Tatil ve Pazar kontrolü
            if (IsHoliday)
            {
                if (IsFullDayHoliday)
                {
                    cubeBackgroundColor = Color.MediumPurple;
                }
                else if (IsHalfDayHoliday)
                {
                    cubeBackgroundColor = Color.LightPink;
                }
            }
            else if (IsSunday)
            {
                cubeBackgroundColor = Color.Green;
            }

            // Arka planı boyayalım
            e.Graphics.FillRectangle(new SolidBrush(cubeBackgroundColor), cubeRect);
            e.Graphics.DrawRectangle(Pens.Black, cubeRect);

            // Kırmızı çizgiyi ekleyelim
            using (Pen diagonalPen = new Pen(Color.Red, 2))
            {
                e.Graphics.DrawLine(diagonalPen, cubeRect.Left, cubeRect.Bottom, cubeRect.Right, cubeRect.Top);
            }

            // İşaretlemeleri çizelim
            if (IsLeftMarked)
            {
                Point[] leftTriangle = new Point[]
                {
                    new Point(cubeRect.Left, cubeRect.Bottom),
                    new Point(cubeRect.Right, cubeRect.Top),
                    new Point(cubeRect.Right, cubeRect.Bottom)
                };
                e.Graphics.FillPolygon(new SolidBrush(Color.Red), leftTriangle);
            }

            if (IsRightMarked)
            {
                Point[] rightTriangle = new Point[]
                {
                    new Point(cubeRect.Left, cubeRect.Bottom),
                    new Point(cubeRect.Left, cubeRect.Top),
                    new Point(cubeRect.Right, cubeRect.Top)
                };
                e.Graphics.FillPolygon(new SolidBrush(Color.Red), rightTriangle);
            }

            // Gün numarasını yazalım
            string dayNumber = Date.Day.ToString();
            using (Font boldFont = new Font(this.Font, FontStyle.Bold))
            {
                SizeF textSize = e.Graphics.MeasureString(dayNumber, boldFont);
                PointF textLocation = new PointF(
                    cubeRect.Left + (cubeRect.Width - textSize.Width) / 2,
                    cubeRect.Top + (cubeRect.Height - textSize.Height) / 2);
                e.Graphics.DrawString(dayNumber, boldFont, Brushes.Black, textLocation);
            }
        }

        private void DayCellControl_MouseClick(object sender, MouseEventArgs e)
        {
            int size = Math.Min(this.Width, this.Height) - 10;
            int x = (this.Width - size) / 2;
            int y = (this.Height - size) / 2;
            Rectangle cubeRect = new Rectangle(x, y - 5, size, size); // 5 piksel yukarı kaydırma

            // Tıklama noktası küpün içindeyse işlem yapalım
            if (cubeRect.Contains(e.Location))
            {
                // Tıklanan noktanın, çizgiye göre konumunu belirleyelim
                if ((e.X - cubeRect.Left) + (e.Y - cubeRect.Top) > size)
                {
                    // Sol alt üçgen (çizginin alt tarafı)
                    IsLeftMarked = !IsLeftMarked;
                }
                else
                {
                    // Sağ üst üçgen (çizginin üst tarafı)
                    IsRightMarked = !IsRightMarked;
                }

                // İşaretlemeler değişti, olayı tetikle
                MarkingChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mmswitcherAPI.Extensions
{
    public static class DrawingExtension
    {
        public static System.Windows.Rect ConvertToRect(this System.Drawing.Rectangle value)
        {
            return new System.Windows.Rect(value.X, value.Y, value.Width, value.Height);
        }
    }
}

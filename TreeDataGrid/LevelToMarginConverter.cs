﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ReqIF_Editor.TreeDataGrid
{
    public class LevelToMarginConverter : IValueConverter
    {

        public int LeftMargin { get; set; }
        public int OtherMargin { get; set; }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int lvl = (int)value;
            return new Thickness(6 * lvl, 1, 1, 1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

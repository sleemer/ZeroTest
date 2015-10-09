using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ZeroMainWpf.UI
{
    public class NotificationObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        protected void RaisePropertyChanged<TProp>(Expression<Func<TProp>> propertyExpression)
        {
            var memberExpression = propertyExpression.Body as MemberExpression;
            string propertyName = memberExpression.Member.Name;
            RaisePropertyChanged(propertyName);
        }
    }
}

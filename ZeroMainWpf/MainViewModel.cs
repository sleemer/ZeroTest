using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeroMainWpf
{
    public class MainViewModel
    {
        public MainViewModel()
        {
            ImageStreamClientViewModel = new ImageStreamClientViewModel();
        }

        public ImageStreamClientViewModel ImageStreamClientViewModel { get; private set; }
    }
}

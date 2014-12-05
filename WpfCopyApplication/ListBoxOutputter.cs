using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace WpfCopyApplication
{
    public class ListBoxOutputter : TextWriter
    {
        private ListBox list;
        private StringBuilder content = new StringBuilder();

        public ListBoxOutputter(ListBox list)
        {
            this.list = list;
        }

        public override void Write(char value)
        {
            base.Write(value);
            content.Append(value);
            if (value == '\n')
            {
                list.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => list.Items.Add(content.ToString())));                    
                content = new StringBuilder();
            }
        }

        public override Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }
    }
}

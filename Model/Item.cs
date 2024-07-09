using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenameTool.Model
{
    internal class Item
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Location { get; set; }

        public string NewFullName { get; set; }

        public string GetFullName() { return Name+Extension;}
        public string GetFullPath() { return Location + "\\" + GetFullName(); }    
        public Item Parent { get; set; }


    }
}

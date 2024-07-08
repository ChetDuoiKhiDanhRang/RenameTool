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
        public string FullName { get => Name+Extension;}
        public string FullPath { get => Location + "\\" + FullName; }    
        public Item Parent { get; set; }


    }
}

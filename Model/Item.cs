using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenameTool.Model
{
    public class Item
    {
        public string Location { get; set; }
        public bool WillBeApply { get; set; }
        public bool IsFile { get; set; }
        public int RootLevel { get; set; } = 0;
        public int Level { get; set; } = 0;

        private string orderString;

        public string OrderString
        {
            get { return orderString; }
            set { orderString = value; }
        }



        public string Name { get; set; }
        public string Extension { get; set; }
        public string FullName { get => Name + Extension; }


        public string NewName { get; set; }// = "zzz";
        public string NewExtension { get; set; }// = ".xxx";
        public string NewFullName { get => NewName + NewExtension; }
        public Item(string FullPath)
        {
            if (FullPath.EndsWith(@"\")) { FullPath = FullPath.Remove(FullPath.Length - 1); }
            if (File.Exists(FullPath))
            {
                IsFile = true;
                Name = NewName = Path.GetFileNameWithoutExtension(FullPath);
                Extension = NewExtension = Path.GetExtension(FullPath);
            }
            else
            {
                IsFile = false;
                Name = Path.GetFileName(FullPath);
                Extension = "";
            }
            Location = Path.GetDirectoryName(FullPath);
            WillBeApply = true;
            string str = GetFullPath();
            while (str.Contains(@"\\")) { str = str.Replace(@"\\", @"\"); }
            str = str.EndsWith('\\') ? str.Remove(str.Length - 1) : str;
            Level = str.Count<char>(x => x == '\\');
            RootLevel = Level;
            OrderString = "";
        }

        public string GetFullName() { return Name + Extension; }
        public string GetFullPath() { return Location + "\\" + GetFullName(); }


    }
}

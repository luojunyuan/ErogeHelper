using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace ErogeHelper.Model.DataModels
{
    public class ProcessDataModel : IEquatable<ProcessDataModel>
    {
        public ProcessDataModel(Process process, BitmapImage icon, string title)
        {
            Proc = process;
            Icon = icon;
            Title = title;
        }

        public Process Proc { get; }

        public BitmapImage Icon { get; }

        public string Title { get; }

        public bool Equals(ProcessDataModel? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Proc.Id == other.Proc.Id;
        }

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ProcessDataModel)obj);
        }

        public override int GetHashCode() => Title.GetHashCode();
    }
}

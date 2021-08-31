using System;
using System.Diagnostics;
using System.Windows.Media.Imaging;

namespace ErogeHelper.SelectProcess
{
    public class ProcessDataModel : IEquatable<ProcessDataModel>
    {
        public ProcessDataModel(Process process, BitmapImage icon, string descript, string title)
        {
            Proc = process;
            Icon = icon;
            Descript = descript;
            Title = title;
        }

        public Process Proc { get; }

        public BitmapImage Icon { get; }

        public string Descript { get; }

        public string Title { get; }

        public bool Equals(ProcessDataModel? other) =>
            other is not null &&
            (ReferenceEquals(this, other) || Proc.Id == other.Proc.Id);

        #region Override
        public override bool Equals(object? obj) =>
            obj is not null &&
            (ReferenceEquals(this, obj) || (obj.GetType() == GetType() && Equals(other: (ProcessDataModel)obj)));

        public override int GetHashCode() => Proc.Id.GetHashCode();
        #endregion
    }
}

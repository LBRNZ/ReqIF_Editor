using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReqIF_Editor.TreeDataGrid
{
    public class RowDef
    {
        public event EventHandler<RowDef> RowExpanding;
        public event EventHandler<RowDef> RowCollapsing;

        public RowDef()
        {
            Cells = new SpecobjectViewModel();
            Children = new List<RowDef>();
        }

        public RowDef(RowDef parent)
            : this()
        {
            Parent = parent;
        }

        //TODO: Probably should have another class defining Cell, in case you want something more sophisticated than just a string
        public SpecobjectViewModel Cells { get; internal set; }

        bool? _isExpanded;
        public bool? IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    if (_isExpanded.Value)
                    {
                        RowExpanding?.Invoke(this, this);
                    }
                    else
                    {
                        RowCollapsing?.Invoke(this, this);
                    }
                }
            }
        }

        public List<RowDef> Children { get; set; }

        private RowDef _parent;
        public RowDef Parent
        {
            get { return _parent; }
            private set
            {
                _parent = value;
                if (_parent != null)
                    Level = _parent.Level + 1;
            }
        }

        public int Level { get; set; }
        public bool IsVisible { get; set; }
    }
}

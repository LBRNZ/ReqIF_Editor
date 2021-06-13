using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReqIFSharp;

namespace ReqIF_Editor.TreeDataGrid
{
    public class GridDef
    {
        public GridDef(Specification specification)
        {
            Source = new ObservableCollection<RowDef>();
            if (specification.Children.Any())
            {
                LoadData(Source, specification.Children, null);
                Source[0].IsVisible = true;
            }

        }

        public ObservableCollection<RowDef> Source { get; }

        public ObservableCollection<RowDef> Display
        {
            get
            {
                //TODO: How to do this with multiple roots?
                return new ObservableCollection<RowDef>(IterateTree(Source[0]));
            }
        }

        private void RowDef_RowExpanding(object sender, RowDef row)
        {
            foreach (RowDef child in row.Children)
            {

                if (row.IsExpanded.HasValue && row.IsExpanded.Value)
                {
                    child.IsVisible = true;
                    RowDef_RowExpanding(this, child);
                }
            }
        }

        private void RowDef_RowCollapsing(object sender, RowDef row)
        {
            foreach (RowDef child in row.Children)
            {
                child.IsVisible = false;
                RowDef_RowCollapsing(this, child);

            }
        }

        private int LoadData(IList<RowDef> srce, List<SpecHierarchy> specification, RowDef parent)
        {
            int count = 0;
            foreach (SpecHierarchy sh in specification)
            {
                SpecObject specObject = sh.Object;
                SpecobjectViewModel specobjectViewModel = new SpecobjectViewModel()
                {
                    Identifier = specObject.Identifier,
                    AlternativeId = specObject.AlternativeId,
                    Description = specObject.Description,
                    LastChange = specObject.LastChange,
                    LongName = specObject.LongName,
                    Type = specObject.Type,
                    SpecType = specObject.SpecType
                };
                foreach (AttributeDefinition attributeDefinition in specObject.SpecType.SpecAttributes)
                {
                    AttributeValue attributeValue = specObject.Values.Where(x => x.AttributeDefinition == attributeDefinition).FirstOrDefault();

                    specobjectViewModel.Values.Add(new AttributeValueViewModel() { AttributeValue = attributeValue, AttributeDefinition = attributeDefinition });
                }




                RowDef row = InitRow(parent, specobjectViewModel);
                srce.Add(row);
                ++count;

                int children = LoadData(row.Children, sh.Children, row);
                if (children > 0)
                    row.IsExpanded = true;
            }
            return count;
        }

        private RowDef InitRow(RowDef parent, SpecobjectViewModel values)
        {
            RowDef row = new RowDef(parent)
            {
                IsVisible = false,
                Cells = values,
            };

            row.RowCollapsing += RowDef_RowCollapsing;
            row.RowExpanding += RowDef_RowExpanding;

            return row;
        }

        private IEnumerable<RowDef> IterateTree(RowDef parent)
        {
            if (!parent.IsVisible)
                yield break;
            yield return parent;
            foreach (RowDef child in parent.Children)
            {
                foreach (RowDef r in IterateTree(child))
                {
                    yield return r;
                }
            }
        }
    }
}

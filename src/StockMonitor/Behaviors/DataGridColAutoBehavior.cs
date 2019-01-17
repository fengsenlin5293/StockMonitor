using CommonHelpers;
using Models.Stock;
using Structures.Attibutes;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace StockMonitor.Behaviors
{
    public class DataGridColAutoBehavior : Behavior<DataGrid>
    {
        private readonly List<DataGridColumn> _dataGridColumns = new List<DataGridColumn>();

        protected override void OnAttached()
        {
            base.OnAttached();

            foreach (var column in AssociatedObject.Columns)
            {
                _dataGridColumns.Add(column);
            }

            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;
        }

        private void AssociatedObject_Unloaded(object sender, RoutedEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid == null)
                return;
            var itemsource = dataGrid.ItemsSource as INotifyCollectionChanged;
            if (itemsource != null)
            {
                itemsource.CollectionChanged -= Itemsource_CollectionChanged;
            }

        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid == null)
                return;
            var itemsource = dataGrid.ItemsSource as INotifyCollectionChanged;
            if (itemsource != null)
            {
                itemsource.CollectionChanged += Itemsource_CollectionChanged;
            }
            dataGrid.Columns.Clear();
            if (dataGrid.Items != null && dataGrid.Items.Count > 0)
            {
                var attrs = GetColumnDescriptionAttributes(dataGrid.Items[0].GetType());
                foreach (var attr in attrs)
                {
                    DataGridTextColumn dataGridTextColumn = new DataGridTextColumn();
                    dataGridTextColumn.Header = ResourceHelper.FindKey(attr.ColumnHeaderName) ?? "";
                    dataGridTextColumn.Binding = new Binding(attr.ColumnPropertyName);
                    dataGridTextColumn.SetValue(DataGridColumn.WidthProperty, new DataGridLength(1, DataGridLengthUnitType.Star));
                    dataGrid.Columns.Add(dataGridTextColumn);
                }
            }

            var models = dataGrid.Items.SourceCollection.Cast<StockModel>();
            if (models != null && models.Any())
            {
                int count = models.Max(x => x.TopicModels.Count);
                _maxCountTC = count;
                var att = GetColumnDescriptionAttributes(typeof(TopicModel));
                if (!att.Any()) return;
                var temp = att.ElementAt(0) as ColumnDescriptionAttribute;
                for (int i = 0; i < count; i++)
                {
                    DataGridTextColumn dataGridTextColumn = new DataGridTextColumn();
                    dataGridTextColumn.Header = ResourceHelper.FindKey(temp.ColumnHeaderName) + i;
                    dataGridTextColumn.Foreground = Brushes.Red;
                    dataGridTextColumn.Binding = new Binding($"TopicModels[{i}].{temp.ColumnPropertyName}");
                    dataGridTextColumn.SetValue(DataGridColumn.WidthProperty, new DataGridLength(1, DataGridLengthUnitType.Star));
                    if (!dataGrid.Columns.Any(x => x.Header.ToString().Equals(dataGridTextColumn.Header.ToString())))
                        dataGrid.Columns.Add(dataGridTextColumn);
                }
            }

        }

        private int _maxCountTC = 0;

        private IEnumerable<ColumnDescriptionAttribute> GetColumnDescriptionAttributes(Type t)
        {
            List<ColumnDescriptionAttribute> result = new List<ColumnDescriptionAttribute>();
            var properties = t.GetProperties();
            foreach (var property in properties)
            {
                var atts = property.GetCustomAttributes(typeof(ColumnDescriptionAttribute), true);
                if (atts.Any())
                {
                    result.Add(atts[0] as ColumnDescriptionAttribute);
                }
            }
            return result;

        }

        private void Itemsource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var dataGrid = AssociatedObject;
            if (dataGrid.Items != null && dataGrid.Items.Count > 0)
            {
                var attrs = GetColumnDescriptionAttributes(dataGrid.Items[0].GetType());
                foreach (var attr in attrs)
                {
                    if (dataGrid.Columns.Any(x => x.Header.Equals(ResourceHelper.FindKey(attr.ColumnHeaderName))))
                        continue;

                    DataGridTextColumn dataGridTextColumn = new DataGridTextColumn();
                    dataGridTextColumn.Header = ResourceHelper.FindKey(attr.ColumnHeaderName) ?? "";
                    dataGridTextColumn.Binding = new Binding(attr.ColumnPropertyName);
                    dataGridTextColumn.SetValue(DataGridColumn.WidthProperty, new DataGridLength(1, DataGridLengthUnitType.Star));
                    dataGrid.Columns.Add(dataGridTextColumn);
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var models = dataGrid.Items.SourceCollection.Cast<StockModel>();
                if (models != null && models.Any())
                {
                    int count = models.Max(x => x.TopicModels.Count);
                    if (count <= _maxCountTC)
                    {
                        for (int i = 0; i < _maxCountTC - count; i++)
                        {
                            var last = dataGrid.Columns.LastOrDefault();
                            if (last != null) dataGrid.Columns.Remove(last);
                        }
                        _maxCountTC = count;
                        return;
                    }

                    _maxCountTC = count;
                    var att = GetColumnDescriptionAttributes(typeof(TopicModel));
                    if (!att.Any()) return;
                    var temp = att.ElementAt(0) as ColumnDescriptionAttribute;
                    for (int i = 0; i < count; i++)
                    {
                        DataGridTextColumn dataGridTextColumn = new DataGridTextColumn();
                        dataGridTextColumn.Foreground = Brushes.Red;
                        dataGridTextColumn.Header = ResourceHelper.FindKey(temp.ColumnHeaderName) + i;
                        dataGridTextColumn.Binding = new Binding($"TopicModels[{i}].{temp.ColumnPropertyName}");
                        dataGridTextColumn.SetValue(DataGridColumn.WidthProperty, new DataGridLength(1, DataGridLengthUnitType.Star));
                        if (!dataGrid.Columns.Any(x => x.Header.ToString().Equals(dataGridTextColumn.Header.ToString())))
                            dataGrid.Columns.Add(dataGridTextColumn);
                    }
                }
            }
        }
    }
}

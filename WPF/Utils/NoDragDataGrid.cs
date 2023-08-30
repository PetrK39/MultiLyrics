using System.Reflection;
using System.Windows.Controls;
using System.Windows.Input;

namespace WPF.Utils
{
    public class NoDragDataGrid : DataGrid
    {
        private static readonly FieldInfo s_isDraggingSelectionField =
        typeof(DataGrid).GetField("_isDraggingSelection", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly MethodInfo s_endDraggingMethod =
            typeof(DataGrid).GetMethod("EndDragging", BindingFlags.Instance | BindingFlags.NonPublic);

        // DataGrid.OnMouseMove() serves no other purpose than to execute click-drag-selection.
        // Bypass that, and stop 'is dragging selection' mode for DataGrid
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if ((bool)(s_isDraggingSelectionField?.GetValue(this) ?? false))
                s_endDraggingMethod.Invoke(this, new object[0]);
        }

        public event SelectionChangedEventHandler NewSelectionChanged;

        public event MouseWheelEventHandler NewPreviewMouseWheel;

        public NoDragDataGrid()
        {
            this.SelectionChanged += NoDragDataGrid_SelectionChanged;
            this.PreviewMouseWheel += NoDragDataGrid_PreviewMouseWheel;
        }

        private void NoDragDataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            NewPreviewMouseWheel?.Invoke(sender, e);
        }

        private void NoDragDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NewSelectionChanged?.Invoke(sender, e);
        }
    }
}
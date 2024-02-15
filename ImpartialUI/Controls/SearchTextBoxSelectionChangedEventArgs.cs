using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace ImpartialUI.Controls
{
    public class SearchTextBoxSelectionChangedEventArgs : SelectionChangedEventArgs
    {
        public int Placement { get; set; }

        public SearchTextBoxSelectionChangedEventArgs(RoutedEvent id, IList removedItems, IList addedItems, int placement = 0) : base(id, removedItems, addedItems)
        {
            Placement = placement;
        }
    }

    public delegate void SearchTextBoxSelectionChangedEventHandler(object sender, SearchTextBoxSelectionChangedEventArgs e);
}

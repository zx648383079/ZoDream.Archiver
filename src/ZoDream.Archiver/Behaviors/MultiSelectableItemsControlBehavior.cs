using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.Xaml.Interactivity;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ZoDream.Archiver.ViewModels;
using ZoDream.Shared.Collections;

namespace ZoDream.Archiver.Behaviors
{
    public class MultiSelectableItemsControlBehavior : Behavior<Selector>
    {


        public EntryViewModel[] SelectedItems {
            get { return (EntryViewModel[])GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(EntryViewModel[]), 
                typeof(MultiSelectableItemsControlBehavior), 
                new PropertyMetadata(null, OnSelectionChanged));

        private static void OnSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //if (d is MultiSelectableItemsControlBehavior bhv)
            //{
            //    if (bhv.AssociatedObject is ListViewBase ctl 
            //        && ctl.SelectionMode >= ListViewSelectionMode.Multiple)
            //    {
            //        ctl.SelectedItems.Clear();
            //        if (bhv.SelectedItems is not null)
            //        {
            //            ctl.SelectedItems.AddRange(bhv.SelectedItems);
            //        }
            //    }
            //    else if (bhv.AssociatedObject is Selector selector)
            //    {
            //        selector.SelectedItem = bhv.SelectedItems.FirstOrDefault();
            //    }
            //}
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
        }

        private void AssociatedObject_SelectionChanged(object sender, Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs e)
        {
            if (sender is ListViewBase ctl)
            {
                SelectedItems = ctl.SelectedItems.Select(i => (EntryViewModel)i).ToArray();
            } 
            else if (sender is Selector selector && selector.SelectedItem is not null)
            {
                SelectedItems = [(EntryViewModel)selector.SelectedItem];
            }
        }


    }
}

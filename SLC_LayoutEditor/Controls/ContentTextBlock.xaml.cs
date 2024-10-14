﻿using System.Windows;
using System.Windows.Controls;

namespace SLC_LayoutEditor.Controls
{
    /// <summary>
    /// Interaction logic for ContentTextBlock.xaml
    /// </summary>
    public partial class ContentTextBlock : Grid
    {
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(ContentTextBlock), new PropertyMetadata(null));

        public string Content
        {
            get { return (string)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CheckedText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(string), typeof(ContentTextBlock), new PropertyMetadata(null));

        public double MaxContentWidth
        {
            get { return (double)GetValue(MaxContentWidthProperty); }
            set { SetValue(MaxContentWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxContentWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxContentWidthProperty =
            DependencyProperty.Register("MaxContentWidth", typeof(double), typeof(ContentTextBlock), new PropertyMetadata(double.NaN));

        public ContentTextBlock()
        {
            InitializeComponent();
        }
    }
}

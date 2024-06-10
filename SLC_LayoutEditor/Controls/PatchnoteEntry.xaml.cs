using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SLC_LayoutEditor.Controls
{
    /// <summary>
    /// Interaction logic for PatchnoteEntry.xaml
    /// </summary>
    public partial class PatchnoteEntry : Grid
    {
        public string Intro
        {
            get { return (string)GetValue(IntroProperty); }
            set { SetValue(IntroProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Intro.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IntroProperty =
            DependencyProperty.Register("Intro", typeof(string), typeof(PatchnoteEntry), new PropertyMetadata(null));

        public string Content
        {
            get { return (string)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Content.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(string), typeof(PatchnoteEntry), new PropertyMetadata(null));

        public Brush IntroForeground
        {
            get { return (Brush)GetValue(IntroForegroundProperty); }
            set { SetValue(IntroForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IntroForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IntroForegroundProperty =
            DependencyProperty.Register("IntroForeground", typeof(Brush), typeof(PatchnoteEntry), new PropertyMetadata(FixedValues.DEFAULT_BRUSH));

        public bool IsIntroUnderlined
        {
            get { return (bool)GetValue(IsIntroUnderlinedProperty); }
            set { SetValue(IsIntroUnderlinedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsIntroUnderlined.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsIntroUnderlinedProperty =
            DependencyProperty.Register("IsIntroUnderlined", typeof(bool), typeof(PatchnoteEntry), new PropertyMetadata(false));

        public bool HasBetaTag
        {
            get { return (bool)GetValue(HasBetaTagProperty); }
            set { SetValue(HasBetaTagProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HasBetaTag.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HasBetaTagProperty =
            DependencyProperty.Register("HasBetaTag", typeof(bool), typeof(PatchnoteEntry), new PropertyMetadata(false));

        public PatchnoteEntry(string line)
        {
            InitializeComponent();

            string[] change = line.Split(new char[] { ':' }, 2);
            if (change.Length >= 2)
            {
                Intro = change[0];
                IsIntroUnderlined = change[0].ToLower().Contains("re-enabled");
                IntroForeground = GetNoteBrush(change[0]);

                string text = change[1].Trim();
                HasBetaTag = text.StartsWith("(BETA)");
                Content = !HasBetaTag ? text : text.Substring(7);
            }
            else
            {
                Content = line;
            }
        }

        private Brush GetNoteBrush(string type)
        {
            switch (type.ToLower())
            {
                case "added":
                case "re-enabled":
                    return FixedValues.PATCH_ADDED_BRUSH;
                case "changed":
                case "updated":
                    return FixedValues.PATCH_CHANGED_BRUSH;
                case "fixed":
                    return FixedValues.PATCH_FIXED_BRUSH;
                case "disabled":
                    return FixedValues.PATCH_DISABLED_BRUSH;
                case "removed":
                    return FixedValues.PATCH_REMOVED_BRUSH;
                default:
                    return FixedValues.DEFAULT_BRUSH;
            }
        }
    }
}

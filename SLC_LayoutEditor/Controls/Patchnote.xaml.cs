﻿using SLC_LayoutEditor.Core.Patcher;
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
    /// Interaction logic for Patchnote.xaml
    /// </summary>
    public partial class Patchnote : DockPanel
    {
        private string version;

        public Patchnote()
        {
            InitializeComponent();

            /*Mediator.Instance.Register((object o) =>
            {
                Dispatcher.Invoke(() =>
                {
                    Initialize((ChangelogEntry)o);
                });
            }, ChangelogViewModelMessage.PatchnoteChanged);*/
        }

        /// <summary>
        /// Initializes the patchnote control with the specified <see cref="PatchData"/>.
        /// </summary>
        /// <param name="data">The <see cref="PatchData"/> which should be used</param>
        public void Initialize(ChangelogEntry data)
        {
            version = data.PatchNumber;
            PatchDate = data.PatchDate.ToString("dd. MMMM yyyy");

            Nightly = data.IsSketch ? FontStyles.Italic : FontStyles.Normal;
            IsHotfix = data.IsHotfix;

            rtb_patchnotes.Document = BuildPatchnote(data.PatchContent);

            PatchNumber = string.Format("v{0}{1}", version, data.IsSketch ? " Sketch" : "");
        }

        private FlowDocument BuildPatchnote(string[] content)
        {
            FlowDocument doc = new FlowDocument();

            if (content != null)
            {
                if (!version.Contains("n") && !version.Contains("s"))
                {
                    for (int i = 0; i < content.Length; i++)
                    {
                        string[] change = content[i].Split(new char[] { ':' }, 2);
                        if (change.Length >= 2)
                        {
                            Run header = new Run(change[0] + ":")
                            {
                                FontWeight = FontWeights.SemiBold
                            };
                            if (change[0].ToLower().Contains("re-enabled"))
                                header.TextDecorations = TextDecorations.Underline;
                            header.Foreground = GetNoteBrush(change[0]);

                            Paragraph p = new Paragraph
                            {
                                Margin = new Thickness(0, 0, 0, 4)
                            };
                            p.Inlines.Add(header);
                            p.Inlines.Add(change[1]);
                            doc.Blocks.Add(p);
                        }
                        else
                        {
                            doc.Blocks.Add(new Paragraph(new Run(content[i]))
                            {
                                Margin = new Thickness(0)
                            });
                        }
                    }
                }
                else
                {
                    Run text = new Run("This is a secret for now...")
                    {
                        FontSize = 20,
                        BaselineAlignment = BaselineAlignment.Center
                    };

                    Paragraph p = new Paragraph();
                    p.Inlines.Add(text);

                    doc.Blocks.Add(p);
                }
            }

            doc.PageWidth = 576;
            return doc;
        }

        private Brush GetNoteBrush(string type)
        {
            switch (type.ToLower())
            {
                case "added":
                case "re-enabled":
                    return FixedValues.PATCH_ADDED_BRUSH;
                case "changed":
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

        #region PatchNumberRaw
        public string PatchNumber
        {
            get { return (string)GetValue(PatchNumberProperty); }
            set { SetValue(PatchNumberProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PatchNumber.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PatchNumberProperty =
            DependencyProperty.Register("PatchNumber", typeof(string), typeof(Patchnote), new PropertyMetadata("UNKNOWN"));
        #endregion

        #region Version formatter
        public FontStyle Nightly
        {
            get { return (FontStyle)GetValue(NightlyProperty); }
            set { SetValue(NightlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Nightly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NightlyProperty =
            DependencyProperty.Register("Nightly", typeof(FontStyle), typeof(Patchnote), new PropertyMetadata(FontStyles.Normal));

        public bool IsHotfix
        {
            get { return (bool)GetValue(IsHotfixProperty); }
            set { SetValue(IsHotfixProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsHotfix.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsHotfixProperty =
            DependencyProperty.Register("IsHotfix", typeof(bool), typeof(Patchnote), new PropertyMetadata(false));
        #endregion

        #region PatchDate
        public string PatchDate
        {
            get { return (string)GetValue(PatchDateProperty); }
            set { SetValue(PatchDateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PatchDate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PatchDateProperty =
            DependencyProperty.Register("PatchDate", typeof(string), typeof(Patchnote), new PropertyMetadata("01.01.1989"));
        #endregion

        #region Patchnotes
        public string[] Patchnotes
        {
            get { return (string[])GetValue(PatchnotesProperty); }
            set
            {
                SetValue(PatchnotesProperty, value);
                rtb_patchnotes.Document = BuildPatchnote(value);
            }
        }

        // Using a DependencyProperty as the backing store for Patchnotes.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PatchnotesProperty =
            DependencyProperty.Register("Patchnotes", typeof(string[]), typeof(Patchnote), new PropertyMetadata(null));
        #endregion
    }
}

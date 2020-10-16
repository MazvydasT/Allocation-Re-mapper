using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AllocationRerouter
{
    public class Book
    {
        public string Title { get; set; }
        public Chapter[] Chapters { get; set; }
    }

    public class Chapter
    {
        public string Title { get; set; } = null;

        public string Paragraph { get; set; } = null;

        public Bitmap Image { get; set; } = null;

        private BitmapImage wpfImage = null;
        public BitmapImage WPFImage
        {
            get
            {
                if (wpfImage == null && Image != null)
                {
                    using (MemoryStream memory = new MemoryStream())
                    {
                        Image.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                        memory.Position = 0;
                        wpfImage = new BitmapImage();
                        wpfImage.BeginInit();
                        wpfImage.StreamSource = memory;
                        wpfImage.CacheOption = BitmapCacheOption.OnLoad;
                        wpfImage.EndInit();
                    }
                }

                return wpfImage;
            }
        }

        public int ImageHeight => Image == null ? 0 : Image.Height;
        public int ImageWidth => Image == null ? 0 : Image.Width;

        public Visibility TitleVisibility => Title == null ? Visibility.Collapsed : Visibility.Visible;
        public Visibility ParagraphVisibility => Paragraph == null ? Visibility.Collapsed : Visibility.Visible;
        public Visibility WPFImageVisibility => WPFImage == null ? Visibility.Collapsed : Visibility.Visible;
    }

    public static class HelpData
    {
        public static Book[] Books { get; } = new[]
        {
            new Book()
            {
                Title="Operations",
                Chapters = new[]
                {
                    new Chapter()
                    {
                        Title = "Operations",
                        Paragraph = "Operations that have CAD allocations also hold not so obvious Flow objects, which this software uses to figure out which objects are allocated to which operations."
                    },

                    new Chapter()
                    {
                        Paragraph = "Easiest way to get Flow objects is by exporting operations tree as XML.",
                        Image = HelpImages.ExportOperationsTree
                    },

                    new Chapter()
                    {
                        Paragraph = "Multiple operations trees can be exported one after another, in parallel using multiple sessions or in one go by multiselecting them.",
                        Image = HelpImages.ExportMultipleOperationsTree
                    },

                    new Chapter()
                    {
                        Paragraph = "To keep business disruption to minimum and checked out object count as low as possible, one can use TableView to only check-out and export operations that contain Flow objects. We rely on assumption that Flow objects are contained within operations also containing Source objects.",
                        Image = HelpImages.ExportOperationsFromTableView
                    },

                    new Chapter()
                    {
                        Paragraph = "TableView configuration:",
                        Image = HelpImages.TableViewConfiguration
                    },

                    new Chapter()
                    {
                        Paragraph = "TableView filter:",
                        Image = HelpImages.TableViewFilter
                    }
                }
            },

            new Book()
            {
                Title = "Old EBOMs",

                Chapters = new[]
                {
                    new Chapter()
                    {
                        Title = "Old EBOMs",
                        Paragraph = "Old EBOM data is used by this software to work out DS number of an OLD_LEVEL allocated assembly/part. Combination of a DS number, assembly/part number and absolute XYZ position makes up a unique identifier used to locate matching assembly/part within new EBOM data."
                    },

                    new Chapter()
                    {
                        Paragraph = "Old EBOM data can be obtained from one or more previous revisions using Version Manager. Data allocated to operations can in fact span multiple revisions.",
                        Image = HelpImages.VersionManager
                    },

                    new Chapter()
                    {
                        Paragraph = "Same as operations, EBOM data can be exported one object at a time, multiple objects (including across multiple revisions) in parallel using multiple sessions or by multiselection in one go.",
                        Image = HelpImages.ExportingOldEBOM
                    }
                }
            },

            new Book()
            {
                Title = "New EBOMs",

                Chapters = new[]
                {
                    new Chapter()
                    {
                        Title = "New EBOMs",
                        Paragraph = "New EBOM data is used by this software to lookup assembly/part external ids to replace OLD_LEVEL assemblies/parts with. Note, OLD_LEVEL marking is not required for mechanism to work - if allocated assembly/part external id is found among old EBOM data and is not found within new EBOM data - it is considered to be OLD_LEVEL and is attempted to be replaced with a match from a new EBOM data set."
                    },

                    new Chapter()
                    {
                        Paragraph = "New EBOM data may be exported in exactly same ways as old EBOM data, with exception that generally only latest data version gets exported, in other words - Version Manager generally does not get used."
                    }
                }
            },

            new Book()
            {
                Title = "Matching mechanics",

                Chapters = new[]
                {
                    "<PmFlow> XML elements get read from operation extracts.",
                    "Allocated assembly/part external ids get extracted from <parts> element of each <PmFlow> element.",
                    "Allocated assembly/part external ids get matched against external ids found in old EBOM data set.",
                    "Identifier gets created for each matched assembly/part consisiting of DS number, aeembly/part number and absolute location.",
                    "Allocated assembly/part identifiers get matched against identifiers generated for each assmebly/part in new EBOM data set.",
                    "Where single identifier match is found and assembly/part external ids are different between matching assemblies/parts, <PmFlow> element with updated <parts> external id gets written into output XML document.",
                    "Where single identifier match is found but assembly/part external ids are same, no action is taken.",
                    "Where multiple identifiers are matched no action is taken.",
                    "Where no identifiers are matched no action is taken."
                }.Select((v,i) => new Chapter() { Paragraph = $"{i + 1}. {v}" }).Prepend(new Chapter() { Title = "Matching mechanics" }).ToArray()
            },

            new Book()
            {
                Title = $"v{string.Join(".", Assembly.GetEntryAssembly().GetName().Version.ToString().Split(new[] { '.' }).Take(3))}"
            }
        };
    }
}

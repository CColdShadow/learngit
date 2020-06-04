using System;
using System.Collections.Generic;
using System.Text;
using log4net;

namespace Apowersoft.Utils.Imaging {
    public enum OutputFormat {
        jpg, png, gif, bmp, tiff, pdf
    }

    public class OutputSettings {
        private static ILog log = LogManager.GetLogger(typeof(OutputSettings));

        private bool reduceColors;
        private bool disableReduceColors;
        private List<IEffect> effects = new List<IEffect>();

        public OutputSettings() {
            disableReduceColors = false;
            Format = OutputFormat.png;
            JPGQuality = 100;
            ReduceColors = false;
        }

        public OutputSettings(OutputFormat format)
            : this() {
            Format = format;
        }

        public OutputSettings(OutputFormat format, int quality)
            : this(format) {
            JPGQuality = quality;
        }

        public OutputSettings(OutputFormat format, int quality, bool reduceColors)
            : this(format, quality) {
            ReduceColors = reduceColors;
        }

        public OutputFormat Format { get; set; }
        public int JPGQuality { get; set; }
        public bool SaveBackgroundOnly { get; set; }

        public List<IEffect> Effects {
            get { return effects; }
        }

        public bool ReduceColors {
            get {
                if (OutputFormat.gif.Equals(Format)) {
                    return true;
                }
                return reduceColors;
            }
            set { reduceColors = value; }
        }

        public bool DisableReduceColors {
            get { return disableReduceColors; }
            set {
                // Quantizing os needed when output format is gif as this has only 256 colors!
                if (!OutputFormat.gif.Equals(Format)) {
                    disableReduceColors = value;
                }
            }
        }

        public static OutputFormat GetFormatFromName(string strFormat) {
            OutputFormat format = OutputFormat.png;
            try {
                format = (OutputFormat)Enum.Parse(typeof(OutputFormat), strFormat.ToLower());
            }
            catch (ArgumentException e) {
                log.ErrorFormat("Failed to get output format AAAAfrom str, {0}, {1}.", strFormat, e.Message);
            }
            return format;
        }

        public static OutputFormat GetFormatFromFileName(string fullPath) {
            OutputFormat format = OutputFormat.png;
            try {
                string extension = fullPath.Substring(fullPath.LastIndexOf(".") + 1);
                if (extension != null) {
                    format = (OutputFormat)Enum.Parse(typeof(OutputFormat), extension.ToLower());
                }
            }
            catch (ArgumentException ae) {
                log.ErrorFormat("Failed to get output format from full path, {0}, {1}.", fullPath, ae.Message);
            }
            return format;
        }

        public static string[] GetFileFormatNames(bool upperCase = false) {
            OutputFormat[] supportedOutputFormats = Enum.GetValues(typeof(OutputFormat)) as OutputFormat[];
            List<string> formatList = new List<string>();
            foreach (OutputFormat fmt in supportedOutputFormats) {
                formatList.Add(upperCase ? fmt.ToString().ToUpper() : fmt.ToString());
            }
            return formatList.ToArray();
        }

    }
}
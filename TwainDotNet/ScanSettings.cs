﻿using System;
using System.Collections.Generic;
using System.Text;

namespace TwainDotNet
{
    public class ScanSettings
    {
        public ScanSettings()
        {
            TransferCount = 1;
        }

        /// <summary>
        /// Indicates if the TWAIN/driver user interface should be used to pick the scan settings.
        /// </summary>
        public bool ShowTwainUI { get; set; }

        /// <summary>
        /// Indicates if the automatic document feeder (ADF) should be the source of the document(s) to scan.
        /// </summary>
        public bool UseDocumentFeeder { get; set; }

        /// <summary>
        /// The number of pages to transfer.
        /// </summary>
        public short TransferCount { get; set; }

        /// <summary>
        /// The resolution settings. Set null to use current defaults.
        /// </summary>
        public ResolutionSettings Resolution { get; set; }

        /// <summary>
        /// The value to set to scan all available pages.
        /// </summary>
        public const short TransferAllPages = -1;

        /// <summary>
        /// Whether to use duplexing, if supported.
        /// </summary>
        public bool UseDuplex { get; set; }

        /// <summary>
        /// Default scan settings.
        /// </summary>
        public static readonly ScanSettings Default = new ScanSettings()
        {
            Resolution = ResolutionSettings.ColourPhotocopier
        };
    }
}

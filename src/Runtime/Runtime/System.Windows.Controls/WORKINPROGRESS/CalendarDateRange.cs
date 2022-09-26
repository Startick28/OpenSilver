﻿

/*===================================================================================
* 
*   Copyright (c) Userware/OpenSilver.net
*      
*   This file is part of the OpenSilver Runtime (https://opensilver.net), which is
*   licensed under the MIT license: https://opensource.org/licenses/MIT
*   
*   As stated in the MIT license, "the above copyright notice and this permission
*   notice shall be included in all copies or substantial portions of the Software."
*  
\*====================================================================================*/

using System;

#if MIGRATION
namespace System.Windows.Controls
#else
namespace Windows.UI.Xaml.Controls
#endif
{
    /// <summary>
    /// Represents a range of dates in a <see cref="T:System.Windows.Controls.Calendar" />.
    /// </summary>
    [OpenSilver.NotImplemented]
    public sealed class CalendarDateRange
    {
        /// <summary>Gets the first date in the represented range.</summary>
        /// <returns>The first date in the represented range.</returns>
        public DateTime Start { get; private set; }

        /// <summary>
        /// Gets the last date in the represented range.
        /// </summary>
        /// <returns>
        /// The last date in the represented range.
        /// </returns>
        public DateTime End { get; private set; }

        /// <summary>
        /// Gets a description of the represented range.
        /// </summary>
        /// <returns>
        /// The description of the represented range.
        /// </returns>
        public string Description { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Windows.Controls.CalendarDateRange" /> class with a single date.
        /// </summary>
        /// <param name="day">
        /// The date to be represented by the range.
        /// </param>
        public CalendarDateRange(DateTime day)
        {
            this.Start = day;
            this.End = day;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Windows.Controls.CalendarDateRange" />
        /// class with a range of dates.
        /// </summary>
        /// <param name="start">
        /// The start of the range to be represented.
        /// </param>
        /// <param name="end">
        /// The end of the range to be represented.
        /// </param>
        public CalendarDateRange(DateTime start, DateTime end)
            : this(start, end, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Windows.Controls.CalendarDateRange" />
        /// class with the specified start and end dates and description.
        /// </summary>
        /// <param name="start">
        /// The start of the range to be represented.
        /// </param>
        /// <param name="end">
        /// The end of the range to be represented.
        /// </param>
        /// <param name="description">
        /// A description of the data range.
        /// </param>
        public CalendarDateRange(DateTime start, DateTime end, string description)
        {
            if (DateTime.Compare(end, start) >= 0)
            {
                this.Start = start;
                this.End = end;
            }
            else
            {
                this.Start = start;
                this.End = start;
            }
            this.Description = description;
        }

        internal bool ContainsAny(CalendarDateRange range)
        {
            int num = DateTime.Compare(Start, range.Start);
            if (num > 0 || DateTime.Compare(End, range.Start) < 0)
            {
                if (num >= 0)
                {
                    return DateTime.Compare(Start, range.End) <= 0;
                }
                return false;
            }
            return true;
        }
    }
}

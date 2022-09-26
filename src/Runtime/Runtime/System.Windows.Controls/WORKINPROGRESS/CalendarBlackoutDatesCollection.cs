

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
using System.Collections.ObjectModel;
using System.Linq;
using System.Resources;
using System.Threading;

#if MIGRATION
namespace System.Windows.Controls
#else
namespace Windows.UI.Xaml.Controls
#endif
{
    /// <summary>
    /// Represents a collection of non-selectable dates in a <see cref="T:System.Windows.Controls.Calendar" />.
    /// </summary>
    [OpenSilver.NotImplemented]
    public sealed class CalendarBlackoutDatesCollection : ObservableCollection<CalendarDateRange>
    {

        private Calendar _owner;

        private Thread _dispatcherThread;

        /// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.CalendarBlackoutDatesCollection" /> class. </summary>
        /// <param name="owner">The <see cref="T:System.Windows.Controls.Calendar" /> whose dates this object represents.</param>
        public CalendarBlackoutDatesCollection(Calendar owner)
        {
            _owner = owner;
            _dispatcherThread = Thread.CurrentThread;
        }

        /// <summary>
        /// Adds all dates before <see cref="P:System.DateTime.Today" /> to the collection.
        /// </summary>
        public void AddDatesInPast() => Add(new CalendarDateRange(DateTime.MinValue, DateTime.Today.AddDays(-1)));

        /// <summary>Returns a value that represents whether this collection contains the specified date.</summary>
        /// <returns>true if the collection contains the specified date; otherwise, false.</returns>
        /// <param name="date">The date to search for.</param>
        public bool Contains(DateTime date)
        {
            int count = base.Count;
            for (int i = 0; i < count; i++)
            {
                if (DateTimeHelper.InRange(date, base[i]))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Returns a value that represents whether this collection contains the specified range of dates.</summary>
        /// <returns>true if all dates in the range are contained in the collection; otherwise, false.</returns>
        /// <param name="start">The start of the date range.</param>
        /// <param name="end">The end of the date range.</param>
        public bool Contains(DateTime start, DateTime end)
        {
            DateTime value;
            DateTime value2;
            if (DateTime.Compare(end, start) > -1)
            {
                value = DateTimeHelper.DiscardTime(start).Value;
                value2 = DateTimeHelper.DiscardTime(end).Value;
            }
            else
            {
                value = DateTimeHelper.DiscardTime(end).Value;
                value2 = DateTimeHelper.DiscardTime(start).Value;
            }
            int count = base.Count;
            for (int i = 0; i < count; i++)
            {
                CalendarDateRange calendarDateRange = base[i];
                if (DateTime.Compare(calendarDateRange.Start, value) == 0 && DateTime.Compare(calendarDateRange.End, value2) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Returns a value that represents whether this collection contains any date in the specified range.</summary>
        /// <returns>true if any date in the range is contained in the collection; otherwise, false.</returns>
        /// <param name="range">The range of dates to search for.</param>
        public bool ContainsAny(CalendarDateRange range)
        {
            return this.Any((CalendarDateRange r) => r.ContainsAny(range));
        }

        [OpenSilver.NotImplemented]
        protected override void ClearItems()
        {
            EnsureValidThread();
            base.ClearItems();
            _owner.UpdateMonths();
        }

        [OpenSilver.NotImplemented]
        protected override void InsertItem(int index, CalendarDateRange item)
        {
            EnsureValidThread();
            if (!IsValid(item))
            {
                throw new ArgumentOutOfRangeException(this.ToString() + " : UnSelectableDates - " + item.ToString());
            }
            base.InsertItem(index, item);
            _owner.UpdateMonths();
        }

        [OpenSilver.NotImplemented]
        protected override void RemoveItem(int index)
        {
            EnsureValidThread();
            base.RemoveItem(index);
            _owner.UpdateMonths();
        }

        [OpenSilver.NotImplemented]
        protected override void SetItem(int index, CalendarDateRange item)
        {
            EnsureValidThread();
            if (!IsValid(item))
            {
                throw new ArgumentOutOfRangeException(this.ToString() + " : UnSelectableDates - " + item.ToString());
            }
            base.SetItem(index, item);
            _owner.UpdateMonths();
        }

        [OpenSilver.NotImplemented]
        private bool IsValid(CalendarDateRange item)
        {
            foreach (DateTime selectedDate in _owner.SelectedDates)
            {
                if (DateTimeHelper.InRange(selectedDate, item))
                {
                    return false;
                }
            }
            return true;
        }

        private void EnsureValidThread()
        {
            if (Thread.CurrentThread != _dispatcherThread)
            {
                throw new NotSupportedException(this.ToString() + " : CalendarCollection_MultiThreadedCollectionChangeNotSupported in the current thread - " + Thread.CurrentThread.ToString());
            }
        }
    }
}

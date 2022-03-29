﻿// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

#if MIGRATION
namespace System.Windows.Controls
#else
namespace Windows.UI.Xaml.Controls
#endif
{
    internal class DataGridColumnCollection : ObservableCollection<DataGridColumn>
    {
        #region Data

        private DataGrid _owningGrid;

        #endregion Data

        public DataGridColumnCollection(DataGrid owningGrid)
        {
            this._owningGrid = owningGrid;
            this.ItemsInternal = new List<DataGridColumn>();
            this.FillerColumn = new DataGridFillerColumn(owningGrid);
            this.RowGroupSpacerColumn = new DataGridFillerColumn(owningGrid);
            this.DisplayIndexMap = new List<int>();
        }

        #region Properties

        internal int AutogeneratedColumnCount
        {
            get;
            set;
        }

        internal List<int> DisplayIndexMap
        {
            get;
            set;
        }

        internal DataGridFillerColumn FillerColumn
        {
            get;
            private set;
        }

        internal DataGridColumn FirstColumn
        {
            get
            {
                return GetFirstColumn(null /*isVisible*/, null /*isFrozen*/, null /*isReadOnly*/);
            }
        }

        internal DataGridColumn FirstVisibleColumn
        {
            get
            {
                return GetFirstColumn(true /*isVisible*/, null /*isFrozen*/, null /*isReadOnly*/);
            }
        }

        internal DataGridColumn FirstVisibleNonFillerColumn
        {
            get
            {
                DataGridColumn dataGridColumn = this.FirstVisibleColumn;
                if (dataGridColumn == this.RowGroupSpacerColumn)
                {
                    dataGridColumn = GetNextVisibleColumn(dataGridColumn);
                }
                return dataGridColumn;
            }
        }

        internal DataGridColumn FirstVisibleWritableColumn
        {
            get
            {
                return GetFirstColumn(true /*isVisible*/, null /*isFrozen*/, false /*isReadOnly*/);
            }
        }

        internal DataGridColumn FirstVisibleScrollingColumn
        {
            get
            {
                return GetFirstColumn(true /*isVisible*/, false /*isFrozen*/, null /*isReadOnly*/);
            }
        }

        internal List<DataGridColumn> ItemsInternal
        {
            get;
            private set;
        }

        internal DataGridColumn LastVisibleColumn
        {
            get
            {
                return GetLastColumn(true /*isVisible*/, null /*isFrozen*/, null /*isReadOnly*/);
            }
        }

        internal DataGridColumn LastVisibleScrollingColumn
        {
            get
            {
                return GetLastColumn(true /*isVisible*/, false /*isFrozen*/, null /*isReadOnly*/);
            }
        }

        internal DataGridColumn LastVisibleWritableColumn
        {
            get
            {
                return GetLastColumn(true /*isVisible*/, null /*isFrozen*/, false /*isReadOnly*/);
            }
        }

        internal DataGridFillerColumn RowGroupSpacerColumn
        {
            get;
            private set;
        }

        internal int VisibleColumnCount
        {
            get
            {
                int visibleColumnCount = 0;
                for (int columnIndex = 0; columnIndex < this.ItemsInternal.Count; columnIndex++)
                {
                    if (this.ItemsInternal[columnIndex].IsVisible)
                    {
                        visibleColumnCount++;
                    }
                }
                return visibleColumnCount;
            }
        }

        internal double VisibleEdgedColumnsWidth
        {
            get;
            private set;
        }

        /// <summary>
        /// The number of star columns that are currently visible.
        /// NOTE: Requires that EnsureVisibleEdgedColumnsWidth has been called.
        /// </summary>
        internal int VisibleStarColumnCount
        {
            get;
            private set;
        }

        #endregion Properties

        #region Protected Methods

        protected override void ClearItems()
        {
            Debug.Assert(this._owningGrid != null);
            try
            {
                this._owningGrid.NoCurrentCellChangeCount++;
                if (this.ItemsInternal.Count > 0)
                {
                    if (this._owningGrid.InDisplayIndexAdjustments)
                    {
                        // We are within columns display indexes adjustments. We do not allow changing the column collection while adjusting display indexes.
                        throw DataGridError.DataGrid.CannotChangeColumnCollectionWhileAdjustingDisplayIndexes();
                    }

                    this._owningGrid.OnClearingColumns();
                    for (int columnIndex = 0; columnIndex < this.ItemsInternal.Count; columnIndex++)
                    {
                        // Detach the column...
                        this.ItemsInternal[columnIndex].OwningGrid = null;
                    }
                    this.ItemsInternal.Clear();
                    this.DisplayIndexMap.Clear();
                    this.AutogeneratedColumnCount = 0;
                    this._owningGrid.OnColumnCollectionChanged_PreNotification(false /*columnsGrew*/);
                    base.ClearItems();
                    this.VisibleEdgedColumnsWidth = 0;
                    this._owningGrid.OnColumnCollectionChanged_PostNotification(false /*columnsGrew*/);
                }
            }
            finally
            {
                this._owningGrid.NoCurrentCellChangeCount--;
            }
        }

        protected override void InsertItem(int columnIndex, DataGridColumn dataGridColumn)
        {
            Debug.Assert(this._owningGrid != null);
            try
            {
                this._owningGrid.NoCurrentCellChangeCount++;
                if (this._owningGrid.InDisplayIndexAdjustments)
                {
                    // We are within columns display indexes adjustments. We do not allow changing the column collection while adjusting display indexes.
                    throw DataGridError.DataGrid.CannotChangeColumnCollectionWhileAdjustingDisplayIndexes();
                }
                if (dataGridColumn == null)
                {
                    throw new ArgumentNullException("dataGridColumn");
                }

                int columnIndexWithFiller = columnIndex;
                if (dataGridColumn != this.RowGroupSpacerColumn && this.RowGroupSpacerColumn.IsRepresented)
                {
                    columnIndexWithFiller++;
                }

                // get the new current cell coordinates
                DataGridCellCoordinates newCurrentCellCoordinates = this._owningGrid.OnInsertingColumn(columnIndex, dataGridColumn);

                // insert the column into our internal list
                this.ItemsInternal.Insert(columnIndexWithFiller, dataGridColumn);
                dataGridColumn.Index = columnIndexWithFiller;
                dataGridColumn.OwningGrid = this._owningGrid;
                dataGridColumn.RemoveEditingElement();
                if (dataGridColumn.IsVisible)
                {
                    this.VisibleEdgedColumnsWidth += dataGridColumn.ActualWidth;
                }

                // continue with the base insert
                this._owningGrid.OnInsertedColumn_PreNotification(dataGridColumn);
                this._owningGrid.OnColumnCollectionChanged_PreNotification(true /*columnsGrew*/);

                if (dataGridColumn != this.RowGroupSpacerColumn)
                {
                    base.InsertItem(columnIndex, dataGridColumn);
                }
                this._owningGrid.OnInsertedColumn_PostNotification(newCurrentCellCoordinates, dataGridColumn.DisplayIndex);
                this._owningGrid.OnColumnCollectionChanged_PostNotification(true /*columnsGrew*/);
            }
            finally
            {
                this._owningGrid.NoCurrentCellChangeCount--;
            }
        }

        protected override void RemoveItem(int columnIndex)
        {
            RemoveItemPrivate(columnIndex, false /*isSpacer*/);
        }

        protected override void SetItem(int columnIndex, DataGridColumn dataGridColumn)
        {
            throw new NotSupportedException();
        }

        #endregion Protected Methods

        #region Internal Methods

        internal bool DisplayInOrder(int columnIndex1, int columnIndex2)
        {
            int displayIndex1 = ((DataGridColumn)this.ItemsInternal[columnIndex1]).DisplayIndexWithFiller;
            int displayIndex2 = ((DataGridColumn)this.ItemsInternal[columnIndex2]).DisplayIndexWithFiller;
            return displayIndex1 < displayIndex2;
        }

        internal bool EnsureRowGrouping(bool rowGrouping)
        {
            // The insert below could cause the first column to be added.  That causes a refresh 
            // which re-enters this method so instead of checking RowGroupSpacerColumn.IsRepresented, 
            // we need to check to see if it's actually in our collection instead.
            bool spacerRepresented = (this.ItemsInternal.Count > 0) && (this.ItemsInternal[0] == this.RowGroupSpacerColumn);
            if (rowGrouping && !spacerRepresented)
            {
                this.Insert(0, this.RowGroupSpacerColumn);
                this.RowGroupSpacerColumn.IsRepresented = true;
                return true;
            }
            else if (!rowGrouping && spacerRepresented)
            {
                Debug.Assert(this.ItemsInternal[0] == this.RowGroupSpacerColumn);
                // We need to set IsRepresented to false before removing the RowGroupSpacerColumn
                // otherwise, we'll remove the column after it
                this.RowGroupSpacerColumn.IsRepresented = false;
                RemoveItemPrivate(0, true /*isSpacer*/);
                Debug.Assert(this.DisplayIndexMap.Count == this.ItemsInternal.Count);
                return true;
            }
            return false;
        }

        /// <summary>
        /// In addition to ensuring that column widths are valid, this method updates the values of
        /// VisibleEdgedColumnsWidth and VisibleStarColumnCount.
        /// </summary>
        internal void EnsureVisibleEdgedColumnsWidth()
        {
            this.VisibleStarColumnCount = 0;
            this.VisibleEdgedColumnsWidth = 0;
            for (int columnIndex = 0; columnIndex < this.ItemsInternal.Count; columnIndex++)
            {
                if (this.ItemsInternal[columnIndex].IsVisible)
                {
                    this.ItemsInternal[columnIndex].EnsureWidth();
                    if (this.ItemsInternal[columnIndex].Width.IsStar)
                    {
                        this.VisibleStarColumnCount++;
                    }
                    this.VisibleEdgedColumnsWidth += this.ItemsInternal[columnIndex].ActualWidth;
                }
            }
        }

        internal DataGridColumn GetColumnAtDisplayIndex(int displayIndex)
        {
            if (displayIndex < 0 || displayIndex >= this.ItemsInternal.Count || displayIndex >= this.DisplayIndexMap.Count)
            {
                return null;
            }
            int columnIndex = this.DisplayIndexMap[displayIndex];
            return this.ItemsInternal[columnIndex];
        }

        internal int GetColumnCount(bool isVisible, bool isFrozen, int fromColumnIndex, int toColumnIndex)
        {
            Debug.Assert(DisplayInOrder(fromColumnIndex, toColumnIndex));
            Debug.Assert((this.ItemsInternal[toColumnIndex].IsVisible) == isVisible);
            Debug.Assert(this.ItemsInternal[toColumnIndex].IsFrozen == isFrozen);

            int columnCount = 0;
            DataGridColumn dataGridColumn = this.ItemsInternal[fromColumnIndex];

            while (dataGridColumn != this.ItemsInternal[toColumnIndex])
            {
                dataGridColumn = GetNextColumn(dataGridColumn, isVisible, isFrozen, null /*isReadOnly*/);
                Debug.Assert(dataGridColumn != null);
                columnCount++;
            }
            return columnCount;
        }

        internal IEnumerable<DataGridColumn> GetDisplayedColumns()
        {
            Debug.Assert(this.ItemsInternal.Count == this.DisplayIndexMap.Count);
            foreach (int columnIndex in this.DisplayIndexMap)
            {
                yield return this.ItemsInternal[columnIndex];
            }
        }

        /// <summary>
        /// Returns an enumeration of all columns that meet the criteria of the filter predicate.
        /// </summary>
        /// <param name="filter">Criteria for inclusion.</param>
        /// <returns>Columns that meet the criteria, in ascending DisplayIndex order.</returns>
        internal IEnumerable<DataGridColumn> GetDisplayedColumns(Predicate<DataGridColumn> filter)
        {
            Debug.Assert(filter != null);
            Debug.Assert(this.ItemsInternal.Count == this.DisplayIndexMap.Count);
            foreach (int columnIndex in this.DisplayIndexMap)
            {
                DataGridColumn column = this.ItemsInternal[columnIndex];
                if (filter(column))
                {
                    yield return column;
                }
            }
        }

        /// <summary>
        /// Returns an enumeration of all columns that meet the criteria of the filter predicate.
        /// The columns are returned in the order specified by the reverse flag.
        /// </summary>
        /// <param name="reverse">Whether or not to return the columns in descending DisplayIndex order.</param>
        /// <param name="filter">Criteria for inclusion.</param>
        /// <returns>Columns that meet the criteria, in the order specified by the reverse flag.</returns>
        internal IEnumerable<DataGridColumn> GetDisplayedColumns(bool reverse, Predicate<DataGridColumn> filter)
        {
            return reverse ? GetDisplayedColumnsReverse(filter) : GetDisplayedColumns(filter);
        }

        /// <summary>
        /// Returns an enumeration of all columns that meet the criteria of the filter predicate.
        /// The columns are returned in descending DisplayIndex order.
        /// </summary>
        /// <param name="filter">Criteria for inclusion.</param>
        /// <returns>Columns that meet the criteria, in descending DisplayIndex order.</returns>
        internal IEnumerable<DataGridColumn> GetDisplayedColumnsReverse(Predicate<DataGridColumn> filter)
        {
            Debug.Assert(filter != null);
            Debug.Assert(this.ItemsInternal.Count == this.DisplayIndexMap.Count);
            for (int displayIndex = this.DisplayIndexMap.Count - 1; displayIndex >= 0; displayIndex--)
            {
                DataGridColumn column = this.ItemsInternal[this.DisplayIndexMap[displayIndex]];
                if (filter(column))
                {
                    yield return column;
                }
            }
        }

        internal DataGridColumn GetFirstColumn(bool? isVisible, bool? isFrozen, bool? isReadOnly)
        {
            Debug.Assert(this.ItemsInternal.Count == this.DisplayIndexMap.Count);
            int index = 0;
            while (index < this.DisplayIndexMap.Count)
            {
                DataGridColumn dataGridColumn = GetColumnAtDisplayIndex(index);
                if ((isVisible == null || (dataGridColumn.IsVisible) == isVisible) &&
                    (isFrozen == null || dataGridColumn.IsFrozen == isFrozen) &&
                    (isReadOnly == null || dataGridColumn.IsReadOnly == isReadOnly))
                {
                    return dataGridColumn;
                }
                index++;
            }
            return null;
        }

        internal DataGridColumn GetLastColumn(bool? isVisible, bool? isFrozen, bool? isReadOnly)
        {
            Debug.Assert(this.ItemsInternal.Count == this.DisplayIndexMap.Count);
            int index = this.DisplayIndexMap.Count - 1;
            while (index >= 0)
            {
                DataGridColumn dataGridColumn = GetColumnAtDisplayIndex(index);
                if ((isVisible == null || (dataGridColumn.IsVisible) == isVisible) &&
                    (isFrozen == null || dataGridColumn.IsFrozen == isFrozen) &&
                    (isReadOnly == null || dataGridColumn.IsReadOnly == isReadOnly))
                {
                    return dataGridColumn;
                }
                index--;
            }
            return null;
        }

        internal DataGridColumn GetNextColumn(DataGridColumn dataGridColumnStart)
        {
            return GetNextColumn(dataGridColumnStart, null /*isVisible*/, null /*isFrozen*/, null /*isReadOnly*/);
        }

        internal DataGridColumn GetNextColumn(DataGridColumn dataGridColumnStart,
                                                  bool? isVisible, bool? isFrozen, bool? isReadOnly)
        {
            Debug.Assert(dataGridColumnStart != null);
            Debug.Assert(this.ItemsInternal.Contains(dataGridColumnStart));
            Debug.Assert(this.ItemsInternal.Count == this.DisplayIndexMap.Count);

            int index = dataGridColumnStart.DisplayIndexWithFiller + 1;
            while (index < this.DisplayIndexMap.Count)
            {
                DataGridColumn dataGridColumn = GetColumnAtDisplayIndex(index);

                if ((isVisible == null || (dataGridColumn.IsVisible) == isVisible) &&
                    (isFrozen == null || dataGridColumn.IsFrozen == isFrozen) &&
                    (isReadOnly == null || dataGridColumn.IsReadOnly == isReadOnly))
                {
                    return dataGridColumn;
                }
                index++;
            }
            return null;
        }

        internal DataGridColumn GetNextVisibleColumn(DataGridColumn dataGridColumnStart)
        {
            return GetNextColumn(dataGridColumnStart, true /*isVisible*/, null /*isFrozen*/, null /*isReadOnly*/);
        }

        internal DataGridColumn GetNextVisibleFrozenColumn(DataGridColumn dataGridColumnStart)
        {
            return GetNextColumn(dataGridColumnStart, true /*isVisible*/, true /*isFrozen*/, null /*isReadOnly*/);
        }

        internal DataGridColumn GetNextVisibleWritableColumn(DataGridColumn dataGridColumnStart)
        {
            return GetNextColumn(dataGridColumnStart, true /*isVisible*/, null /*isFrozen*/, false /*isReadOnly*/);
        }

        internal DataGridColumn GetPreviousColumn(DataGridColumn dataGridColumnStart,
                                                      bool? isVisible, bool? isFrozen, bool? isReadOnly)
        {
            Debug.Assert(dataGridColumnStart != null);
            Debug.Assert(this.ItemsInternal.Contains(dataGridColumnStart));
            Debug.Assert(this.ItemsInternal.Count == this.DisplayIndexMap.Count);

            int index = dataGridColumnStart.DisplayIndexWithFiller - 1;
            while (index >= 0)
            {
                DataGridColumn dataGridColumn = GetColumnAtDisplayIndex(index);
                if ((isVisible == null || (dataGridColumn.IsVisible) == isVisible) &&
                    (isFrozen == null || dataGridColumn.IsFrozen == isFrozen) &&
                    (isReadOnly == null || dataGridColumn.IsReadOnly == isReadOnly))
                {
                    return dataGridColumn;
                }
                index--;
            }
            return null;
        }

        internal DataGridColumn GetPreviousVisibleNonFillerColumn(DataGridColumn dataGridColumnStart)
        {
            DataGridColumn column = GetPreviousColumn(dataGridColumnStart, true /*isVisible*/, null /*isFrozen*/, null /*isReadOnly*/);
            return (column is DataGridFillerColumn) ? null : column;
        }

        internal DataGridColumn GetPreviousVisibleScrollingColumn(DataGridColumn dataGridColumnStart)
        {
            return GetPreviousColumn(dataGridColumnStart, true /*isVisible*/, false /*isFrozen*/, null /*isReadOnly*/);
        }

        internal DataGridColumn GetPreviousVisibleWritableColumn(DataGridColumn dataGridColumnStart)
        {
            return GetPreviousColumn(dataGridColumnStart, true /*isVisible*/, null /*isFrozen*/, false /*isReadOnly*/);
        }

        internal int GetVisibleColumnCount(int fromColumnIndex, int toColumnIndex)
        {
            Debug.Assert(DisplayInOrder(fromColumnIndex, toColumnIndex));
            Debug.Assert(this.ItemsInternal[toColumnIndex].IsVisible);

            int columnCount = 0;
            DataGridColumn dataGridColumn = this.ItemsInternal[fromColumnIndex];

            while (dataGridColumn != this.ItemsInternal[toColumnIndex])
            {
                dataGridColumn = GetNextVisibleColumn(dataGridColumn);
                Debug.Assert(dataGridColumn != null);
                columnCount++;
            }
            return columnCount;
        }

        internal IEnumerable<DataGridColumn> GetVisibleColumns()
        {
            Predicate<DataGridColumn> filter = column => column.IsVisible;
            return GetDisplayedColumns(filter);
        }

        internal IEnumerable<DataGridColumn> GetVisibleFrozenColumns()
        {
            Predicate<DataGridColumn> filter = column => column.IsVisible && column.IsFrozen;
            return GetDisplayedColumns(filter);
        }

        internal double GetVisibleFrozenEdgedColumnsWidth()
        {
            double visibleFrozenColumnsWidth = 0;
            for (int columnIndex = 0; columnIndex < this.ItemsInternal.Count; columnIndex++)
            {
                if (this.ItemsInternal[columnIndex].IsVisible && this.ItemsInternal[columnIndex].IsFrozen)
                {
                    visibleFrozenColumnsWidth += this.ItemsInternal[columnIndex].ActualWidth;
                }
            }
            return visibleFrozenColumnsWidth;
        }

        internal IEnumerable<DataGridColumn> GetVisibleScrollingColumns()
        {
            Predicate<DataGridColumn> filter = column => column.IsVisible && !column.IsFrozen;
            return GetDisplayedColumns(filter);
        }

        #endregion Internal Methods

        #region Private Methods

        private void RemoveItemPrivate(int columnIndex, bool isSpacer)
        {
            Debug.Assert(this._owningGrid != null);
            try
            {
                this._owningGrid.NoCurrentCellChangeCount++;

                if (this._owningGrid.InDisplayIndexAdjustments)
                {
                    // We are within columns display indexes adjustments. We do not allow changing the column collection while adjusting display indexes.
                    throw DataGridError.DataGrid.CannotChangeColumnCollectionWhileAdjustingDisplayIndexes();
                }

                int columnIndexWithFiller = columnIndex;
                if (!isSpacer && this.RowGroupSpacerColumn.IsRepresented)
                {
                    columnIndexWithFiller++;
                }

                Debug.Assert(columnIndexWithFiller >= 0 && columnIndexWithFiller < this.ItemsInternal.Count);

                DataGridColumn dataGridColumn = this.ItemsInternal[columnIndexWithFiller];
                DataGridCellCoordinates newCurrentCellCoordinates = this._owningGrid.OnRemovingColumn(dataGridColumn);
                this.ItemsInternal.RemoveAt(columnIndexWithFiller);
                if (dataGridColumn.IsVisible)
                {
                    this.VisibleEdgedColumnsWidth -= dataGridColumn.ActualWidth;
                }
                dataGridColumn.OwningGrid = null;
                dataGridColumn.RemoveEditingElement();

                // continue with the base remove
                this._owningGrid.OnRemovedColumn_PreNotification(dataGridColumn);
                this._owningGrid.OnColumnCollectionChanged_PreNotification(false /*columnsGrew*/);
                if (!isSpacer)
                {
                    base.RemoveItem(columnIndex);
                }
                this._owningGrid.OnRemovedColumn_PostNotification(newCurrentCellCoordinates);
                this._owningGrid.OnColumnCollectionChanged_PostNotification(false /*columnsGrew*/);
            }
            finally
            {
                this._owningGrid.NoCurrentCellChangeCount--;
            }
        }

        #endregion Private Methods

        #region Debugging Methods

#if DEBUG && MIGRATION
        internal bool Debug_VerifyColumnDisplayIndexes()
        {
            for (int columnDisplayIndex = 0; columnDisplayIndex < this.ItemsInternal.Count; columnDisplayIndex++)
            {
                if (GetColumnAtDisplayIndex(columnDisplayIndex) == null)
                {
                    return false;
                }
            }
            return true;
        }

        internal void Debug_PrintColumns()
        {
            foreach (DataGridColumn column in this.ItemsInternal)
            {
                Debug.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0} {1} {2}", column.Header, column.Index, column.DisplayIndex));
            }
        }
#endif

        #endregion Debugging Methods
    }
}

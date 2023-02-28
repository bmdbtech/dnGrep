﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace dnGREP.WPF.MVHelpers
{
    public class FileNameOnlyComparer : IComparer<FormattedGrepResult>
    {
        private readonly ListSortDirection direction;
        public FileNameOnlyComparer(ListSortDirection direction)
        {
            this.direction = direction;
        }

        public int Compare(FormattedGrepResult? x, FormattedGrepResult? y)
        {
            var left = direction == ListSortDirection.Ascending ? x : y;
            var right = direction == ListSortDirection.Ascending ? y : x;

            var leftSort = Path.GetFileName(left?.GrepResult.FileNameDisplayed);
            var rightSort = Path.GetFileName(right?.GrepResult.FileNameDisplayed);

            return string.Compare(leftSort, rightSort, StringComparison.CurrentCulture);
        }
    }

    public class FileTypeAndNameComparer : IComparer<FormattedGrepResult>
    {
        private readonly ListSortDirection direction;
        public FileTypeAndNameComparer(ListSortDirection direction)
        {
            this.direction = direction;
        }

        public int Compare(FormattedGrepResult? x, FormattedGrepResult? y)
        {
            var left = direction == ListSortDirection.Ascending ? x : y;
            var right = direction == ListSortDirection.Ascending ? y : x;

            var leftType = Path.GetExtension(left?.GrepResult.FileNameDisplayed);
            var rightType = Path.GetExtension(right?.GrepResult.FileNameDisplayed);

            int comp = string.Compare(leftType, rightType, StringComparison.CurrentCulture);
            if (comp != 0)
                return comp;

            var leftSort = Path.GetFileName(left?.GrepResult.FileNameDisplayed);
            var rightSort = Path.GetFileName(right?.GrepResult.FileNameDisplayed);

            return string.Compare(leftSort, rightSort, StringComparison.CurrentCulture);
        }
    }

    public class FileNameDepthFirstComparer : IComparer<FormattedGrepResult>
    {
        private readonly ListSortDirection direction;
        public FileNameDepthFirstComparer(ListSortDirection direction)
        {
            this.direction = direction;
        }

        public int Compare(FormattedGrepResult? x, FormattedGrepResult? y)
        {
            var left = direction == ListSortDirection.Ascending ? x : y;
            var right = direction == ListSortDirection.Ascending ? y : x;

            // add a path separator that ensures the shortest path sorts first
            var leftSort = Path.GetDirectoryName(left?.GrepResult.FileNameReal) + "/!" +
                Path.GetFileName(left?.GrepResult.FileNameReal) +
                (string.IsNullOrEmpty(left?.GrepResult.InnerFileName) ? string.Empty :
                Path.GetDirectoryName(left?.GrepResult.InnerFileName) + "/!" +
                Path.GetFileName(left?.GrepResult.InnerFileName));
            var rightSort = Path.GetDirectoryName(right?.GrepResult.FileNameReal) + "/!" +
                Path.GetFileName(right?.GrepResult.FileNameReal) +
                (string.IsNullOrEmpty(right?.GrepResult.InnerFileName) ? string.Empty :
                Path.GetDirectoryName(right?.GrepResult.InnerFileName) + "/!" +
                Path.GetFileName(right?.GrepResult.InnerFileName));

            return string.Compare(leftSort, rightSort, StringComparison.CurrentCulture);
        }
    }

    public class FileNameBreadthFirstComparer : IComparer<FormattedGrepResult>
    {
        private readonly ListSortDirection direction;
        public FileNameBreadthFirstComparer(ListSortDirection direction)
        {
            this.direction = direction;
        }

        public int Compare(FormattedGrepResult? x, FormattedGrepResult? y)
        {
            var left = direction == ListSortDirection.Ascending ? x : y;
            var right = direction == ListSortDirection.Ascending ? y : x;

            string? leftDir = Path.GetDirectoryName(left?.GrepResult.FileNameReal);
            string? rightDir = Path.GetDirectoryName(right?.GrepResult.FileNameReal);

            int leftDepth = leftDir != null ? GetDepth(new DirectoryInfo(leftDir)) : -1;
            int rightDepth = rightDir != null ? GetDepth(new DirectoryInfo(rightDir)) : -1;

            if (leftDepth != rightDepth)
                return leftDepth.CompareTo(rightDepth);

            int comp = string.Compare(leftDir, rightDir, StringComparison.CurrentCulture);
            if (comp != 0)
                return comp;

            if (left?.GrepResult.FileNameReal != right?.GrepResult.FileNameReal)
                return string.Compare(left?.GrepResult.FileNameReal, right?.GrepResult.FileNameReal, StringComparison.CurrentCulture);

            return string.Compare(left?.GrepResult.InnerFileName, right?.GrepResult.InnerFileName, StringComparison.CurrentCulture);
        }
        private static int GetDepth(DirectoryInfo di)
        {
            int depth = 0;
            var parent = di.Parent;
            while (parent != null)
            {
                depth++;
                parent = parent.Parent;
            }
            return depth;
        }
    }

    public class FileSizeComparer : IComparer<FormattedGrepResult>
    {
        private readonly ListSortDirection direction;
        public FileSizeComparer(ListSortDirection direction)
        {
            this.direction = direction;
        }

        public int Compare(FormattedGrepResult? x, FormattedGrepResult? y)
        {
            var left = direction == ListSortDirection.Ascending ? x : y;
            var right = direction == ListSortDirection.Ascending ? y : x;

            if (left != null && right != null)
            {
                return left.GrepResult.FileInfo.Length.CompareTo(right.GrepResult.FileInfo.Length);
            }
            return 0;
        }
    }

    public class FileDateComparer : IComparer<FormattedGrepResult>
    {
        private readonly ListSortDirection direction;
        public FileDateComparer(ListSortDirection direction)
        {
            this.direction = direction;
        }

        public int Compare(FormattedGrepResult? x, FormattedGrepResult? y)
        {
            var left = direction == ListSortDirection.Ascending ? x : y;
            var right = direction == ListSortDirection.Ascending ? y : x;

            if (left != null && right != null)
            {
                return left.GrepResult.FileInfo.LastWriteTime.CompareTo(right.GrepResult.FileInfo.LastWriteTime);
            }
            return 0;
        }
    }

    public class MatchCountComparer : IComparer<FormattedGrepResult>
    {
        private readonly ListSortDirection direction;
        public MatchCountComparer(ListSortDirection direction)
        {
            this.direction = direction;
        }

        public int Compare(FormattedGrepResult? x, FormattedGrepResult? y)
        {
            var left = direction == ListSortDirection.Ascending ? x : y;
            var right = direction == ListSortDirection.Ascending ? y : x;

            if (left != null && right != null && left.Matches != right.Matches)
                return left.Matches.CompareTo(right.Matches);

            var leftSort = Path.GetFileName(left?.GrepResult.FileNameDisplayed);
            var rightSort = Path.GetFileName(right?.GrepResult.FileNameDisplayed);

            return string.Compare(leftSort, rightSort, StringComparison.CurrentCulture);
        }
    }
}

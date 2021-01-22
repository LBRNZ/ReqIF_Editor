using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace ReqIF_Editor
{
    /// <summary>
    /// Wraps the functionality provided by the <see cref="AnimationHelper"/> class
    /// in a behavior which can be used with the <see cref="ColumnDefinition"/>
    /// and <see cref="RowDefinition"/> types.
    /// </summary>
    public class GridAnimationBehavior : DependencyObject
    {
        #region Attached IsExpanded DependencyProperty

        /// <summary>
        /// Register the "IsExpanded" attached property and the "OnIsExpanded" callback 
        /// </summary>
        public static readonly DependencyProperty IsExpandedProperty =
          DependencyProperty.RegisterAttached("IsExpanded", typeof(bool), typeof(GridAnimationBehavior),
            new FrameworkPropertyMetadata(OnIsExpandedChanged));

        public static void SetIsExpanded(DependencyObject dependencyObject, bool value)
        {
            dependencyObject.SetValue(IsExpandedProperty, value);
        }

        #endregion

        #region Attached Duration DependencyProperty

        /// <summary>
        /// Register the "Duration" attached property 
        /// </summary>
        public static readonly DependencyProperty DurationProperty =
          DependencyProperty.RegisterAttached("Duration", typeof(TimeSpan), typeof(GridAnimationBehavior),
            new FrameworkPropertyMetadata(TimeSpan.FromMilliseconds(300)));

        public static void SetDuration(DependencyObject dependencyObject, TimeSpan value)
        {
            dependencyObject.SetValue(DurationProperty, value);
        }

        private static TimeSpan GetDuration(DependencyObject dependencyObject)
        {
            return (TimeSpan)dependencyObject.GetValue(DurationProperty);
        }

        #endregion

        #region GridCellSize DependencyProperty

        /// <summary>
        /// Use a private "GridCellSize" dependency property as a temporary backing 
        /// store for the last expanded grid cell size (row height or column width).
        /// </summary>
        private static readonly DependencyProperty GridCellSizeProperty =
          DependencyProperty.Register("GridCellSize", typeof(double), typeof(GridAnimationBehavior),
            new UIPropertyMetadata(0.0));

        private static void SetGridCellSize(DependencyObject dependencyObject, double value)
        {
            dependencyObject.SetValue(GridCellSizeProperty, value);
        }

        private static double GetGridCellSize(DependencyObject dependencyObject)
        {
            return (double)dependencyObject.GetValue(GridCellSizeProperty);
        }

        #endregion

        /// <summary>
        /// Called when the attached <c>IsExpanded</c> property changed.
        /// </summary>
        private static void OnIsExpandedChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var duration = GetDuration(dependencyObject);
            var rowDefinition = dependencyObject as RowDefinition;
            if (rowDefinition != null)
            {
                // The IsExpanded attached property of a RowDefinition changed
                if ((bool)e.NewValue)
                {
                    var expandedHeight = GetGridCellSize(rowDefinition);
                    if (expandedHeight > 0)
                    {
                        // Animate row height back to saved expanded height.
                        AnimationHelper.AnimateGridRowExpandCollapse(rowDefinition, true, expandedHeight, rowDefinition.ActualHeight, 0, duration);
                    }
                }
                else
                {
                    // Save expanded height and animate row height down to zero.
                    SetGridCellSize(rowDefinition, rowDefinition.ActualHeight);
                    AnimationHelper.AnimateGridRowExpandCollapse(rowDefinition, false, rowDefinition.ActualHeight, 0, 0, duration);
                }
            }

            var columnDefinition = dependencyObject as ColumnDefinition;
            if (columnDefinition != null)
            {
                // The IsExpanded attached property of a ColumnDefinition changed
                if ((bool)e.NewValue)
                {
                    var expandedWidth = GetGridCellSize(columnDefinition);
                    if (expandedWidth > 0)
                    {
                        // Animate column width back to saved expanded width.
                        AnimationHelper.AnimateGridColumnExpandCollapse(columnDefinition, true, expandedWidth, columnDefinition.ActualWidth, 0, duration);
                    }
                }
                else
                {
                    // Save expanded width and animate column width down to zero.
                    SetGridCellSize(columnDefinition, columnDefinition.ActualWidth);
                    AnimationHelper.AnimateGridColumnExpandCollapse(columnDefinition, false, columnDefinition.ActualWidth, 0, 0, duration);
                }
            }
        }
    }
    class AnimationHelper
    {
        /// <summary>
        /// Animate expand/collapse of a grid column. 
        /// </summary>
        /// <param name="gridColumn">The grid column to expand/collapse.</param>
        /// <param name="expandedWidth">The expanded width.</param>
        /// <param name="milliseconds">The milliseconds component of the duration.</param>
        /// <param name="collapsedWidth">The width when collapsed.</param>
        /// <param name="minWidth">The minimum width of the column.</param>
        /// <param name="seconds">The seconds component of the duration.</param>
        /// <param name="expand">If true, expand, otherwise collapse.</param>
        public static void AnimateGridColumnExpandCollapse(ColumnDefinition gridColumn, bool expand, double expandedWidth, double collapsedWidth,
            double minWidth, TimeSpan duration)
        {
            if (expand && gridColumn.ActualWidth >= expandedWidth)
                // It's as wide as it needs to be.
                return;

            if (!expand && gridColumn.ActualWidth == collapsedWidth)
                // It's already collapsed.
                return;

            Storyboard storyBoard = new Storyboard();

            GridLengthAnimation animation = new GridLengthAnimation();
            animation.From = new GridLength(gridColumn.ActualWidth);
            animation.To = new GridLength(expand ? expandedWidth : collapsedWidth);
            animation.Duration = duration;

            // Set delegate that will fire on completion.
            animation.Completed += delegate
            {
                // Set the animation to null on completion. This allows the grid to be resized manually
                gridColumn.BeginAnimation(ColumnDefinition.WidthProperty, null);

                // Set the final value manually.
                gridColumn.Width = new GridLength(expand ? expandedWidth : collapsedWidth);

                // Set the minimum width.
                gridColumn.MinWidth = minWidth;
            };

            storyBoard.Children.Add(animation);

            Storyboard.SetTarget(animation, gridColumn);
            Storyboard.SetTargetProperty(animation, new PropertyPath(ColumnDefinition.WidthProperty));
            storyBoard.Children.Add(animation);

            // Begin the animation.
            storyBoard.Begin();
        }

        /// <summary>
        /// Animate expand/collapse of a grid row. 
        /// </summary>
        /// <param name="gridRow">The grid row to expand/collapse.</param>
        /// <param name="expandedHeight">The expanded height.</param>
        /// <param name="collapsedHeight">The collapesed height.</param>
        /// <param name="minHeight">The minimum height.</param>
        /// <param name="milliseconds">The milliseconds component of the duration.</param>
        /// <param name="seconds">The seconds component of the duration.</param>
        /// <param name="expand">If true, expand, otherwise collapse.</param>
        public static void AnimateGridRowExpandCollapse(RowDefinition gridRow, bool expand, double expandedHeight, double collapsedHeight, double minHeight, TimeSpan duration)
        {
            if (expand && gridRow.ActualHeight >= expandedHeight)
                // It's as high as it needs to be.
                return;

            if (!expand && gridRow.ActualHeight == collapsedHeight)
                // It's already collapsed.
                return;

            Storyboard storyBoard = new Storyboard();

            GridLengthAnimation animation = new GridLengthAnimation();
            animation.From = new GridLength(gridRow.ActualHeight);
            animation.To = new GridLength(expand ? expandedHeight : collapsedHeight);
            animation.Duration = duration;

            // Set delegate that will fire on completioon.
            animation.Completed += delegate
            {
                // Set the animation to null on completion. This allows the grid to be resized manually
                gridRow.BeginAnimation(RowDefinition.HeightProperty, null);

                // Set the final height.
                gridRow.Height = new GridLength(expand ? expandedHeight : collapsedHeight);

                // Set the minimum height.
                gridRow.MinHeight = minHeight;
            };

            storyBoard.Children.Add(animation);

            Storyboard.SetTarget(animation, gridRow);
            Storyboard.SetTargetProperty(animation, new PropertyPath(RowDefinition.HeightProperty));
            storyBoard.Children.Add(animation);

            // Begin the animation.
            storyBoard.Begin();
        }
    }
    internal class GridLengthAnimation : AnimationTimeline
    {
        static GridLengthAnimation()
        {
            FromProperty = DependencyProperty.Register("From", typeof(GridLength),
                typeof(GridLengthAnimation));

            ToProperty = DependencyProperty.Register("To", typeof(GridLength),
                typeof(GridLengthAnimation));
        }

        public override Type TargetPropertyType
        {
            get
            {
                return typeof(GridLength);
            }
        }

        protected override System.Windows.Freezable CreateInstanceCore()
        {
            return new GridLengthAnimation();
        }

        public static readonly DependencyProperty FromProperty;
        public GridLength From
        {
            get
            {
                return (GridLength)GetValue(GridLengthAnimation.FromProperty);
            }
            set
            {
                SetValue(GridLengthAnimation.FromProperty, value);
            }
        }

        public static readonly DependencyProperty ToProperty;
        public GridLength To
        {
            get
            {
                return (GridLength)GetValue(GridLengthAnimation.ToProperty);
            }
            set
            {
                SetValue(GridLengthAnimation.ToProperty, value);
            }
        }

        public override object GetCurrentValue(object defaultOriginValue,
            object defaultDestinationValue, AnimationClock animationClock)
        {
            double fromVal = ((GridLength)GetValue(GridLengthAnimation.FromProperty)).Value;
            double toVal = ((GridLength)GetValue(GridLengthAnimation.ToProperty)).Value;

            if (fromVal > toVal)
            {
                return new GridLength((1 - animationClock.CurrentProgress.Value) * (fromVal - toVal) + toVal, GridUnitType.Star);
            }
            else
                return new GridLength(animationClock.CurrentProgress.Value * (toVal - fromVal) + fromVal, GridUnitType.Star);
        }
    }
}

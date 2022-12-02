using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace common
{
    public static class MatrisExtensions
    {
        public static (int r, int c) Up(this (int r, int c) cell) => (cell.r - 1, cell.c);
        public static (int r, int c) Down(this (int r, int c) cell) => (cell.r + 1, cell.c);
        public static (int r, int c) Left(this (int r, int c) cell) => (cell.r, cell.c - 1);
        public static (int r, int c) Right(this (int r, int c) cell) => (cell.r, cell.c + 1);
        public static bool Equals(this (int r, int c) cell, (int r, int c) other)
          => cell.r == other.r && cell.c == other.c;

        public static IEnumerable<(int r, int c)> GetAdjacentStraight(this (int r, int c) c)
        {
            return new[]
            {
                c.Right(),
                c.Down(),
                c.Up(),
                c.Left()
            };
        }
        public static IEnumerable<(int r, int c)> GetAdjacentAll(this (int r, int c) c)
        {
            return new[]
            {
                c.Right(),
                c.Down().Right(),
                c.Down(),
                c.Down().Left(),
                c.Left(),
                c.Up().Right(),
                c.Up(),
                c.Up().Left(),
            };
        }
        public static IEnumerable<(int r, int c)> GetAdjacentAll(this (int r, int c) c, IMatris matrix = null)
        {
            var arr = c.GetAdjacentAll();
            if (matrix != null)
                return arr.Where(matrix.CellInside).ToList();
            return arr;
        }
        public static IEnumerable<(int r, int c)> GetAdjacentStraight(this (int r, int c) c, IMatris matrix = null)
        {
            var arr = c.GetAdjacentStraight();
            if (matrix != null)
                return arr.Where(matrix.CellInside).ToList();
            return arr;
        }

    }

    public class Matris<T> : IMatris
    {
        private readonly List<List<T>> _data;
        public readonly Func<(int r, int c), T> InitFunc;
        public  Func<(int r, int c), T> OutsideFunc;
        public int TrackLowR { get; private set; }
        public int TrackLowC { get; private set; }

        public Matris(int rows, int cols, Func<(int r, int c), T> initFunc)
        {
            TrackLowR = rows;
            TrackLowC = cols;
            Columns = cols;
            InitFunc = initFunc;
            OutsideFunc = initFunc;
            _data = new List<List<T>>(rows);
            for (var r = 0; r < rows; r++)
                _data.Add(new List<T>(Columns));
        }

        public Matris(Matris<T> m, Func<(int r, int c), T> initFunc)
        : this(m.Rows, m.Columns, initFunc)
        {
        }

        public int Rows => _data.Count;
        public int Columns { get; private set; }

        public IEnumerable<((int, int) Cell, T Value)> Cells
        {
            get
            {
                var r = 0;
                foreach (var row in _data)
                {
                    var c = 0;
                    foreach (var value in row)
                    {
                        yield return ((r, c), value);
                        ++c;
                    }
                    ++r;
                }
            }
        }
        public IEnumerable<((int, int) Cell, T Value)> AllCells
        {
            get
            {
                for (int r = 0; r < Rows; r++)
                {
                    for (int c = 0; c < Columns; c++)
                    {
                        yield return ((r, c), Value((r, c)));
                    }
                }
            }
        }

        public void Update((int r, int c) cell, Func<T, T> func)
        {
            Set(cell, func(Value(cell)));
        }

        public void Set((int r, int c) cell, T value)
        {
            if (cell.r < 0 || cell.c < 0 && value.Equals( OutsideFunc(cell)))
                return;
            var (r, c) = cell;
            while (_data.Count < r + 1)
                _data.Add(new List<T>(Columns));
            var list = _data[r];
            while (list.Count < c + 1)
            {
                list.Add(InitFunc((r, list.Count)));
            }

            if (c + 1 > Columns)
                Columns = c + 1;

            if (!(value?.Equals(list[c]) ?? list[c]?.Equals(value) ?? true))
            {
                if (r < TrackLowR) TrackLowR = r;
                if (c < TrackLowC) TrackLowC = c;
            }
            list[c] = value;

        }

        public T Value((int r, int c) cell)
        {
            if (!CellInside(cell))
                return OutsideFunc(cell);

            if (cell.r < 0 || cell.r >= _data.Count)
                return InitFunc(cell);

            var column = _data[cell.r];
            if (cell.c < 0 || cell.c >= column.Count)
                return InitFunc(cell);

            return column[cell.c];
        }
        public void Resize(int toR, int toC)
        {
            if (toR < _data.Count) _data.RemoveRange(toR, _data.Count - toR);

            if (toC < Columns)
                _data.ForEach(col =>
                {
                    if (toC < col.Count)
                    {
                        col.RemoveRange(toC, col.Count - toC);
                    }
                });

            Columns = toC;
        }

        public void PrependColumns(int number, T outsideValue)
        {
            _data.ForEach((col, r) =>
            {
                var insData = new List<T>();
                for (int c = 0; c < number; c++)
                {
                    insData.Add(outsideValue);
                }
                col.InsertRange(0, insData);
            });
            TrackLowC += number;
            Columns += number;
        }
        public void PrependRows(int number, T outsideValue)
        {
            for (int r = 0; r < number; r++)
            {
                var item = new List<T>();
                for (int c = 0; c < Columns; c++)
                {
                    item.Add(outsideValue);
                }
                _data.Insert(0, item);
                TrackLowR++;
            }
        }

        public string ToString(string format = "{0}")
        {
            var sb = new StringBuilder();
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    var value = Value((r, c));
                    sb.Append(string.Format(format, value));
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public string ToString(Func<(int r, int c), T, string> formatterFunc,int extra=0)
        {
            var sb = new StringBuilder();
            for (int r = 0-extra; r < Rows+extra; r++)
            {
                for (int c = 0-extra; c < Columns+extra; c++)
                {
                    sb.Append(formatterFunc((r, c), Value((r, c))));
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public void EachCell(Action<(int, int), T> action)
        {
            foreach (var c in Cells) action(c.Cell, c.Value);
        }


        public bool CellInside((int r, int c) cell)
        {
            return cell.r >= 0 &&
                   cell.r < Rows &&
                   cell.c >= 0 &&
                   cell.c < Columns;
        }

        public void Fold((char direction, int number) fold, params T[] background)
        {
            if (fold.direction == 'r')
            {
                var fr = fold.number;
                for (var r = 1; r < fr + 1; r++)
                    for (var c = 0; c < Columns; c++)
                        if (!Value((fr + r, c)).In(background))
                            Set((fr - r, c), Value((fr + r, c)));

                Resize(fr, Columns);
            }
            else
            {
                var fc = fold.number;
                for (var c = 1; c < fc + 1; c++)
                    for (var r = 0; r < Rows; r++)
                        if (!Value((r, fc + c)).In(background))
                            Set((r, fc - c), Value((r, fc + c)));

                Resize(Rows, fc);
            }
        }

    }

    public interface IMatris
    {
        bool CellInside((int r, int c) arg);
    }
}
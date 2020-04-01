using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Montage.Weiss.Tools.Utilities
{
    public static class StringExtensions
    {
        public delegate ReadOnlySpan<char> SpanEnumerator();
        public delegate int ReadOnlySpanCursor(ReadOnlySpan<char> original);
        public delegate T SpanFunction<T>(ReadOnlySpan<char> original);
        public delegate bool SpanOptionalFunction<T>(ReadOnlySpan<char> original, out T outvar);
        public delegate R FuncOut<I,T,R>(I original, out T outvar);

        public static SpanCursor AsSpanCursor(this string parent, string separator = "\n")
        {
            return new SpanCursor(separator, () => parent.AsSpan());
        }

        public static ReadOnlySpan<char> Slice(this ReadOnlySpan<char> original, ReadOnlySpanCursor start, ReadOnlySpanCursor end)
        {
            var startIndex = start(original);
            return original.Slice(startIndex, end(original) - startIndex);
        }

        public static ReadOnlySpan<char> Slice(this ReadOnlySpan<char> original, ReadOnlySpanCursor start)
        {
            return original.Slice(start(original));
        }

        public static T? AsParsed<T>(this ReadOnlySpan<char> original, SpanOptionalFunction<T> spanFunction) where T : struct
        {
            if (spanFunction(original, out T outVar))
                return outVar;
            else
                return null;
        }

        public static T? AsParsed<T>(this string original, FuncOut<string,T,bool> stringFunction) where T : struct
        {
            if (stringFunction(original, out T outVar))
                return outVar;
            else
                return null;
        }

        public class SpanCursor
        {
            //string _parent;
            private string _separator;
            private int _cursor = -1;
            private int _lineNumber = 1;

            SpanEnumerator _parent;
            public int LineNumber => _lineNumber;


            public SpanCursor(string separator, SpanEnumerator parent)
            {
                this._separator = separator;
                this._parent = parent;
            }

            public ReadOnlySpan<char> ReadLine()
            {
                var length = _parent().Slice(_cursor + 1).IndexOf(_separator) + 1; //, _cursor + 1) - _cursor;
                Log.Debug("Length: {length}", length);
                if (length > -1)
                    return _parent().Slice(_cursor + 1, length).Trim();
                else
                    return _parent().Slice(_cursor + 1).Trim();
            }

            public ReadOnlySpan<char> ReadAll()
            {
                return _parent().Slice(_cursor + 1).Trim();
            }

            public void MoveUp()
            {
                if (_cursor - 1 >= 0)
                {
                    _cursor = _parent().Slice(0, _cursor - 1).LastIndexOf(_separator);
                    _lineNumber++;
                }
                Log.Debug("Cursor: {_cursor}", _cursor);
            }


            public void Next()
            {
                //                _cursor = _parent.IndexOf(_separator, _cursor + 1);
                _cursor = _cursor + _parent().Slice(_cursor + 1).IndexOf(_separator) + 1;
                if (_cursor > 0)
                    _lineNumber++;
                else
                    _lineNumber = 1;
                //_cursor = _cursor + _parent().Slice(_cursor + 1).IndexOf(_separator);
                Log.Debug("Next Cursor: {_cursor}", _cursor);
            }

            public void Previous(int lines = 1)
            {
                for (int i = 0; i < lines; i++) MoveUp();
            }

            public void Next(int lines)
            {
                for (int i = 0; i < lines; i++) Next();

            }

        }

    }
}

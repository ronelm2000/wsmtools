﻿using AngleSharp;
using AngleSharp.Dom;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Montage.Card.API.Utilities;

public static class StringExtensions
{
    public delegate ReadOnlySpan<char> SpanEnumerator();
    public delegate int ReadOnlySpanCursor(ReadOnlySpan<char> original);
    public delegate T SpanFunction<T>(ReadOnlySpan<char> original);
    public delegate bool SpanOptionalFunction<T>(ReadOnlySpan<char> original, out T outvar);
    public delegate R FuncOut<I,T,R>(I original, out T outvar);

    public static string EscapeQuotes(this string str)
    {
        return str  .Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\b", "\\b")
                    .Replace("\n", "\\n")
                    .Replace("\t", "\\t")
                    .Replace("\r", "\\r");                
    }

    public static string Replace(this string str, Regex regex, string replacementString)
        => regex.Replace(str, replacementString);
    public static string Replace(this string str, Regex regex, string replacementString, int count)
        => regex.Replace(str, replacementString, count);
    public static string Replace(this string str, Regex regex, MatchEvaluator matchEvaluator)
        => regex.Replace(str, matchEvaluator);

    public static string ReplaceAll(this string str, params (string searchString, string replaceString)[] allReplacements)
    {
        return allReplacements.Aggregate(str, (str, p) => str.Replace(p.searchString, p.replaceString));
    }

    public static bool StartsWithAny(this string str, IEnumerable<string> values)
    {
        return values.Any(v => str.StartsWith(v));
    }

    public static IEnumerable<Match> SplitWithRegex(this string str, string regex)
    {
        return (new Regex(regex)).Matches(str);
    }

    public static IEnumerable<Match> SplitWithRegex(this string str, Regex regex)
    {
        return regex.Matches(str);
    }

    public static string AsFileNameFriendly(this string str, char replacement = '_')
    {
        var res = str;
        foreach (var c in System.IO.Path.GetInvalidFileNameChars())
            res = res.Replace(c, replacement);
        res = res.Replace('\\', replacement);
        res = res.Replace('/', replacement);
        res = res.Replace(' ', replacement);
        res = res.Replace('!', replacement);
        return res;
    }

    public static string AsFriendlyToTabletopSimulator(this string str, char replacement = '_')
    {
        var res = str;
        foreach (var c in System.IO.Path.GetInvalidFileNameChars())
            res = res.Replace(c, replacement);
        res = res   .Replace('\\', replacement)
                    .Replace('/', replacement)
                    .Replace(' ', replacement)
                    .Replace(')', replacement)
                    .Replace('(', replacement);
        return res;
    }

    public static string Limit(this string str, int characterLimit, string continuationPhrase = " [..]")
    {
        var limit = int.Min(str.Length, characterLimit);
        if (limit == str.Length) return str;
        else return str[0..(limit - continuationPhrase.Length)] + continuationPhrase;
    }

    public static async Task<IDocument> ParseHTML(this string content)
    {
        var config = AngleSharp.Configuration.Default;
        var context = AngleSharp.BrowsingContext.New(config);
        //Create a virtual request to specify the document to load (here from our fixed string)
        return await context.OpenAsync(req => req.Content(content));
    }

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
        return original.Slice(Math.Min(original.Length, start(original)));
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
        private bool _isLastLine = false;

        SpanEnumerator _parent;
        public int LineNumber => _lineNumber;

        public SpanCursor(string separator, SpanEnumerator parent)
        {
            this._separator = separator;
            this._parent = parent;
        }

        public ReadOnlySpan<char> CurrentLine
        {
            get
            {
                var length = _parent().Slice(_cursor + 1).IndexOf(_separator) + 1; //, _cursor + 1) - _cursor;
                Log.Debug("Length: {length}", length);
                if (length > -1)
                    return _parent().Slice(_cursor + 1, length).TrimEnd("\r\n");
                else
                    return _parent().Slice(_cursor + 1).TrimEnd("\r\n");
            }
        }

        /// <summary>
        /// The current line and all succeeding lines until the end of the parent Span (or string).
        /// </summary>
        public ReadOnlySpan<char> LinesUntilEOS
        {
            get
            {
                return _parent().Slice(_cursor + 1).Trim();
            }
        }

        public void MoveUp()
        {
            if (_cursor - 1 >= 0 && !_isLastLine)
            {
                _cursor = _parent().Slice(0, _cursor).LastIndexOf(_separator);
                _lineNumber--;
            }
            Log.Debug("Cursor: {_cursor}", _cursor);
        }

        public bool Next()
        {
            var previousCursor = _cursor;
            var diff = _parent().Slice(_cursor + 1).IndexOf(_separator);
            _cursor = _cursor + diff + 1;
            if (diff >= 0)
                _lineNumber++;
            else
            {
                _cursor = previousCursor;
                _isLastLine = true;
            //    _lineNumber = 1;
            }
            Log.Debug("Next Cursor: {_cursor}", _cursor);
            return previousCursor < _cursor;
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

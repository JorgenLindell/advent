using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace common;

public class DebuggerTextWriter : TextWriter
{
    private bool _isOpen;

    public DebuggerTextWriter()
    {
        Encoding = new UnicodeEncoding();
        this._isOpen = true;
    }

    protected override void Dispose(bool disposing)
    {
        _isOpen = false;
        base.Dispose(disposing);
    }

    public override void Write(char value)
    {
        if (!_isOpen)
        {
            throw new ObjectDisposedException(null);
        }
        Debug.Write(value.ToString());
    }

    public override void Write(string? value)
    {
        if (!_isOpen)
        {
            throw new ObjectDisposedException(null);
        }
        if (value != null!)
        {
            Debug.Write(value);
        }
    }

    public override Encoding Encoding { get; }

    public override void Write(char[] buffer, int index, int count)
    {
        if (!_isOpen)
        {
            throw new ObjectDisposedException(null);
        }
        if (buffer == null! || index < 0 || count < 0 || buffer.Length - index < count)
        {
            base.Write(buffer!, index, count); // delegate throw exception to base class
        }
        Debug.Write(new string(buffer!, index, count));

    }
}
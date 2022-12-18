using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using common;

namespace _7;

public class Node
{
    public Node(string name, Node parent)
    {
        Name = name;
        Size = 0;
        Parent = parent;
    }
    public Node(string name, long size, Node parent)
    {
        Name = name;
        Size = size;
        Parent = parent;
    }

    public Node Parent { get; set; } = null;
    public string Name { get; set; }
    public long Size { get; set; }
    public Dictionary<string, Node> Children { get; set; } = new();

    public bool IsFile => Size != 0;

    public string FullPath
    {
        get
        {
            if (Parent == null)
                return "";
            return Parent.FullPath + FileSystem.PathSeparator + Name;
        }
    }

    public void ParseContent(IEnumerable<string> fileList)
    {
        foreach (var row in fileList)
        {
            var parts = row.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts[0] == "dir")
            {
                Children.Add(parts[1], new Node(parts[1], this));
            }
            else
            {
                Children.Add(parts[1], new Node(parts[1], parts[0].ToLong()!.Value,this));
            }
        }
    }
    public Node Root => Parent == null ? this : Parent.Root;

    public IEnumerable<Node> AllFiles()
    {
        foreach (var child in Children.Values)
        {
            if (child.Size > 0)
                yield return child;
            else
            {
                foreach (var allFile in child.AllFiles())
                {
                    yield return allFile;
                }
            }
        }
    }
    public IEnumerable<Node> AllDirectories()
    {
        foreach (var child in Children.Values)
        {
            if (child.Size <= 0)
            {
                yield return child;
                foreach (var dir in child.AllDirectories())
                {
                    yield return dir;
                }
            }
        }
    }
    public long CalculatedSize
    {
        get => Size > 0 
            ? Size 
            : Children.Values.Sum(x => x.CalculatedSize);
    }
}

public class FileSystem
{
    public const string PathSeparator = "/";
    private Node CurrentDirectory { get; set; }

    public FileSystem()
    {
        CurrentDirectory = new Node(PathSeparator,null);
        Root = CurrentDirectory;
    }

    public Node Root { get; set; }

    public void SetCurrent(Node node)
    {
        CurrentDirectory = node;
    }

    public void GoUp()
    {
        CurrentDirectory = CurrentDirectory.Parent;
    }

    public void GoDown(string name)
    {
        if (!CurrentDirectory.Children.ContainsKey(name))
        {
            CurrentDirectory.Children.Add(name, new Node(name,CurrentDirectory));
        }
        CurrentDirectory = CurrentDirectory.Children[name];
    }

    public void Parse(string command, List<string> output)
    {
        string GetParamTrim(string st,int len)
        {
            
            return st.PadRight(len).Remove(0, len).Trim();
        }

        var cmd = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (cmd[1] == "cd")
        {
            ParseCd(GetParamTrim(command,5), output);
        }
        else if (cmd[1] == "ls")
        {
            ParseLs(GetParamTrim(command, 5), output);
        }
    }

    private void ParseLs(string path, List<string> output)
    {
        CurrentDirectory.ParseContent(output);
    }

    private void ParseCd(string path, List<string> output)
    {
        if (path == "..")
            GoUp();
        else if (path == PathSeparator)
            CurrentDirectory = Root;
        else GoDown(path);
    }

}
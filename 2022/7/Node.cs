using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using common;

public class Node
{
    public Node(string name)
    {
        Name = name;
    }
    public Node(string name, long size)
    {
        Name = name;
        Size = size;
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
                Children.Add(parts[1], new Node(parts[1], 0));
            }
            else
            {
                Children.Add(parts[1], new Node(parts[1], parts[0].ToLong()!.Value));
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
        CurrentDirectory = new Node(PathSeparator);
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
            CurrentDirectory.Children.Add(name, new Node(name));
        }
        CurrentDirectory = CurrentDirectory.Children[name];
    }

    public void Parse(string command, List<string> output)
    {
        var cmd = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (cmd[1] == "cd")
        {
            ParseCd(cmd[2], output);
        }
        else if (cmd[1] == "ls")
        {
            ParseLs(cmd[2], output);
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
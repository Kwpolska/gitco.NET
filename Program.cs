using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace gitco.NET;

public static class Program
{
  private static readonly string gitBranchWithRemote = "--no-pager branch --all --list --color=never";
  private static readonly string gitBranchWithoutRemote = "--no-pager branch --list --color=never";
  private static readonly string remotePrefix = "remote/";
  private static readonly int remotePrefixLength = remotePrefix.Length;

  public static List<Branch> ExtractBranchListFromGitOutput(string gitOutput)
    => gitOutput
      .TrimEnd()
      .Split('\n')
      .Select(branchLine =>
      {
        var isCurrent = branchLine.StartsWith('*');
        var branch = branchLine[2..];
        var isRemote = false;

        if (branch.StartsWith(remotePrefix))
        {
          isRemote = true;
          branch = branch[remotePrefixLength..];
        }

        return new Branch(branch, isRemote, isCurrent);
      })
      .OrderBy(b => b.Name + (b.IsRemote ? "1" : "0"))
      .DistinctBy(b => b.Name)
      .ToList();

  public static void PrintHeader(string? filter)
  {
    if (filter != null)
    {
      var fLines = new StringBuilder().Append('-', filter.Length).ToString();
      Console.WriteLine($"\x1b[36;1mChoose a Branch (Filter: {filter})\x1b[0m");
      Console.WriteLine($"\x1b[36;1m-------------------------{fLines}-\x1b[0m\n");
    }
    else
    {
      Console.WriteLine("\x1b[36;1mChoose a Branch\x1b[0m");
      Console.WriteLine("\x1b[36;1m---------------\x1b[0m\n");
    }
  }

  public static string FormatBranches(List<Branch> branches, string? filter)
  {
    var branchWidth = (branches.Count + 1).ToString(CultureInfo.InvariantCulture).Length;
    var formatString = $"\x1b[1m{{0,{branchWidth}}}.\x1b[0m {{1}}";

    var branchLines = branches.Select(
        (branch, index) =>
        {
          var branchDisplay = branch.Name;
          if (branch.IsCurrent) branchDisplay = $"\x1b[32;1m{branchDisplay}\x1b[0m";
          if (branch.IsRemote) branchDisplay = $"{branchDisplay} \x1b[35m(R)\x1b[0m";
          var branchLine = string.Format(formatString, index + 1, branchDisplay);
          return new BranchDisplay(branchLine, branch.Name);
        }).Where(branchDisplay => filter == null || branchDisplay.BranchName.Contains(filter))
      .Select(branchDisplay => branchDisplay.BranchLine);

    return string.Join('\n', branchLines);
  }

  public static void PrintBranches(List<Branch> branches, string? filter)
    => Console.WriteLine(FormatBranches(branches, filter));

  public static List<Branch> BuildBranchList(bool includeRemote)
  {
    var gitArgs = includeRemote ? gitBranchWithRemote : gitBranchWithoutRemote;
    var gitOutput = RunGitWithOutput(gitArgs);
    return ExtractBranchListFromGitOutput(gitOutput);
  }

  public static void Main(string[] args)
  {
    var includeRemote = false;
    string? filter = null;
    var branches = BuildBranchList(includeRemote);

    while (true)
    {
      PrintHeader(filter);
      PrintBranches(branches, filter);

      Console.WriteLine(
        "\n\x1b[36mnumber → select    M → master    R → show remote branches    /QUERY → filter\x1b[0m");
      Console.Write("\x1b[36;1m>\x1b[0m ");

      var query = Console.ReadLine();
      if (query == null)
      {
        Console.WriteLine();
        Environment.ExitCode = 2;
        return;
      }

      query = query.Trim();

      if (query == "M")
      {
        GitCheckout("master");
        return;
      }

      if (query.ToLower().StartsWith("q"))
      {
        return;
      }

      if (query == "R")
      {
        includeRemote = !includeRemote;
        filter = null;
        branches = BuildBranchList(includeRemote);
      }
      else if (query.StartsWith('/'))
      {
        filter = query[1..];
        if (filter == string.Empty) filter = null;
      }
      else if (int.TryParse(query, out var number) && number > 0)
      {
        var branchName = branches[number - 1].Name;
        GitCheckout(branchName);
        return;
      }
      else
      {
        Console.WriteLine("\x1b[31;1mError:\x1b[0m no number specified!\n\n");
      }
    }
  }

  private static string RunGitWithOutput(string args)
  {
    var proc = new Process
    {
      StartInfo = new ProcessStartInfo
      {
        FileName = "git",
        Arguments = args,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        CreateNoWindow = true
      }
    };
    proc.Start();
    return proc.StandardOutput.ReadToEnd();
  }

  private static void GitCheckout(string branchName)
  {
    var proc = new Process
    {
      StartInfo = new ProcessStartInfo
      {
        FileName = "git",
        Arguments = $"checkout {branchName}",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true
      }
    };
    proc.Start();
    proc.WaitForExit();
    Console.Write(proc.StandardOutput.ReadToEnd());
    Console.Write(proc.StandardError.ReadToEnd());
    Environment.ExitCode = proc.ExitCode;
  }
}

public record Branch(string Name, bool IsRemote = false, bool IsCurrent = false);

public record BranchDisplay(string BranchLine, string BranchName);
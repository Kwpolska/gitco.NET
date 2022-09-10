using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace gitco.NET;

public static class Program
{
  private static readonly string gitBranchWithRemote = "--no-pager branch --all --list --color=never";
  private static readonly string gitBranchWithoutRemote = "--no-pager branch --list --color=never";
  private static readonly string remotePrefix = "remotes/";

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
          branch = string.Join('/', branch.Split(" ").First().Split("/").Skip(2));
        }

        return new Branch(branch, isRemote, isCurrent);
      })
      .OrderBy(b => b.Name + (b.IsRemote ? "1" : "0"))
      .DistinctBy(b => b.Name)
      .ToList();

  public static void PrintHeader(string? filter)
  {
    Console.ForegroundColor = ConsoleColor.Cyan;
    if (filter != null)
    {
      var fLines = new StringBuilder().Append('-', filter.Length).ToString();
      Console.WriteLine($"Choose a Branch (Filter: {filter})");
      Console.WriteLine($"-------------------------{fLines}-\n");
    }
    else
    {
      Console.WriteLine("Choose a Branch");
      Console.WriteLine("---------------\n");
    }
    Console.ResetColor();
  }

  public static IEnumerable<BranchDisplay> FormatBranches(List<Branch> branches, string? filter)
  {
    var branchWidth = (branches.Count + 1).ToString(CultureInfo.InvariantCulture).Length;
    var numberFormatString = $"{{0,{branchWidth}}}. ";

    return branches.Select(
        (branch, index) =>
          new BranchDisplay(
            Number: string.Format(numberFormatString, index + 1),
            BranchName: branch.Name,
            IsRemote: branch.IsRemote,
            IsCurrent: branch.IsCurrent
          )
        ).Where(branchDisplay => filter == null || branchDisplay.BranchName.Contains(filter));
  }

  public static void PrintBranches(List<Branch> branches, string? filter)
  {
    foreach (var branchDisplay in FormatBranches(branches, filter))
    {
      Console.ForegroundColor = ConsoleColor.White;
      Console.Write(branchDisplay.Number);
      Console.ResetColor();
      if (branchDisplay.IsCurrent) Console.ForegroundColor = ConsoleColor.Green;
      Console.Write(branchDisplay.BranchName);
      if (branchDisplay.IsRemote)
      {
        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        Console.Write(" (R)");
      }
      Console.ResetColor();
      Console.WriteLine();
    }
  } 

  public static List<Branch> BuildBranchList(bool includeRemote)
  {
    var gitArgs = includeRemote ? gitBranchWithRemote : gitBranchWithoutRemote;
    var gitOutput = RunGitWithOutput(gitArgs);
    return ExtractBranchListFromGitOutput(gitOutput);
  }

  private static List<Branch> BuildBranchListWithExit(bool includeRemote)
  {
    try
    {
      return BuildBranchList(includeRemote);
    }
    catch (GitException e)
    {
      Console.WriteLine(e.Message);
      Environment.Exit(e.ExitCode);
      return new(); // unreachable
    }
  }

  public static void Main(string[] args)
  {
    var includeRemote = false;
    string? filter = null;
    var branches = BuildBranchListWithExit(includeRemote);

    while (true)
    {
      PrintHeader(filter);
      PrintBranches(branches, filter);
      
      Console.ForegroundColor = ConsoleColor.DarkCyan;
      Console.WriteLine(
        "\nnumber → select    M → master    R → show remote branches    /QUERY → filter");
      Console.ForegroundColor = ConsoleColor.Cyan;
      Console.Write("> ");
      Console.ResetColor();

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
        branches = BuildBranchListWithExit(includeRemote);
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
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("Error:");
        Console.ResetColor();
        Console.WriteLine("no number specified!\n\n");
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
        RedirectStandardError = true,
        CreateNoWindow = true
      }
    };
    proc.Start();
    var output = proc.StandardOutput.ReadToEnd();
    proc.WaitForExit();
    if (proc.ExitCode != 0)
    {
      var errorOutput = proc.StandardError.ReadToEnd();
      throw new GitException((output + errorOutput).Trim(), proc.ExitCode);
    }
    return output;
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
    var output = proc.StandardOutput.ReadToEnd() + proc.StandardError.ReadToEnd();
    Console.Write(output.Trim());
    Environment.ExitCode = proc.ExitCode;
  }
}

public record Branch(string Name, bool IsRemote = false, bool IsCurrent = false);

public record BranchDisplay(string Number, string BranchName, bool IsRemote, bool IsCurrent);

public class GitException : Exception
{
  public int ExitCode { get; set; }
  public GitException(string message, int exitCode) : base(message)
  {
    ExitCode = exitCode;
  }
}

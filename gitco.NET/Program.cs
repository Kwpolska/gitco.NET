/*
 * gitco.NET
 * Copyright © 2014-2022, Chris Warrick.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are
 * met:
 *
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions, and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions, and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 *
 * 3. Neither the name of the author of this software nor the names of
 *    contributors to this software may be used to endorse or promote
 *    products derived from this software without specific prior written
 *    consent.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT
 * OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

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
      .ReplaceLineEndings("\n")
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
      .OrderBy(b => b.Name)
      .ThenBy(b => b.IsRemote)
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

  public static IEnumerable<BranchDisplay> FilterAndNumberBranches(List<Branch> branches, string? filter)
  {
    var branchWidth = branches.Count.ToString(CultureInfo.InvariantCulture).Length;
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
    foreach (var branchDisplay in FilterAndNumberBranches(branches, filter))
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
    Console.WriteLine(output.Trim());
    Environment.ExitCode = proc.ExitCode;
  }
}

public record Branch(string Name, bool IsRemote = false, bool IsCurrent = false);

public record BranchDisplay(string Number, string BranchName, bool IsRemote = false, bool IsCurrent = false);

public class GitException : Exception
{
  public int ExitCode { get; set; }
  public GitException(string message, int exitCode) : base(message)
  {
    ExitCode = exitCode;
  }
}

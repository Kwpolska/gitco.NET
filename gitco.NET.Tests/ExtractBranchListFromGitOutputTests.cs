/*
 * gitco.NET
 * Copyright © 2014-2022, Chris Warrick. All rights reserved.
 * Licensed under the 3-clause BSD license. See /LICENSE for details.
 */

namespace gitco.NET.Tests;

public class ExtractBranchListFromGitOutputTests
{
  [Fact]
  public void SingleBranch_NotCurrent()
  {
    var gitOutput = "  foo";
    var expectedOutput = new List<Branch> { new Branch("foo") };
    var actualOutput = Program.ExtractBranchListFromGitOutput(gitOutput);

    Assert.Equal(expectedOutput, actualOutput);
  }

  [Fact]
  public void SingleBranch_Current()
  {
    var gitOutput = "* foo";
    var expectedOutput = new List<Branch> { new Branch("foo", IsCurrent: true) };
    var actualOutput = Program.ExtractBranchListFromGitOutput(gitOutput);

    Assert.Equal(expectedOutput, actualOutput);
  }

  [Fact]
  public void TwoBranches_NoneCurrent()
  {
    var gitOutput = "  foo\n  bar";
    var expectedOutput = new List<Branch> { new Branch("bar"), new Branch("foo") };
    var actualOutput = Program.ExtractBranchListFromGitOutput(gitOutput);

    Assert.Equal(expectedOutput, actualOutput);
  }

  [Fact]
  public void TwoBranches_FirstCurrent()
  {
    var gitOutput = "* foo\n  bar";
    var expectedOutput = new List<Branch> { new Branch("bar"), new Branch("foo", IsCurrent: true) };
    var actualOutput = Program.ExtractBranchListFromGitOutput(gitOutput);

    Assert.Equal(expectedOutput, actualOutput);
  }

  [Fact]
  public void TwoBranches_SecondCurrent()
  {
    var gitOutput = "  foo\n* bar";
    var expectedOutput = new List<Branch> { new Branch("bar", IsCurrent: true), new Branch("foo") };
    var actualOutput = Program.ExtractBranchListFromGitOutput(gitOutput);

    Assert.Equal(expectedOutput, actualOutput);
  }

  [Fact]
  public void TwoBranches_SecondCurrent_WindowsLineEndings()
  {
    var gitOutput = "  foo\r\n* bar";
    var expectedOutput = new List<Branch> { new Branch("bar", IsCurrent: true), new Branch("foo") };
    var actualOutput = Program.ExtractBranchListFromGitOutput(gitOutput);

    Assert.Equal(expectedOutput, actualOutput);
  }

  [Fact]
  public void BranchesWithSlashes_TwoCurrent()
  {
    var gitOutput = "* foo/bar\n* foo/baz";
    var expectedOutput = new List<Branch> { new Branch("foo/bar", IsCurrent: true), new Branch("foo/baz", IsCurrent: true) };
    var actualOutput = Program.ExtractBranchListFromGitOutput(gitOutput);

    Assert.Equal(expectedOutput, actualOutput);
  }

  [Fact]
  public void Remotes_Folders_Duplicates()
  {
    var gitLines = new List<string>
    {
      "  master",
      "* secondary",
      "  remotes/origin/HEAD -> origin/master",
      "  remotes/origin/secondary",
      "  remotes/origin/foo",
      "  foo",
      "  remotes/origin/bar",
      "  remotes/another/bar",
      "  bar",
      "  folder/one",
      "  remotes/origin/folder/one",
      "  remotes/origin/folder/two"
    };
    var gitOutput = string.Join('\n', gitLines);

    var expectedOutput = new List<Branch>
    {
      new Branch("bar"),
      new Branch("folder/one"),
      new Branch("folder/two", IsRemote: true),
      new Branch("foo"),
      new Branch("HEAD", IsRemote: true),
      new Branch("master"),
      new Branch("secondary", IsCurrent: true),
    };

    var actualOutput = Program.ExtractBranchListFromGitOutput(gitOutput);

    Assert.Equal(expectedOutput, actualOutput);
  }
}

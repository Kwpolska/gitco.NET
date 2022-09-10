/*
 * gitco.NET
 * Copyright © 2014-2022, Chris Warrick. All rights reserved.
 * Licensed under the 3-clause BSD license. See /LICENSE for details.
 */

namespace gitco.NET.Tests;

public class FilterAndNumberBranchesTests
{
  [Fact]
  public void SingleBranch_NoFilter()
  {
    var branches = new List<Branch> { new Branch("a") };
    var expectedOutput = new List<BranchDisplay> { new BranchDisplay("1. ", "a") };
    var actualOutput = Program.FilterAndNumberBranches(branches, null);

    Assert.Equal(expectedOutput, actualOutput);
  }

  [Fact]
  public void TwoBranches_NoFilter()
  {
    var branches = new List<Branch> { new Branch("aa"), new Branch("ab") };
    var expectedOutput = new List<BranchDisplay> { new BranchDisplay("1. ", "aa"), new BranchDisplay("2. ", "ab") };
    var actualOutput = Program.FilterAndNumberBranches(branches, null);

    Assert.Equal(expectedOutput, actualOutput);
  }

  [Fact]
  public void TwoBranches_FilterBoth()
  {
    var branches = new List<Branch> { new Branch("aa"), new Branch("ab") };
    var expectedOutput = new List<BranchDisplay> { new BranchDisplay("1. ", "aa"), new BranchDisplay("2. ", "ab") };
    var actualOutput = Program.FilterAndNumberBranches(branches, "a");

    Assert.Equal(expectedOutput, actualOutput);
  }

  [Fact]
  public void TwoBranches_FilterOne_NumbersNotChanged()
  {
    var branches = new List<Branch> { new Branch("aa"), new Branch("ab") };
    var expectedOutput = new List<BranchDisplay> { new BranchDisplay("2. ", "ab") };
    var actualOutput = Program.FilterAndNumberBranches(branches, "b");

    Assert.Equal(expectedOutput, actualOutput);
  }

  [Fact]
  public void TwoBranches_FilterNone()
  {
    var branches = new List<Branch> { new Branch("aa"), new Branch("ab") };
    var expectedOutput = new List<BranchDisplay>();
    var actualOutput = Program.FilterAndNumberBranches(branches, "c");

    Assert.Equal(expectedOutput, actualOutput);
  }

  [Fact]
  public void TwoBranches_FilterCaseSensitive()
  {
    var branches = new List<Branch> { new Branch("Aa"), new Branch("ab") };
    var expectedOutput = new List<BranchDisplay> { new BranchDisplay("1. ", "Aa") };
    var actualOutput = Program.FilterAndNumberBranches(branches, "A");

    Assert.Equal(expectedOutput, actualOutput);
  }

  [Fact]
  public void NineBranches_NoPadding()
  {
    var branches = "abcdefghi".Select(b => new Branch(b.ToString())).ToList();
    var expectedOutput = new List<BranchDisplay> {
      new BranchDisplay("1. ", "a"),
      new BranchDisplay("2. ", "b"),
      new BranchDisplay("3. ", "c"),
      new BranchDisplay("4. ", "d"),
      new BranchDisplay("5. ", "e"),
      new BranchDisplay("6. ", "f"),
      new BranchDisplay("7. ", "g"),
      new BranchDisplay("8. ", "h"),
      new BranchDisplay("9. ", "i"),
    };

    var actualOutput = Program.FilterAndNumberBranches(branches, null);
    Assert.Equal(expectedOutput, actualOutput);
  }

  [Fact]
  public void TenBranches_HasPadding()
  {
    var branches = "abcdefghij".Select(b => new Branch(b.ToString())).ToList();
    var expectedOutput = new List<BranchDisplay> {
      new BranchDisplay(" 1. ", "a"),
      new BranchDisplay(" 2. ", "b"),
      new BranchDisplay(" 3. ", "c"),
      new BranchDisplay(" 4. ", "d"),
      new BranchDisplay(" 5. ", "e"),
      new BranchDisplay(" 6. ", "f"),
      new BranchDisplay(" 7. ", "g"),
      new BranchDisplay(" 8. ", "h"),
      new BranchDisplay(" 9. ", "i"),
      new BranchDisplay("10. ", "j"),
    };

    var actualOutput = Program.FilterAndNumberBranches(branches, null);
    Assert.Equal(expectedOutput, actualOutput);
  }

  [Fact]
  public void TenBranches_Filter_KeepsPadding()
  {
    var branches = "abcdefghij".Select(b => new Branch(b.ToString())).ToList();
    var expectedOutput = new List<BranchDisplay> {
      new BranchDisplay(" 6. ", "f"),
    };

    var actualOutput = Program.FilterAndNumberBranches(branches, "f");
    Assert.Equal(expectedOutput, actualOutput);
  }

  [Fact]
  public void HundredBranches_LargerPadding()
  {
    var branches = Enumerable.Range(0, 100).Select(b => new Branch(b.ToString())).ToList();
    var actualOutput = Program.FilterAndNumberBranches(branches, null).ToList();

    Assert.Equal("  1. ", actualOutput[0].Number);
    Assert.Equal(" 10. ", actualOutput[9].Number);
    Assert.Equal("100. ", actualOutput[99].Number);
  }

  [Fact]
  public void DuplicatesIn_DuplicatesOut()
  {
    var branches = new List<Branch> { new Branch("a"), new Branch("b"), new Branch("a"), new Branch("b") };
    var expectedOutput = new List<BranchDisplay>
    {
      new BranchDisplay("1. ", "a"),
      new BranchDisplay("3. ", "a")
    };

    var actualOutput = Program.FilterAndNumberBranches(branches, "a").ToList();
    Assert.Equal(expectedOutput, actualOutput);
  }

  [Fact]
  public void PreservesAttributes()
  {
    var branches = new List<Branch>
    {
      new Branch("ax", IsRemote: false, IsCurrent: false),
      new Branch("ay", IsRemote: false, IsCurrent: false),
      new Branch("by", IsRemote: false, IsCurrent: true),
      new Branch("by", IsRemote: false, IsCurrent: true),
      new Branch("bx", IsRemote: false, IsCurrent: true),
      new Branch("cx", IsRemote: true, IsCurrent: false),
      new Branch("dx", IsRemote: true, IsCurrent: true)
    };

    var expectedOutput = new List<BranchDisplay>
    {
      new BranchDisplay("1. ", "ax", IsRemote: false, IsCurrent: false),
      new BranchDisplay("5. ", "bx", IsRemote: false, IsCurrent: true),
      new BranchDisplay("6. ", "cx", IsRemote: true, IsCurrent: false),
      new BranchDisplay("7. ", "dx", IsRemote: true, IsCurrent: true)
    };

    var actualOutput = Program.FilterAndNumberBranches(branches, "x").ToList();
    Assert.Equal(expectedOutput, actualOutput);
  }
}

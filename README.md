Gitco.NET
=========

This is a small .NET console app for checking out a Git branch without
memorizing branch names, or copy-pasting from `git branch`.

This app works on Windows, Linux, and macOS. Executables for all three platforms
can be found in [GitHub Releases](https://github.com/Kwpolska/gitco/releases).

Linux users might also want to check out the original
[gitco Ruby script](https://github.com/Kwpolska/gitco) (but the .NET version
works too, and is probably nicer/faster/more resilient).

![gitco](https://github.com/Kwpolska/gitco.NET/raw/master/gitco.png)

Configuration
-------------

Gitco.NET can be configured via environment variables:

* `GITCO_QUICK_BRANCH_KEY` — key that can be used to quickly choose a branch, defaults to `M`
* `GITCO_QUICK_BRANCH_NAME` — the branch that can be quickly chosen, defaults to `master`

License
-------

Copyright © 2014-2023, Chris Warrick.
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are
met:

1. Redistributions of source code must retain the above copyright
   notice, this list of conditions, and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright
   notice, this list of conditions, and the following disclaimer in the
   documentation and/or other materials provided with the distribution.

3. Neither the name of the author of this software nor the names of
   contributors to this software may be used to endorse or promote
   products derived from this software without specific prior written
   consent.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
"AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT
OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

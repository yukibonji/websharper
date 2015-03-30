// $begin{copyright}
//
// This file is part of WebSharper
//
// Copyright (c) 2008-2014 IntelliFactory
//
// Licensed under the Apache License, Version 2.0 (the "License"); you
// may not use this file except in compliance with the License.  You may
// obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
// implied.  See the License for the specific language governing
// permissions and limitations under the License.
//
// $end{copyright}

namespace WebSharper.MSBuild

open Microsoft.Build.Utilities

type internal CompilerInput =
    {
        AssemblyFile : string
        DocumentationFile : option<string>
        EmbeddedResources : list<string>
        KeyOriginatorFile : string
        ProjectDir : string
        References : list<string>
        RunInterfaceGenerator : bool
        IncludeSourceMap : bool
    }

[<Sealed>]
type internal CompilerMessage =
    member SendTo : TaskLoggingHelper -> unit

type internal CompilerOutput =
    {
        Messages : CompilerMessage []
        Ok : bool
    }

module CompilerUtility =
    val Compile : CompilerInput -> CompilerOutput
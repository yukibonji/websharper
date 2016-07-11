// $begin{copyright}
//
// This file is part of WebSharper
//
// Copyright (c) 2008-2016 IntelliFactory
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

namespace WebSharper.Collections

open System.Collections
open System.Collections.Generic

open WebSharper
open WebSharper.JavaScript

[<AutoOpen>]
module private HashSetUtil =
    [<Direct "var r=[]; for(var k in $o) { r.push.apply(r, $o[k]) }; return r">]
    let concat (o: Array<Array<'T>>) = X<Array<'T>>
    
open DictionaryUtil

[<Proxy(typeof<HashSet<_>>)>]
[<Name "HashSet">]
type internal HashSetProxy<'T when 'T : equality>

    private (init   : seq<'T>,
             equals : 'T -> 'T -> bool,
             hash   : 'T -> int) =

        let mutable data  = Array<Array<'T>>()
        let mutable count = 0

        let arrContains (item: 'T) (arr: Array<'T>)  =
            let mutable c = true
            let mutable i = 0
            let l = arr.Length
            while c && i < l do
                if equals arr.[i] item then
                    c <- false
                else
                    i <- i + 1
            not c

        let arrRemove (item: 'T) (arr: Array<'T>)  =
            let mutable c = true
            let mutable i = 0
            let l = arr.Length
            while c && i < l do
                if equals arr.[i] item then
                    arr.Splice(i, 1) |> ignore
                    c <- false
                else
                    i <- i + 1
            not c

        let add (item: 'T) =
            let h = hash item
            let arr = data.[h]
            if arr ==. null then
                data.[h] <- As [| item |]
                count <- count + 1
                true
            else
                if arrContains item arr then false else    
                    arr.Push item |> ignore
                    count <- count + 1
                    true

        do for x in init do add x |> ignore

        new () = HashSetProxy<'T>(Seq.empty, (=), hash)

        new (init: seq<'T>) = new HashSetProxy<'T>(init, (=), hash)

        new (comparer: IEqualityComparer<'T>) =
            new HashSetProxy<'T>(Seq.empty, equals comparer, getHashCode comparer)

        new (init: seq<'T>, comparer: IEqualityComparer<'T>) =
            new HashSetProxy<'T>(init, equals comparer, getHashCode comparer)

        member this.Add(item: 'T) = add item

        member this.Clear() =
            data <- Array()
            count <- 0

        member x.Contains(item: 'T) =
            let arr = data.[hash item]
            if arr ==. null then false else arrContains item arr

        member x.CopyTo(arr: 'T[]) =
            let mutable i = 0
            let all = concat data 
            for i = 0 to all.Length - 1 do 
                arr.[i] <- all.[i]

        member x.Count = count

        member x.ExceptWith(xs: seq<'T>) =
            for item in xs do
                x.Remove(item) |> ignore

        [<Inline>]
        member this.GetEnumerator() =
           (As<seq<'T>>(concat data)).GetEnumerator()

        interface IEnumerable with
            member this.GetEnumerator() = this.GetEnumerator() :> _
        
        interface IEnumerable<'T> with
            member this.GetEnumerator() = this.GetEnumerator()

        // TODO: optimize methods by checking if other collection
        // is a HashSet with the same IEqualityComparer
        
        member x.IntersectWith(xs: seq<'T>) =
            let other = HashSetProxy(xs, equals, hash) 
            let all = concat data
            for i = 0 to all.Length - 1 do
                let item = all.[i]
                if other.Contains(item) |> not then
                    x.Remove(item) |> ignore

        member x.IsProperSubsetOf(xs: seq<'T>) =
            let other = xs |> Array.ofSeq
            count < other.Length && x.IsSubsetOf(other)

        member x.IsProperSupersetOf(xs: seq<'T>) =
            let other = xs |> Array.ofSeq
            count > other.Length && x.IsSupersetOf(other)

        member x.IsSubsetOf(xs: seq<'T>) =
            let other = HashSetProxy(xs, equals, hash)
            As<_[]>(concat data) |> Array.forall other.Contains

        member x.IsSupersetOf(xs: seq<'T>) =
            xs |> Seq.forall x.Contains

        member x.Overlaps(xs: seq<'T>) =
            xs |> Seq.exists x.Contains

        member x.Remove(item: 'T) =
            let h = hash item
            let arr = data.[h]
            if arr ==. null then false else
                if arrRemove item arr then
                    count <- count - 1
                    true
                else false

        member x.RemoveWhere(cond: System.Predicate<'T>) =
            let all = concat data
            for i = 0 to all.Length - 1 do
                let item = all.[i]
                if cond.Invoke item then
                    x.Remove(item) |> ignore

        member x.SetEquals(xs: seq<'T>) =
            let other = HashSetProxy(xs, equals, hash)
            x.Count = other.Count && x.IsSupersetOf(other)

        member x.SymmetricExceptWith(xs: seq<'T>) =
            for item in xs do
                if x.Contains item then
                    x.Remove(item) |> ignore
                else
                    x.Add(item) |> ignore

        member x.UnionWith(xs: seq<'T>) =
            for item in xs do
                x.Add(item) |> ignore

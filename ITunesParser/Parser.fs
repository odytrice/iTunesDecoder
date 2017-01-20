﻿module ITunesParser.Parser

open ITunesParser.Types
open System.Xml.Linq
open System

// XElement -> Value
let rec toValue (element : XElement) = 
    match element.Name.LocalName with
    | "integer" -> Value.Integer (int64 (element.Value))
    | "string" -> Value.String element.Value
    | "date" -> Value.Date(element.Value |> DateTime.Parse)
    | "data" -> Value.Data element.Value
    | "true" -> Value.Bool true
    | "false" -> Value.Bool false
    | "dict" -> element |> toDict
    | "array" -> element |> toArray
    | s -> failwith ("Unknown Value: " + s)

// XElement -> Value
and toDict dictElement = 
    // (string * Value) list -> XElement list -> (string * Value) list
    let rec processPairs result (elements : XElement list) = 
        match elements with
        | x :: y :: rest -> processPairs ((x.Value, toValue y) :: result) rest
        | [ _ ] | [] -> result

    Dict(processPairs [] (dictElement.Elements() |> List.ofSeq))

// XElement -> value
and toArray arrayElement = 
    // Value list -> XElement list -> Value list
    let rec processSeq result (elements : XElement list) = 
        match elements with
        | x :: rest -> processSeq ((toValue x) :: result) rest
        | [] -> result

    Value.Array(processSeq [] (arrayElement.Elements() |> List.ofSeq))

let parsePListXML (xml:string) =
    let plist = 
        XDocument.Parse(xml)
        |> fun doc -> doc.Elements() 
        |> Seq.head

    //Parse the First Dict
    plist.Elements() 
    |> Seq.head
    |> toValue

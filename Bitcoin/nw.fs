open System.Net
open System.Text
open Akka
open Akka.FSharp
open Akka.Actor
open Akka.IO
open System

[<EntryPoint>]
let main argv =
    printfn "%s" "hello"
    let byteAdress = Encoding.ASCII.GetBytes("54.65.85.42:9090") 
    let objs = IPAddress(byteAdress)
    Console.WriteLine(objs)
    // let server = Dns.GetHostEntry(Dns.getHostName)
    // printfn "Hostname : %s" server
    0

#r "nuget: Akka.FSharp"
#r "nuget: Akka.Remote"
#load "Util.fsx"
#time "on"

open System
open System.Threading
open Akka.Actor
open Akka.Configuration
open Akka.FSharp
open Util

let zeros = fsi.CommandLineArgs.[1]|>int;
let workerCount = 8
let totalStrings = 100000
let eachSideDuty = totalStrings/2
let eachWorkerEffort = eachSideDuty / workerCount
// printfn "Worker effort : %d" eachWorkerEffort
let mutable clientSideCompleted = false;
let mutable serverSideCompleted = false;
// To keep a count of when all the workers have finished and terminate the program
let mutable workersFinished = 0
let mutable totalCoinsFound =0;


let clientConfiguration = 
    ConfigurationFactory.ParseString(   
        @"akka{
            actor.provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""    
            remote{
                helios.tcp{
                    port:8202
                    hostname:localhost
                }
            }
        }")

let system = System.create "Client" clientConfiguration


//Initializing message types
type MessageFormat = 
    |CreateWorkers of int
    |ReceiveMessageFromWorker of int*string*string
    |StartYourJob of int*int*int
    |Finished of int*string
    // |Finish of string

//Creating working actors
let worker (mailbox:Actor<_>)=
    let rec loop() = actor{
        let! msg = mailbox.Receive()
        let sender = mailbox.Sender()

        match msg with

        |StartYourJob(ID,capacity,startCharOffset) -> 
            // printfn "Worker %d Initialised\n" ID

            //String to store all the encrypted strings and their corresponding keys
            let mutable str = ""
            let mutable invalidCounter = 0
            let mutable validCounter = 0
            //Perform the encryption
            for i = 1 to capacity do
                let stringToEncrypt = stringGenerator(startCharOffset)
                let encryptedString = encrypt(stringToEncrypt)

                //Check for validation of the encryption string
                if validCoin(encryptedString,zeros) then 
                    totalCoinsFound <- totalCoinsFound + 1
                    sender<!ReceiveMessageFromWorker(ID,stringToEncrypt,encryptedString)

            sender<!Finished(ID,"Done");

        | _ -> printfn "Incorrect message"

        return! loop()
    }
    loop()


//Creating the boss

let bossActor (mailbox:Actor<_> ) = 
    let rec loop() = actor{
        let! msg = mailbox.Receive()
        
        let mutable workers =[]
        
        match msg with 
        |CreateWorkers (n)->
            workers <- [for i in 0..(workerCount-1) do 
                            yield spawn system ("Worker" + (string i)) worker]
                       
            // printfn "\nTotal number of workers %d \n" workers.Length

            let mutable characterStartOffset =0
            
            //Commanding the workers to start their jobs
            let mutable workerID = 1
            for i in 0..(workerCount-1) do //minus 1 because zero index based
                
                workers.Item(i|>int)<!StartYourJob(workerID,eachWorkerEffort,characterStartOffset)
                characterStartOffset<- characterStartOffset + 3
                workerID <- workerID + 1
            
            // printfn "welcome to assigning task to %d people" n

        |ReceiveMessageFromWorker (workerID,generatedString,encryptedString) ->
            printfn "WorkerID : %d \nInput: %s \nOutput: %s" workerID generatedString encryptedString
            
        |Finished(workerID,message)->
            // printfn "Received %s \nOkay, good job worker %d" message workerID
            
            //Everyone has finished processing, terminate the process
            workersFinished <- workersFinished + 1
            // printfn "finished workers %d" workersFinished
            if workersFinished = workerCount then 
                // mailbox.Context.System.Terminate() |>ignore
                clientSideCompleted <- true
                printfn "Client side has finished thier job"
                printfn "Total coins found by CLIENT = %d" totalCoinsFound

        | _ -> printfn "Incorrect message"
        

        return! loop()
    }
    loop()

let clientBoss = spawn system "bossActor" bossActor







// -----------------------Connecting to remote actor system----------------------------

let serverAddress = fsi.CommandLineArgs.[2]
let serverPort = fsi.CommandLineArgs.[3]

// let serverAddress = "192.168.251.176"
// let serverPort = "8201"

let connectionAddress = "akka.tcp://RemoteSystem@" + serverAddress + ":" + serverPort + "/user/server" 
printfn "%s" connectionAddress

// Creating a client actor
let client (mailbox:Actor<_>) = 
    let rec loop() = actor{
        let! message = mailbox.Receive()
        let sender = mailbox.Sender()
        let incoming = message|>string
        
        if incoming = "godspeed" then

            //Assign task for remote actor
            let remoteActor = system.ActorSelection(connectionAddress)
            let taskForRemoteActor = "Perform "+(string eachSideDuty)+","+ (string zeros) + "zeros"
            remoteActor<!taskForRemoteActor

            //Assign task for local actors
            clientBoss<!CreateWorkers(workerCount)

        else if incoming = "ServerSide Completed" then 
                serverSideCompleted <- true
        else if incoming.[0..4] = "Found" then
            printfn "Total coins found by SERVER: %s \n\n" incoming.[5..]

        if (clientSideCompleted && serverSideCompleted) then
            system.Terminate() |>ignore
            
        return! loop()
    }
    loop()

let clientRef = spawn system "client" client

clientRef<!"godspeed"

while (not clientSideCompleted && not serverSideCompleted) do

system.WhenTerminated.Wait()

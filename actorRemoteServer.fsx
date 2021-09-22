#r "nuget: Akka.FSharp"
#r "nuget: Akka.Remote"

#load "Util.fsx"

open System
open System.Threading
open Akka.Actor
open Akka.Configuration
open Akka.FSharp
open Util


let workerCount = 8
let mutable returnRef = null
// printfn "Worker effort : %d" eachWorkerEffort
let mutable totalCoinsFound = 0
// To keep a count of when all the workers have finished and terminate the program
let mutable workersFinished = 0

//Initializing message types
type MessageFormat = 
    |CreateWorkers of int*int
    |ReceiveMessageFromWorker of int*string*string
    |StartYourJob of int*int*int*int
    |Finished of int*string
    // |Finish of string

let serverConfiguration = 
    ConfigurationFactory.ParseString(
        @"akka{
            actor{
                provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""    
                debug : {
                    receive : on
                    autoreceive : on
                    lifecycle : on
                    event-stream : on
                    unhandled : on
                }
            }
            remote{
                helios.tcp{
                    port:8201
                    hostname:localhost
                }
            }
        }")
// Server actor with name RemoteSystem to be referrenced by client    
let system =  System.create "RemoteSystem" (serverConfiguration)
 
//Creating working actors
let worker (mailbox:Actor<_>)=
    let rec loop() = actor{
        let! msg = mailbox.Receive()
        let sender = mailbox.Sender()

        match msg with

        |StartYourJob(ID,capacity,startCharOffset,z) -> 
     
            //Perform the encryption
            for i = 1 to capacity do
                let stringToEncrypt = stringGenerator(startCharOffset)
                let encryptedString = encrypt(stringToEncrypt)

                //Check for validation of the encryption string
                if validCoin(encryptedString,z) then 
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
        |CreateWorkers (iterations,z)->
            workers <- [for i in 0..(workerCount-1) do 
                            yield spawn system ("Worker" + (string i)) worker]

            let eachWorkerEffort = iterations/workerCount 
            // printfn "\nTotal number of workers %d \n" workers.Length

            let mutable characterStartOffset =0
            
            //Commanding the workers to start their jobs
            let mutable workerID = 1
            for i in 0..(workerCount-1) do //minus 1 because zero index based
                
                workers.Item(i|>int)<!StartYourJob(workerID,eachWorkerEffort,characterStartOffset,z)
                characterStartOffset<- characterStartOffset + 5
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
                returnRef <!"ServerSide Completed"
                printfn "Servers have finished their job"
                printfn "Total coins found = %d" totalCoinsFound

        | _ -> printfn "Incorrect message"
        

        return! loop()
    }
    loop()

let serverBoss = spawn system "bossActor" bossActor






// Connection link between the 2 machines
let serverTunnel (mailbox:Actor<_>)=
    let rec loop() = actor{
        let! msg = mailbox.Receive()
        let sender = mailbox.Sender()
        
        
        let inputJob = (string msg).Split(",")

        let command = inputJob.[0].[0..6]
        if command = "Perform" then
            let iterations = (inputJob.[0]).[8..] |>int 
            let requiredZeros = (inputJob.[1]).Substring(0,1) |>int
            
            printfn "Need to perform  %d iterations and %d zeros" iterations requiredZeros
            serverBoss<!CreateWorkers(iterations,requiredZeros)
        returnRef <- sender
        // sender<!"ServerSide Completed"


        return! loop()
    }
    loop()



//Reference for the connection link actor
let tunnel = spawn system "server" serverTunnel


system.WhenTerminated.Wait()
// tunnel<!"hjello"